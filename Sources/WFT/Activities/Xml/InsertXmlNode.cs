using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities.Xml
{
    /// <summary>
    /// Insert a xml node
    /// </summary>
    [Description("Insert a xml node")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.XmlToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/XmlDesigner.bmp")]
    public sealed class InsertXmlNode : BaseXmlTransformation
    {
        public InsertXmlNode()
        {
            InsertMode = XmlInsertNodeMode.Before;
        }

        /// <summary>
        /// Insert Mode (insert before or after)
        /// </summary>
        public XmlInsertNodeMode InsertMode { get; set; }

        /// <summary>
        /// Xml node to insert
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Xml node to insert")]
        public InArgument<string> XmlNode { get; set; }

        protected override string Transform(NativeActivityContext context, string xmlSource)
        {
            return xmlSource.SetXmlValue(
                XPath.Get(context),
                new List<string>() { XmlNode.Get(context) },
                InsertMode == XmlInsertNodeMode.Before ? StringExtensions.XmlNodeOperation.InsertBefore : StringExtensions.XmlNodeOperation.InsertAfter);
        }
    }

    public enum XmlInsertNodeMode
    {
        Before,
        After
    }
}
