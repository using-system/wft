using System;
using System.Activities;
using System.ComponentModel;
using System.Drawing;
using System.Linq.Expressions;
using WFT.Activities.Designers;

namespace WFT.Activities
{
    /// <summary>
    /// Execute an expression with the activity context in parameter
    /// </summary>
    [Description("Execute an expression with the activity context in parameter")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.CodeToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/CodeDesigner.bmp")]
    public sealed class EvaluateExpression : CodeActivity
    {
        public EvaluateExpression()
        {

        }

        public EvaluateExpression(Expression<Action<ActivityContext>> expression)
        {
            Expression = expression;
        }

        /// <summary>
        /// Expression to compile and execute
        /// </summary>
        [Description("Expression to compile and execute")]
        public Expression<Action<ActivityContext>> Expression { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            if(Expression != null)
                Expression.Compile()(context);
        }
    }
}
