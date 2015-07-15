using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities.Xml
{
    /// <summary>
    /// Create a xml child
    /// </summary>
    [Description("Create a xml child")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.XmlToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/XmlDesigner.bmp")]
    public sealed class CreateXmlChild : BaseXmlTransformation
    {
        /// <summary>
        /// Xml child to add
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Xml child to add")]
        public InArgument<string> XmlChild { get; set; }

        protected override string Transform(NativeActivityContext context, string xmlSource)
        {
            return xmlSource.SetXmlValue(
                XPath.Get(context),
                new List<string>(){ XmlChild.Get(context) },
                StringExtensions.XmlNodeOperation.CreateChild);
        }
    }
}
