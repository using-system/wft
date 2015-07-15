using System;
using System.Activities;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities
{
    /// <summary>
    /// Execute a lambda expression with result of type Result and with the activity context in parameter
    /// </summary>
    /// <typeparam name="Result">Result of the expresssion</typeparam>
    [Description("Execute a lambda expression with result of type Result and with the activity context in parameter")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.CodeToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/CodeDesigner.bmp")]
    public sealed class EvaluateLambdaWithResult<Result> : CodeActivity<Result>
    {
        public EvaluateLambdaWithResult()
        {

        }

        public EvaluateLambdaWithResult(Func<ActivityContext, Result> lambda)
        {
            Lambda = lambda;
        }

        /// <summary>
        /// Lambda Expression to execute
        /// </summary>
        [Description(" Lambda Expression to execute")]
        public Func<ActivityContext, Result> Lambda { get; set; }

        protected override Result Execute(CodeActivityContext context)
        {
            if (Lambda != null)
                return Lambda(context);
            else
                return default(Result);
        }
    }
}
