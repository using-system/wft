using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities.Xml
{
    /// <summary>
    /// Replace an xml node
    /// </summary>
    [Description("Replace an xml node")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.XmlToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/XmlDesigner.bmp")]
    public sealed class ReplaceXmlNode : BaseXmlTransformation
    {
        /// <summary>
        /// New xml node content
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("New xml node content")]
        public InArgument<string> NewXmlNode { get; set; }

        protected override string Transform(System.Activities.NativeActivityContext context, string xmlSource)
        {
            return xmlSource.SetXmlValue(
                XPath.Get(context),
                new List<string>() { NewXmlNode.Get(context) },
                StringExtensions.XmlNodeOperation.Replace);
        }
    }
}
