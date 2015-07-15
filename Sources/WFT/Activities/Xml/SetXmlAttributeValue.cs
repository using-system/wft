using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities.Xml
{
    /// <summary>
    /// Set value to an attribute
    /// </summary>
    [Description("Set value to an attribute")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.XmlToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/XmlDesigner.bmp")]
    public sealed class SetXmlAttributeValue : BaseXmlTransformation
    {
        /// <summary>
        /// Attribute value
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Attribute value")]
        public InArgument<string> Value { get; set; }
        
        protected override string Transform(System.Activities.NativeActivityContext context, string xmlSource)
        {
            return xmlSource.SetXmlAttributeValue(XPath.Get(context), new List<string>() { Value.Get(context) });
        }
    }
}
