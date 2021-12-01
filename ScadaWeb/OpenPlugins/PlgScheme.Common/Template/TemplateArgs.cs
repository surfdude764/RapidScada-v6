﻿// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Scada.Web.Plugins.PlgScheme.Template
{
    /// <summary>
    /// Represents scheme arguments in template mode.
    /// <para>Представляет аргументы схемы в режиме шаблона.</para>
    /// </summary>
    public class TemplateArgs
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public TemplateArgs()
        {
            InCnlOffset = 0;
            CtrlCnlOffset = 0;
            TitleCompID = 0;
            BindingFileName = "";
        }


        /// <summary>
        /// Gets or sets the offset of input channel numbers.
        /// </summary>
        public int InCnlOffset { get; set; }
        
        /// <summary>
        /// Gets or sets the offset of output channel numbers.
        /// </summary>
        public int CtrlCnlOffset { get; set; }

        /// <summary>
        /// Gets or sets the ID of the component that displays a scheme title.
        /// </summary>
        public int TitleCompID { get; set; }

        /// <summary>
        /// Gets or sets the name of the file that defines bindings.
        /// </summary>
        public string BindingFileName { get; set; }


        /// <summary>
        /// Initializes the template arguments.
        /// </summary>
        public void Init(SortedList<string, string> args)
        {
            InCnlOffset = args.GetValueAsInt("inCnlOffset");
            CtrlCnlOffset = args.GetValueAsInt("ctrlCnlOffset");
            TitleCompID = args.GetValueAsInt("titleCompID");
            BindingFileName = args.GetValueAsString("bindingFileName");
        }
    }
}
