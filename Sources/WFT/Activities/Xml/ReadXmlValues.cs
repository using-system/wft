using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities.Xml
{
    /// <summary>
    /// Read xml and return collection (IEnumerable'String') values with a xpath expression
    /// </summary>
    [Description("Read xml and return collection (IEnumerable'String') values with a xpath expression")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.XmlToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/XmlDesigner.bmp")]
    public sealed class ReadXmlValues : ReadXmlValues<IEnumerable<string>>
    {

    }

    /// <summary>
    /// Read xml and return collection values with a xpath expression
    /// </summary>
    /// <typeparam name="Collection">Type of the values collection. The collection must implement IEnumerable'String'</typeparam>
    [Description("Read xml and return collection values with a xpath expression")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.XmlToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/XmlDesigner.bmp")]
    public class ReadXmlValues<Collection> : CodeActivity<Collection>
        where Collection : class, IEnumerable<string>
    {
        /// <summary>
        /// Xml content
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Xml content")]
        public InArgument<string> Xml { get; set; }

        /// <summary>
        /// XPath Expression
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("XPath Expression")]
        public InArgument<string> XPath { get; set; }

        /// <summary>
        /// XPath expression for the node to iterate (optionnal)
        /// </summary>
        [Description("XPath expression for the node to iterate")]
        public InArgument<string> XPathIterationNode { get; set; }

        protected override Collection Execute(CodeActivityContext context)
        {
            return Xml.Get(context)
                .GetXmlNodes(XPath.Get(context), XPathIterationNode.Get(context)) as Collection;
        }
    }
}
