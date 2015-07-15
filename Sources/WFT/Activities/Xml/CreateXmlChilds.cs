using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities.Xml
{
    /// <summary>
    /// Create xml childs
    /// </summary>
    [Description("Create xml childs")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.XmlToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/XmlDesigner.bmp")]
    public sealed class CreateXmlChilds : BaseXmlTransformation
    {
        /// <summary>
        /// Xml childs to add
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Xml childs to add")]
        public InArgument<List<string>> XmlChilds { get; set; }

        protected override string Transform(NativeActivityContext context, string xmlSource)
        {
            return xmlSource.SetXmlValue(
                XPath.Get(context),
                XmlChilds.Get(context),
                StringExtensions.XmlNodeOperation.CreateChild);
        }
    }
}
