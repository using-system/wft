using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
    public class ReadXmlValueTest
    {
        [TestMethod]
        public void ReadXmlValueOKTest()
        {
            string xml = "<values><value>4</value><value>7</value><value>-3</value></values>";

            string result = new ReadXmlValue()
            {
                Xml = xml,
                XPath = "/values/value/text()"
            }.Invoke()["Result"] as string;

            Assert.IsNotNull(result);
            Assert.AreEqual("4", result);
        }

        [TestMethod]
        public void ReadXmlValueWithBadXPathOKTest()
        {
            string result = new ReadXmlValue()
            {
                Xml = "<values><value>4</value><value>7</value><value>-3</value></values>",
                XPath = "badxpath"
            }.Invoke()["Result"] as string;

            Assert.IsNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void ReadXmlValueWithBadXmlKOTest()
        {
            new ReadXmlValue()
            {
                Xml = "badxml",
                XPath = "/values/value/text()"
            }.Invoke();
        }
    }
}
