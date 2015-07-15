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
    public class ExpressionWithResultTest
    {
        [TestMethod]
        public void RunExpressionWithResultOKTest()
        {
            var activityContext = System.Linq.Expressions.Expression.Parameter(typeof(ActivityContext));
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
                   new If()
                   {
                       Condition = new EvaluateExpressionWithResult<bool>(
                           System.Linq.Expressions.Expression.Lambda<Func<ActivityContext, bool>>(
                           System.Linq.Expressions.Expression.NotEqual(
                                            activityContext.GetReferenceValue<int>(integerVariable), 
                                            System.Linq.Expressions.Expression.Constant(5)), activityContext)),
                        Then = new Throw()
                        {
                            Exception = new InArgument<Exception>((C) => new NotSupportedException())
                        }
                   }
                }
            };

            activity.Invoke();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void RunExpressionWithResultOnExceptionKOTest()
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
                   new If()
                   {
                       Condition = new EvaluateExpressionWithResult<bool>(
                           Expression.Lambda<Func<ActivityContext, bool>>(
                           Expression.Equal(
                                            activityContext.GetReferenceValue<int>(integerVariable), 
                                            Expression.Constant(5)), activityContext)),
                        Then = new Throw()
                        {
                            Exception = new InArgument<Exception>((C) => new NotSupportedException())
                        }
                   }
                }
            };

            activity.Invoke();
        }

    }
}
