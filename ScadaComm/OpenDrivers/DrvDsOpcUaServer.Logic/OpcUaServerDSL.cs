﻿/*
 * Copyright 2021 Mikhail Shiryaev
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : DrvDsOpcUaServer
 * Summary  : Implements the data source logic
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2021
 * Modified : 2021
 */

using Opc.Ua;
using Opc.Ua.Configuration;
using Opc.Ua.Server;
using Scada.Comm.Config;
using Scada.Comm.DataSources;
using Scada.Comm.Devices;
using Scada.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Scada.Comm.Drivers.DrvDsOpcUaServer.Logic
{
    /// <summary>
    /// Implements the data source logic.
    /// <para>Реализует логику источника данных.</para>
    /// </summary>
    internal class OpcUaServerDSL : DataSourceLogic
    {
        private readonly OpcUaServerDSO options; // the data source options
        private readonly ILog dsLog;             // the data source log

        private ApplicationInstance opcApp;
        private CustomServer opcServer;


        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public OpcUaServerDSL(ICommContext commContext, DataSourceConfig dataSourceConfig)
            : base(commContext, dataSourceConfig)
        {
            options = new OpcUaServerDSO(dataSourceConfig.CustomOptions);
            dsLog = CreateLog(DriverUtils.DriverCode);
        }


        /// <summary>
        /// Prepares OPC UA server for operating.
        /// </summary>
        private async Task PrepareOpcServer()
        {
            // create OPC application instance
            opcApp = new ApplicationInstance
            {
                ApplicationName = DataSourceConfig.Name,
                ApplicationType = ApplicationType.Server,
                ConfigSectionName = "Scada.Comm.Drivers.DrvDsOpcUaServer"
            };

            // load the application configuration
            WriteConfigFile(out string configFileName);
            ApplicationConfiguration opcConfig = await opcApp.LoadApplicationConfiguration(configFileName, false);

            // check the application certificate
            bool haveAppCertificate = await opcApp.CheckApplicationInstanceCertificate(false, 
                CertificateFactory.DefaultKeySize, CertificateFactory.DefaultLifeTime);

            if (!haveAppCertificate)
            {
                throw new ScadaException(Locale.IsRussian ?
                    "Сертификат экземпляра приложения недействителен!" :
                    "Application instance certificate invalid!");
            }

            if (!opcConfig.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                opcConfig.CertificateValidator.CertificateValidation += CertificateValidator_CertificateValidation;

            // create OPC server
            opcServer = new CustomServer(CommContext, options, dsLog);
        }

        /// <summary>
        /// Writes an OPC UA configuration file depending on operating system.
        /// </summary>
        private void WriteConfigFile(out string configFileName)
        {
            string shortFileName = string.IsNullOrEmpty(options.ConfigFileName) ?
                DriverUtils.DefaultConfigFileName : options.ConfigFileName;
            configFileName = Path.Combine(CommContext.AppDirs.ConfigDir, shortFileName);

            if (!File.Exists(configFileName))
            {
                string suffix = ScadaUtils.IsRunningOnWin ? "Win" : "Linux";
                string resourceName = $"Scada.Comm.Drivers.DrvDsOpcUaServer.Logic.Config.DrvDsOpcUaServer.{suffix}.xml";
                string fileContents;

                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        fileContents = reader.ReadToEnd();
                    }
                }

                File.WriteAllText(configFileName, fileContents, Encoding.UTF8);
            }
        }

        /// <summary>
        /// Starts OPC UA server.
        /// </summary>
        private async Task StartOpcServer()
        {
            await opcApp.Start(opcServer);

            StringBuilder sbStartInfo = new StringBuilder("OPC UA server started");
            EndpointDescriptionCollection endpoints = opcServer.GetEndpoints();

            if (endpoints.Count > 0)
            {
                // print endpoint info
                foreach (string endpointUrl in endpoints.Select(e => e.EndpointUrl).Distinct())
                {
                    sbStartInfo.AppendLine().Append("    ").Append(endpointUrl);
                }
            }
            else
            {
                sbStartInfo.AppendLine().Append("    No endpoints");
            }

            dsLog.WriteAction(sbStartInfo.ToString());

            // add event handlers
            ISessionManager sessionManager = opcServer.CurrentInstance.SessionManager;
            sessionManager.SessionActivated += SessionManager_SessionEvent;
            sessionManager.SessionClosing += SessionManager_SessionEvent;
            sessionManager.SessionCreated += SessionManager_SessionEvent;

            ISubscriptionManager subscriptionManager = opcServer.CurrentInstance.SubscriptionManager;
            subscriptionManager.SubscriptionCreated += SubscriptionManager_SubscriptionEvent;
            subscriptionManager.SubscriptionDeleted += SubscriptionManager_SubscriptionEvent;
        }

        /// <summary>
        /// Stops and disposes OPC UA server.
        /// </summary>
        private void StopOpcServer()
        {
            if (opcServer != null)
            {
                opcServer.Stop();
                dsLog.WriteAction("OPC UA server stopped");
            }
        }

        /// <summary>
        /// Validates the certificate.
        /// </summary>
        private void CertificateValidator_CertificateValidation(CertificateValidator sender, 
            CertificateValidationEventArgs e)
        {
            if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
            {
                e.Accept = options.AutoAccept;

                if (options.AutoAccept)
                {
                    dsLog.WriteAction(Locale.IsRussian ?
                        "Принятый сертификат: {0}" :
                        "Accepted certificate: {0}", e.Certificate.Subject);
                }
                else
                {
                    dsLog.WriteError(Locale.IsRussian ?
                        "Отклоненный сертификат: {0}" :
                        "Rejected certificate: {0}", e.Certificate.Subject);
                }
            }
        }

        /// <summary>
        /// Logs the session event.
        /// </summary>
        private void SessionManager_SessionEvent(Session session, SessionEventReason reason)
        {
            dsLog.WriteAction("{0} {1}", session.SessionDiagnostics.SessionName, reason.ToString().ToLowerInvariant());
        }

        /// <summary>
        /// Logs the subscription event.
        /// </summary>
        private void SubscriptionManager_SubscriptionEvent(Subscription subscription, bool deleted)
        {
            if (deleted)
            {
                dsLog.WriteAction(Locale.IsRussian ?
                    "Подписка с ид. {0} удалена" :
                    "Subscription with ID {0} deleted", subscription.Id);
            }
            else
            {
                dsLog.WriteAction(Locale.IsRussian ?
                    "Подписка с ид. {0} создана" :
                    "Subscription with ID {0} created", subscription.Id);
            }
        }


        /// <summary>
        /// Makes the data source ready for operating.
        /// </summary>
        public override void MakeReady()
        {
            dsLog.WriteBreak();
            PrepareOpcServer().Wait();
        }

        /// <summary>
        /// Starts the data source.
        /// </summary>
        public override void Start()
        {
            try
            {
                StartOpcServer().Wait();
            }
            catch
            {
                IsReady = false;
                throw;
            }
        }

        /// <summary>
        /// Closes the data source.
        /// </summary>
        public override void Close()
        {
            StopOpcServer();
            dsLog.WriteBreak();
        }

        /// <summary>
        /// Writes the slice of the current data.
        /// </summary>
        public override void WriteCurrentData(DeviceSlice deviceSlice)
        {
            opcServer?.NodeManager?.WriteCurrentData(deviceSlice);
        }
    }
}
