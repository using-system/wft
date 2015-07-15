using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFT.Activities;

namespace WFT.Tests.Activities
{
    [TestClass]
    public class LambdaWithResultTest
    {
        [TestMethod]
        public void RunLambdaWithResultOKTest()
        {
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
                       Condition = new EvaluateLambdaWithResult<bool>((C)=>
                           {
                               return C.GetReferenceValue<int>(integerVariable) != 5;
                           }),
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
        public void RunLambdaWithResultOnExceptionKOTest()
        {
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
                       Condition = new EvaluateLambdaWithResult<bool>((C) =>
                           {
                               return C.GetReferenceValue<int>(integerVariable) == 5;
                           }),
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
