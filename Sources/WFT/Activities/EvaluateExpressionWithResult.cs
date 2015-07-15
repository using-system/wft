using System;
using System.Activities;
using System.ComponentModel;
using System.Drawing;
using System.Linq.Expressions;
using WFT.Activities.Designers;

namespace WFT.Activities
{
    /// <summary>
    /// Execute an expression with result of type Result and with the activity context in parameter
    /// </summary>
    /// <typeparam name="Result">Result of the expresssion</typeparam>
    [Description("Execute an expression with result of type Result and with the activity context in parameter")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.CodeToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/CodeDesigner.bmp")]
    public sealed class EvaluateExpressionWithResult<Result> : CodeActivity<Result>
    {
        public EvaluateExpressionWithResult()
        {

        }

        public EvaluateExpressionWithResult(Expression<Func<ActivityContext, Result>> expression)
        {
            Expression = expression;
        }

        /// <summary>
        /// Expression to compile and execute
        /// </summary>
        [Description("Expression to compile and execute")]
        public Expression<Func<ActivityContext, Result>> Expression { get; set; }

        protected override Result Execute(CodeActivityContext context)
        {
            if (Expression != null)
                return Expression.Compile()(context);
            else
                return default(Result);
        }
    }
}
