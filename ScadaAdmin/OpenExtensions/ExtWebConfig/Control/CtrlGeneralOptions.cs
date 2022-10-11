﻿using Scada.Admin.Project;
using Scada.Config;
using Scada.Web.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Scada.Admin.Extensions.ExtWebConfig.Control
{
    public partial class CtrlGeneralOptions : UserControl
    {
        private IAdminContext adminContext;      // the Administrator context
        private WebApp webApp;                   // the web application in a project


        public CtrlGeneralOptions()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Raises an OptionsChanged event.
        /// </summary>
        private void OnOptionsChanged()
        {
            OptionsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the options change.
        /// </summary>
        public event EventHandler OptionsChanged;


        /// <summary>
        /// Initializes the control.
        /// </summary>
        public void Init(IAdminContext adminContext, WebApp webApp)
        {
            this.adminContext = adminContext ?? throw new ArgumentNullException(nameof(adminContext));
            this.webApp = webApp ?? throw new ArgumentNullException(nameof(webApp));
        }

        /// <summary>
        /// Sets the controls according to the options.
        /// </summary>
        public void OptionsToControls(GeneralOptions generalOptions)
        {
            ArgumentNullException.ThrowIfNull(generalOptions, nameof(generalOptions));
        }

        /// <summary>
        /// Sets the configuration according to the controls.
        /// </summary>
        public void ControlsToOptions(GeneralOptions generalOptions)
        {
            ArgumentNullException.ThrowIfNull(generalOptions, nameof(generalOptions));
        }
    }
}
