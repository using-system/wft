using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WFT.Activities;
using WFT.Activities.Xml;

namespace WFT.Tests.Activities
{
    [TestClass]
    public class ReadXmlValuesTest
    {
        [TestMethod]
        public void ForEachXmlValuesOKTest()
        {
            DelegateInArgument<string> value = new DelegateInArgument<string>("value");

            var result = new DynamicActivity<int>()
            {
                Implementation = () => new Sequence()
                {
                    Activities =
                    {
                        new Assign<int>()
                        {
                            To = new ArgumentReference<int>("Result"),
                            Value = 0
                        },
                        new ForEach<string>()
                        {
                            Values = new ReadXmlValues()
                            {
                                Xml = @"<values><value>4</value><value>7</value><value>-3</value></values>",
                                XPath = "/values/value/text()",
                                
                            },
                            Body = new ActivityAction<string>()
                            {
                                Argument = value,
                                Handler = new Assign<int>()
                                {
                                    To = new ArgumentReference<int>("Result"),
                                    Value = new EvaluateLambdaWithResult<int>(
                                        (C)=> C.GetArgumentValue<int>("Result") +  int.Parse(C.GetReferenceValue<string>(value)))
                                }
                            }
                        }
                    }
                }
            }.Invoke();

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ContainsKey("Result"));
            Assert.AreEqual(8, result["Result"]);
        }

        [TestMethod]
        public void ReadXmlValuesOKTest()
        {
            string xml = @"<users>
                            <user>
                                <Name>Name1</Name>
                                <Age>25</Age>
                            </user>
                            <user>
                                <Name>Name2</Name>
                            </user>
                            <user>
                                <Name>Name3</Name>
                                <Age>35</Age>
                            </user>
                           </users>";

            IEnumerable<string> result = new DynamicActivity<IEnumerable<string>>()
            {
                Implementation = () => new Assign<IEnumerable<string>>()
                {
                    To = new ArgumentReference<IEnumerable<string>>("Result"),
                    Value = new ReadXmlValues()
                    {
                        Xml = xml,
                        XPathIterationNode = "/users/user",
                        XPath = "/user/Age/text()"
                    }
                }
            }.Invoke()["Result"] as IEnumerable<string>;

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
            Assert.AreEqual("25", result.ToList()[0]);
            Assert.AreEqual(null, result.ToList()[1]);
            Assert.AreEqual("35", result.ToList()[2]);

            List<string> result2 = new DynamicActivity<List<string>>()
            {
                Implementation = () => new Assign<List<string>>()
                {
                    To = new ArgumentReference<List<string>>("Result"),
                    Value = new ReadXmlValues<List<string>>()
                    {
                        Xml = xml,
                        XPath = "/users/user/Age/text()"
                    }
                }
            }.Invoke()["Result"] as List<string>;

            Assert.IsNotNull(result2);
            Assert.AreEqual(2, result2.Count);
            Assert.AreEqual("25", result2[0]);
            Assert.AreEqual("35", result2[1]);
        }

        [TestMethod]
        public void ReadXmlValuesWithBadXPathOKTest()
        {
            IEnumerable<string> result = new ReadXmlValues()
            {
                Xml = "<values><value>4</value><value>7</value><value>-3</value></values>",
                XPath = "badxpath"
            }.Invoke()["Result"] as IEnumerable<string>;

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void ReadXmlValuesWithBadXmlKOTest()
        {
            new ReadXmlValues()
            {
                Xml = "badxml",
                XPath = "/values/value/text()"
            }.Invoke();
        }

    }
}
