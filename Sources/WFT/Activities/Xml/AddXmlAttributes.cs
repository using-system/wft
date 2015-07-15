using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities.Xml
{
    /// <summary>
    /// Add a xml attribute values ​​to multiple elements
    /// </summary>
    [Description("Add a xml attribute values ​​to multiple elements")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.XmlToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/XmlDesigner.bmp")]
    public sealed class AddXmlAttributes : BaseXmlTransformation
    {
        /// <summary>
        /// Attribute name to add
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Attribute name to add")]
        public InArgument<string> AttributeName { get; set; }

        /// <summary>
        /// Attribute values
        /// </summary>
        [Description("Attribute value")]
        public InArgument<List<string>> Values { get; set; }

        protected override string Transform(NativeActivityContext context, string xmlSource)
        {
            if(Values == null)
                return xmlSource.AddXmlAttribute(
                    XPath.Get(context),
                    AttributeName.Get(context),
                    new List<string>());
            else
                return xmlSource.AddXmlAttribute(
                    XPath.Get(context),
                    AttributeName.Get(context),
                    Values.Get(context));
        }

    }
}
