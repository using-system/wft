using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WFT.Activities.Designers;

namespace WFT.Activities.Xml
{
    /// <summary>
    /// Format xml string with indentation
    /// </summary>
    [Description("Format xml string with indentation")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.XmlToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/XmlDesigner.bmp")]
    public class FormatXml : CodeActivity<string>
    {
        /// <summary>
        /// Xml source to format
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Xml source to format")]
        public InArgument<string> XmlSource { get; set; }

        /// <summary>
        /// Load Options
        /// </summary>
        [Description("Load Options")]
        public LoadOptions LoadOptions { get; set; }

        protected override string Execute(CodeActivityContext context)
        {
            return XDocument.Parse(XmlSource.Get(context), LoadOptions).ToString();
        }
    }
}
