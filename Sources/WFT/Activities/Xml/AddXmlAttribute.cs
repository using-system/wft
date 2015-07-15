using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities.Xml
{
    /// <summary>
    /// Add a xml attribute with single value
    /// </summary>
    [Description("Add a xml attribute with single value")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.XmlToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/XmlDesigner.bmp")]
    public sealed class AddXmlAttribute : BaseXmlTransformation
    {
        /// <summary>
        /// Attribute name to add
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Attribute name to add")]
        public InArgument<string> AttributeName { get; set; }

        /// <summary>
        /// Attribute value
        /// </summary>
        [Description("Attribute value")]
        public InArgument<string> Value { get; set; }

        protected override string Transform(NativeActivityContext context, string xmlSource)
        {
            string value = (Value != null) ? Value.Get(context) : String.Empty;

            return xmlSource.AddXmlAttribute(
                XPath.Get(context),
                AttributeName.Get(context),
                new List<string>() { value });
        }

    }
}
