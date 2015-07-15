using System.Activities;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities.Xml
{
    /// <summary>
    /// Transform a xml content with a xsl stylesheet
    /// </summary>
    [Description("Transform a xml content with a xsl stylesheet")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.XmlToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/XmlDesigner.bmp")]
    public sealed class XslTransform : CodeActivity<string>
    {
        /// <summary>
        /// Xml content to transform
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Xml content to transform")]
        public InArgument<string> Xml { get; set; }

        /// <summary>
        /// Xsl stylesheet
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Xsl stylesheet")]
        public InArgument<string> Xsl { get; set; }

        protected override string Execute(CodeActivityContext context)
        {
            return Xml.Get(context)
                .TransformXml(Xsl.Get(context));
        }
    }
}
