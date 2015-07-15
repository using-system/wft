using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities.Xml
{
    /// <summary>
    /// Insert xml nodes
    /// </summary>
    [Description("Insert xml nodes")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.XmlToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/XmlDesigner.bmp")]
    public sealed class InsertXmlNodes : BaseXmlTransformation
    {
        public InsertXmlNodes()
        {
            InsertMode = XmlInsertNodeMode.Before;
        }

        /// <summary>
        /// Insert Mode (insert before or after)
        /// </summary>
        public XmlInsertNodeMode InsertMode { get; set; }

        /// <summary>
        /// Xml nodes to insert
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Xml nodes to insert")]
        public InArgument<List<string>> XmlNodes { get; set; }

        protected override string Transform(NativeActivityContext context, string xmlSource)
        {
            return xmlSource.SetXmlValue(
                XPath.Get(context),
                XmlNodes.Get(context),
                InsertMode == XmlInsertNodeMode.Before ? StringExtensions.XmlNodeOperation.InsertBefore : StringExtensions.XmlNodeOperation.InsertAfter);
        }
    }
}
