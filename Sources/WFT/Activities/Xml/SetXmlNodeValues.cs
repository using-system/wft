using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities.Xml
{
    /// <summary>
    /// Set values to xml nodes
    /// </summary>
    [Description("Set values to xml nodes")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.XmlToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/XmlDesigner.bmp")]
    public sealed class SetXmlNodeValues : BaseXmlTransformation
    {
        /// <summary>
        /// Node values
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Node values")]
        public InArgument<List<string>> Values { get; set; }

        protected override string Transform(System.Activities.NativeActivityContext context, string xmlSource)
        {
            return xmlSource.SetXmlValue(
                XPath.Get(context),
                Values.Get(context),
                StringExtensions.XmlNodeOperation.SetValue);
        }
    }
}
