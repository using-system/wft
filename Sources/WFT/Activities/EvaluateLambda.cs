using System;
using System.Activities;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities
{
    /// <summary>
    /// Execute an lambda expression with the activity context in parameter
    /// </summary>
    [Description("Execute an lambda expression with the activity context in parameter")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.CodeToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/CodeDesigner.bmp")]
    public sealed class EvaluateLambda : CodeActivity
    {
        public EvaluateLambda()
        {

        }

        public EvaluateLambda(Action<ActivityContext> lambda)
        {
            Lambda = lambda;
        }

        /// <summary>
        /// Lambda expression to execute
        /// </summary>
        [Description("Lambda expression to execute")]
        public Action<ActivityContext> Lambda { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            if (Lambda != null)
                Lambda(context);
        }
    }
}
