using System.Activities;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities.Xml
{
    /// <summary>
    /// Read xml and return a string value with a xpath expression
    /// </summary>
    [Description("Read xml and return a string value with a xpath expression")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.XmlToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/XmlDesigner.bmp")]
    public sealed class ReadXmlValue : CodeActivity<string>
    {
        /// <summary>
        /// Xml content
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Xml content")]
        public InArgument<string> Xml { get; set; }

        /// <summary>
        /// XPath Expression
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("XPath Expression")]
        public InArgument<string> XPath { get; set; }

        protected override string Execute(CodeActivityContext context)
        {
            return Xml.Get(context)
                .GetXmValue(XPath.Get(context));
        }
    }
}
