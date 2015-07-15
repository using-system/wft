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
    public class LambdaTest
    {
        [TestMethod]
        public void RunLambdaOKTest()
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
                   new EvaluateLambda()
                   {
                       Lambda = (C) =>
                           {
                               if (C.GetReferenceValue<int>(integerVariable) != 5)
                                   throw new NotSupportedException();
                           }
                   }
                }
            };

            activity.Invoke();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void RunLambdaWithThrowKOTest()
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
                   new EvaluateLambda()
                   {
                       Lambda = (C) =>
                           {
                               if (C.GetReferenceValue<int>(integerVariable) == 5)
                                   throw new NotSupportedException();
                           }
                   }
                }
            };

            activity.Invoke();
        }
    }
}
