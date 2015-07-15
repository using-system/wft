using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Activities.Expressions;
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
    public class XslTransformTest
    {
        [TestMethod]
        public void XslBasicTransformOKTest()
        {
            var result = new DynamicActivity<String>()
            {
                Implementation = () => new XslTransform()
                {
                    Xml = "<hello-world>   <greeter>An XSLT Programmer</greeter>   <greeting>Hello, World!</greeting></hello-world>",
                    Xsl = @"<?xml version='1.0'?>
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0'>
  <xsl:template match='/hello-world'>
    <HTML>
      <HEAD>
        <TITLE></TITLE>
      </HEAD>
      <BODY>
        <H1>
          <xsl:value-of select='greeting'/>
        </H1>
        <xsl:apply-templates select='greeter'/>
      </BODY>
    </HTML>
  </xsl:template>
  <xsl:template match='greeter'>
    <DIV>from <I><xsl:value-of select='.'/></I></DIV>
  </xsl:template>
</xsl:stylesheet>",
                  Result = new ArgumentReference<string>("Result")
                }
            }.Invoke();

            Assert.IsNotNull(result);
            Assert.AreEqual(
@"<HTML>
  <HEAD>
    <TITLE />
  </HEAD>
  <BODY>
    <H1>Hello, World!</H1>
    <DIV>from <I>An XSLT Programmer</I></DIV>
  </BODY>
</HTML>", result["Result"]);

        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void XslTransformWithBadXslKOTest()
        {
            new XslTransform()
            {
                Xml = "<hello-world>   <greeter>An XSLT Programmer</greeter>   <greeting>Hello, World!</greeting></hello-world>",
                Xsl = @"<?xml version='1.0'?>
                        <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0'>
                            bad
                        </xsl:stylesheet>"
            }.Invoke();
        }
    }
}
