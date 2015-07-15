using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFT.Activities;

namespace WFT.Tests.Activities
{
    [TestClass]
    public class ForTest
    {
        [TestMethod]
        public void ForOKTest()
        {
            Variable<string> result = new Variable<string>("result");
            DelegateInArgument<int> argument = new DelegateInArgument<int>("argument");

            DynamicActivity<string> activity = new DynamicActivity<string>()
            {
                Implementation = () => new Sequence()
                {
                    Variables = { result },
                    Activities =
                    {
                        new For()
                        {
                            InitialVariableValue = 1,
                            ConditionOperator = BaseForActivity.Operator.IsLessThanOrEqualTo,
                            ConditionValue = 10,
                            VariableIncrement = 1,
                            Body = new ActivityAction<int>()
                            {
                                Argument = argument,
                                Handler = new Assign<string>()
                                {
                                    To = result,
                                    Value = new EvaluateLambdaWithResult<string>(
                                        (C) => C.GetReferenceValue<string>(result) + C.GetReferenceValue<int>(argument).ToString())
                                }
                            }
                        },
                        new Assign<string>()
                        {
                            To = new ArgumentReference<string>("Result"),
                            Value = result
                        }
                    }
                }
            };

            Assert.AreEqual("12345678910", activity.Invoke()["Result"]);
        }

        [TestMethod]
        public void ParallelForOKTest()
        {
            Variable<string> result = new Variable<string>("result");
            DelegateInArgument<int> argument = new DelegateInArgument<int>("argument");

            DynamicActivity<string> activity = new DynamicActivity<string>()
            {
                Implementation = () => new Sequence()
                {
                    Variables = { result },
                    Activities =
                    {
                        new ParallelFor()
                        {
                            InitialVariableValue = 0,
                            ConditionOperator = BaseForActivity.Operator.IsLessThan,
                            ConditionValue = 10,
                            VariableIncrement = 2,
                            Body = new ActivityAction<int>()
                            {
                                Argument = argument,
                                Handler = new Assign<string>()
                                {
                                    To = result,
                                    Value = new EvaluateLambdaWithResult<string>(
                                        (C) => C.GetReferenceValue<string>(result) + C.GetReferenceValue<int>(argument).ToString())
                                }
                            }
                        },
                        new Assign<string>()
                        {
                            To = new ArgumentReference<string>("Result"),
                            Value = result
                        }
                    }
                }
            };

            Assert.AreEqual("86420", activity.Invoke()["Result"]);
        }

        [TestMethod]
        public void ForDecrementOKTest()
        {
            Variable<string> result = new Variable<string>("result");
            DelegateInArgument<int> argument = new DelegateInArgument<int>("argument");

            DynamicActivity<string> activity = new DynamicActivity<string>()
            {
                Implementation = () => new Sequence()
                {
                    Variables = { result },
                    Activities =
                    {
                        new For()
                        {
                            InitialVariableValue = 10,
                            ConditionOperator = BaseForActivity.Operator.IsGreaterThan,
                            ConditionValue = 0,
                            VariableIncrement = -1,
                            Body = new ActivityAction<int>()
                            {
                                Argument = argument,
                                Handler = new Assign<string>()
                                {
                                    To = result,
                                    Value = new EvaluateLambdaWithResult<string>(
                                        (C) => C.GetReferenceValue<string>(result) + C.GetReferenceValue<int>(argument).ToString())
                                }
                            }
                        },
                        new Assign<string>()
                        {
                            To = new ArgumentReference<string>("Result"),
                            Value = result
                        }
                    }
                }
            };

            Assert.AreEqual("10987654321", activity.Invoke()["Result"]);
        }
    }
}
