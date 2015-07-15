using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities.Xml
{
    /// <summary>
    /// Set value to a xml node
    /// </summary>
    [Description("Set value to a xml node")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.XmlToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/XmlDesigner.bmp")]
    public sealed class SetXmlNodeValue : BaseXmlTransformation
    {
        /// <summary>
        /// Node value
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Node value")]
        public InArgument<string> Value { get; set; }

        protected override string Transform(System.Activities.NativeActivityContext context, string xmlSource)
        {
            return xmlSource.SetXmlValue(
                XPath.Get(context),
                new List<string>() { Value.Get(context) },
                StringExtensions.XmlNodeOperation.SetValue);
        }
    }
}
