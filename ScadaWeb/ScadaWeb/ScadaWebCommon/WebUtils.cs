﻿/*
 * Copyright 2021 Rapid Software LLC
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
 * Module   : ScadaWebCommon
 * Summary  : The class provides helper methods for the Webstation application and its plugins
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2016
 * Modified : 2021
 */

using Microsoft.AspNetCore.Html;
using Scada.Lang;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Scada.Web
{
    /// <summary>
    /// The class provides helper methods for the Webstation application and its plugins.
    /// <para>Класс, предоставляющий вспомогательные методы для приложения Вебстанция и его плагинов.</para>
    /// </summary>
    public static class WebUtils
    {
        /// <summary>
        /// The application version.
        /// </summary>
        public const string AppVersion = "6.0.0.0";
        /// <summary>
        /// The application log file name.
        /// </summary>
        public const string LogFileName = "ScadaWeb.log";


        /// <summary>
        /// Converts the phrases dictionary to a JavaScript object.
        /// </summary>
        public static HtmlString DictionaryToJs(LocaleDict dict)
        {
            StringBuilder sbJs = new();
            sbJs.AppendLine("{");

            if (dict != null)
            {
                foreach (KeyValuePair<string, string> pair in dict.Phrases)
                {
                    sbJs.Append(pair.Key)
                        .Append(": \"")
                        .Append(HttpUtility.JavaScriptStringEncode(pair.Value))
                        .AppendLine("\",");
                }
            }

            sbJs.Append('}');
            return new HtmlString(sbJs.ToString());
        }

        /// <summary>
        /// Converts the phrases dictionary to a JavaScript object.
        /// </summary>
        public static HtmlString DictionaryToJs(string dictKey)
        {
            return DictionaryToJs(Locale.GetDictionary(dictKey));
        }
    }
}
