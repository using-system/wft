using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities.Xml
{
    /// <summary>
    /// Set values to  attributes
    /// </summary>
    [Description("Set values to  attributes")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.XmlToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/XmlDesigner.bmp")]
    public sealed class SetXmlAttributeValues : BaseXmlTransformation
    {
        /// <summary>
        /// Attribute value
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Attribute values")]
        public InArgument<List<string>> Values { get; set; }

        protected override string Transform(System.Activities.NativeActivityContext context, string xmlSource)
        {
            return xmlSource.SetXmlAttributeValue(XPath.Get(context),  Values.Get(context));
        }
    }
}
