using System.Activities;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities.Xml
{
    /// <summary>
    /// Remove a xml attribute
    /// </summary>
    [Description("Remove a xml attribute")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.XmlToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/XmlDesigner.bmp")]
    public sealed class RemoveXmlAttribute : BaseXmlTransformation
    {
        protected override string Transform(NativeActivityContext context, string xmlSource)
        {
            return
                xmlSource.RemoveXmlObject(XPath.Get(context), StringExtensions.XmlRemoveOperation.Self);
        }
    }
}
