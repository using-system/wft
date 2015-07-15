using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities.Xml
{
    /// <summary>
    /// Remove xml childrens objects
    /// </summary>
    [Description("Remove xml childrens objects")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.XmlToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/XmlDesigner.bmp")]
    public sealed class RemoveXmlChildrens : BaseXmlTransformation
    {
        protected override string Transform(System.Activities.NativeActivityContext context, string xmlSource)
        {
            return xmlSource.RemoveXmlObject(XPath.Get(context), StringExtensions.XmlRemoveOperation.Childrens);
        }
    }
}
