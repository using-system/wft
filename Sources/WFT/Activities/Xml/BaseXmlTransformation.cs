using System.Activities;
using System.ComponentModel;

namespace WFT.Activities.Xml
{
    /// <summary>
    /// Base Xml Transformation class
    /// </summary>
    public abstract class BaseXmlTransformation : NativeActivity<string>
    {
        /// <summary>
        /// Xml content to transform
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Xml content to transform")]
        public InArgument<string> XmlSource { get; set; }

        /// <summary>
        /// XPath expression
        /// </summary>
        [RequiredArgument]
        [Description("XPath expression")]
        public InArgument<string> XPath { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            string transformation = Transform(context, XmlSource.Get(context));

            if(Result != null)
                Result.Set(context, transformation);
        }

        protected abstract string Transform(NativeActivityContext context, string xmlSource);
    }
}
