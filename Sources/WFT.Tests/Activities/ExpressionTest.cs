using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WFT.Activities;

namespace WFT.Tests.Activities
{
    [TestClass]
    public class ExpressionTest
    {
        [TestMethod]
        public void RunExpressionOKTest()
        {

            var activityContext = Expression.Parameter(typeof(ActivityContext));
            Variable<int> integerVariable = new Variable<int>("integerVariable");

            Activity activity = new Sequence()
            {
                Variables = { integerVariable},
                Activities =
                {
                   new Assign<int>()
                   {
                       To = integerVariable,
                       Value = 5
                   },
                   new EvaluateExpression()
                   {
                       Expression = Expression.Lambda<Action<ActivityContext>>(                    
                        Expression.IfThen(Expression.NotEqual(
                                            activityContext.GetReferenceValue<int>(integerVariable), 
                                            Expression.Constant(5)),
                                          Expression.Throw(Expression.Constant(new NotSupportedException()))), activityContext)

                   }
                }
            };

            activity.Invoke();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void RunExpressionWithThrowKOTest()
        {

            var activityContext = Expression.Parameter(typeof(ActivityContext));
            Variable<int> integerVariable = new Variable<int>("integerVariable");

            Activity activity = new Sequence()
            {
                Variables = { integerVariable },
                Activities =
                {
                   new Assign<int>()
                   {
                       To = integerVariable,
                       Value = 5
                   },
                   new EvaluateExpression()
                   {
                       Expression = Expression.Lambda<Action<ActivityContext>>(                    
                        Expression.IfThen(Expression.Equal(
                                            activityContext.GetReferenceValue<int>(integerVariable), 
                                            Expression.Constant(5)),
                                          Expression.Throw(Expression.Constant(new NotSupportedException()))), activityContext)

                   }
                }
            };

            activity.Invoke();
        }
    }
}
