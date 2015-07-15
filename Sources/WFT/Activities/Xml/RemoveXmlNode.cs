using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities.Xml
{
    /// <summary>
    /// Remove a xml node
    /// </summary>
    [Description("Remove a xml node")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.XmlToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/XmlDesigner.bmp")]
    public sealed class RemoveXmlNode : BaseXmlTransformation
    {
        protected override string Transform(System.Activities.NativeActivityContext context, string xmlSource)
        {
            return xmlSource.RemoveXmlObject(XPath.Get(context), StringExtensions.XmlRemoveOperation.Self);
        }
    }
}
