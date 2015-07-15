using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WFT.Activities.Xml;

namespace WFT.Tests.Activities
{
    [TestClass]
    public class XmlTransformTest
    {
        [TestMethod]
        public void XmlTransformationAddAttributeValuesAndChildsOKTest()
        {
            Variable<string> xmlVariable = new Variable<string>("xml", "<assets></assets>");
            
            DynamicActivity<string> activity = new DynamicActivity<string>()
            {
                Implementation = () => new Sequence()
                {
                    Variables = { xmlVariable },
                    Activities =
                    {
                        new CreateXmlChilds()
                        {
                            XmlSource = xmlVariable,
                            Result = xmlVariable,
                            XPath = "/assets",
                            XmlChilds = new ReadXmlValues<List<string>>()
                            {
                                Xml =   @"<assets>
                                            <asset>
                                                <name>asset1</name>
                                            </asset>
                                            <asset>
                                                <name>asset2</name>
                                            </asset>
                                            <asset>
                                               <name>asset3</name>
                                             </asset>
                                        </assets>",
                                XPath = "/assets/asset"
                            }                            
                        },
                        new AddXmlAttributes()
                        {
                            XmlSource = xmlVariable,
                            Result = xmlVariable,
                            XPath = "/assets/asset",
                            AttributeName = "priority",
                            Values = new ReadXmlValues<List<string>>()
                            {
                                Xml = @"<attributes>
                                            <asset>
                                                 <id>1</id>
                                            </asset>
                                            <asset>
                                                 <id>2</id>
                                            </asset>
                                            <asset>
                                                 <id>3</id>
                                            </asset>
                                        </attributes>",
                                XPath = "/attributes/asset/id/text()"
                            }
                        },
                        new Assign<string>()
                        {
                            To = new ArgumentReference<string>("Result"),
                            Value = new FormatXml()
                            {
                                XmlSource = xmlVariable
                            }
                        }
                    }
                }
            };

            Assert.AreEqual(XDocument.Parse(
                 @"<assets>
                    <asset priority='1'>
                        <name>asset1</name>
                    </asset>
                    <asset priority='2'>
                        <name>asset2</name>
                    </asset>
                    <asset priority='3'>
                        <name>asset3</name>
                    </asset>
                   </assets>").ToString(), 
                  activity.Invoke()["Result"].ToString());
        }

        [TestMethod]
        public void XmlTransformationAttributeOperationsOKTest()
        {
            Variable<string> xmlVariable = new Variable<string>("xml", 
     @"<assets>
        <asset>
            <name>asset1</name>
        </asset>
        <asset>
           <name>asset2</name>
       </asset>
       <asset>
           <name>asset3</name>
        </asset>
    </assets>");



            DynamicActivity<string> activity = new DynamicActivity<string>()
            {
                Implementation = () => new Sequence()
                {
                    Variables = { xmlVariable },
                    Activities =
                    {
                        new AddXmlAttributes()
                        {
                            XmlSource = xmlVariable,
                            Result = xmlVariable,
                            AttributeName = "priority",
                            XPath = "/assets/asset"
                        },
                        new SetXmlAttributeValues()
                        {
                            XmlSource = xmlVariable,
                            Result = xmlVariable,
                            XPath = "/assets/asset/@priority",
                            Values = new ReadXmlValues<List<string>>()
                            {
                                Xml = @"<attributes>
                                            <asset>
                                                <id>1</id>
                                            </asset>
                                            <asset>
                                                <id>2</id>
                                            </asset>
                                            <asset>
                                                <id>3</id>
                                            </asset>
                                        </attributes>",
                                XPath = "/attributes/asset/id/text()"
                            }
                        },
                        new RemoveXmlAttribute()
                        {
                            XmlSource = xmlVariable,
                            Result = xmlVariable,
                            XPath = "/assets/asset[@priority = 1 or @priority = 3]/@priority"
                        },
                        new AddXmlAttribute()
                        {
                            XmlSource = xmlVariable,
                            Result = xmlVariable,
                            XPath = "/assets/asset[3]",
                            AttributeName = "priority3",
                            Value = "20"
                        },
                        new AddXmlAttribute()
                        {
                            XmlSource = xmlVariable,
                            Result = xmlVariable,
                            XPath = "/assets/asset[1]",
                            AttributeName = "priority1",
                            Value = "3"
                        },
                        new Assign<string>()
                        {
                            To = new ArgumentReference<string>("Result"),
                            Value = new FormatXml()
                            {
                                XmlSource = xmlVariable
                            }
                        }
                    }
                }
            };

            Assert.AreEqual(XDocument.Parse(
                @"<assets>
                    <asset priority1='3'>
                        <name>asset1</name>
                    </asset>
                    <asset priority='2'>
                        <name>asset2</name>
                    </asset>
                    <asset priority3='20'>
                        <name>asset3</name>
                    </asset>
                   </assets>").ToString(),
                activity.Invoke()["Result"].ToString());
        }

        [TestMethod]
        public void XmlTransformationNodeOperationsOKTest()
        {
            Variable<string> xmlVariable = new Variable<string>("xml","<assets></assets>");

            string xmlTemplate = @"<attributes>
                                    <asset>
                                        <id>1</id>
                                    </asset>
                                    <asset>
                                        <id>2</id>
                                    </asset>
                                    <asset>
                                        <id>3</id>
                                    </asset>
                                    <name>1</name>
                                    <name>2</name>
                                    <name>3</name>
                                   </attributes>";

            DynamicActivity<string> activity = new DynamicActivity<string>()
            {
                Implementation = () => new Sequence()
                {
                    Variables = { xmlVariable },
                    Activities =
                    {
                        new SetXmlNodeValue()
                        {
                            XmlSource = xmlVariable,
                            Result = xmlVariable,
                            XPath = "/assets",
                            Value = new ReadXmlValue()
                            {
                                Xml = xmlTemplate,
                                XPath = "/attributes"
                            }
                        },
                        new InsertXmlNodes()
                        {
                            XmlSource = xmlVariable,
                            Result = xmlVariable,
                            InsertMode = XmlInsertNodeMode.Before,
                            XPath = "/assets/attributes",
                            XmlNodes = new ReadXmlValues<List<string>>()
                            {
                                Xml = xmlVariable,
                                XPath = "/assets/attributes/asset"
                            }
                        },
                        new RemoveXmlNode()
                        {
                            XmlSource = xmlVariable,
                            Result = xmlVariable,
                            XPath = "/assets/attributes"
                        },
                        new InsertXmlNode()
                        {
                            XmlSource = xmlVariable,
                            Result = xmlVariable,
                            InsertMode  = XmlInsertNodeMode.After,
                            XPath = "/assets/asset[2]",
                            XmlNode = "<asset> <id>22</id></asset>"
                        },
                        new InsertXmlNode()
                        {
                            XmlSource = xmlVariable,
                            Result = xmlVariable,
                            InsertMode = XmlInsertNodeMode.After,
                            XPath = "/assets/asset/id",
                            XmlNode = "<name></name>"
                        },
                        new InsertXmlNode()
                        {
                            XmlSource = xmlVariable,
                            Result = xmlVariable,
                            InsertMode = XmlInsertNodeMode.Before,
                            XPath = "/assets/asset/id",
                            XmlNode = "<internalid></internalid>"
                        },
                        new SetXmlNodeValues()
                        {
                            XmlSource = xmlVariable,
                            Result = xmlVariable,
                            XPath = "/assets/asset/name",
                            Values = new ReadXmlValues<List<string>>()
                            {
                                Xml = xmlTemplate,
                                XPath = "/attributes/name/text()"
                            }
                        },
                        new ReplaceXmlNode()
                        {
                            XmlSource = xmlVariable,
                            Result = xmlVariable,
                            XPath = "/assets/asset[4]",
                            NewXmlNode = "<asset><empty></empty></asset>"
                        },
                        new Assign<string>()
                        {
                            To = new ArgumentReference<string>("Result"),
                            Value = new FormatXml()
                            {
                                XmlSource = xmlVariable
                            }
                        }
                    }
                }
            };

            Assert.AreEqual(XDocument.Parse(
                @"<assets>
                    <asset>
                        <internalid></internalid>
                        <id>1</id>
                        <name>1</name>
                    </asset>
                    <asset>
                        <internalid></internalid>
                        <id>2</id>
                        <name>2</name>
                    </asset>
                    <asset>
                        <internalid></internalid>
                        <id>22</id>
                        <name>3</name>
                    </asset>
                    <asset>
                        <empty></empty>
                    </asset>
                   </assets>").ToString(),
                activity.Invoke()["Result"].ToString());
        }

        [TestMethod]
        public void XmlTransformationChildOperationsOKTest()
        {
            Variable<string> xmlVariable = new Variable<string>("xml", @"
    <PCCAD_CD:Content contentID='DANSLACOURDH0083706Z' contentName='DANSLACOURDH0083706Z' catalogPriority='2' voLanguage='FRA' xmlns:PCCAD_CD='urn:PCCAD:CD:schema:20050901' xmlns:PCCAD_TV='urn:PCCAD:TVLocation:schema:20050901' xmlns:PCCAD_VOD='urn:PCCAD:VOD:schema:20050901' xmlns:PCCAD_gc='urn:PCCAD:GC:schema:20050901' xmlns:PCCAD_st='urn:PCCAD:ST:schema:20050901' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:noNamespaceSchemaLocation='SchemaW3CPCCAD_SC.xsd' name='NMTOKEN' progressionStatus='0' action='INIT' xmlAutoProvisioning='true' priority='false'>
        <PCCAD_CD:ArtistList>
            <PCCAD_CD:ArtistItem PCCAD_VOD:id='1'>
                <PCCAD_CD:FirstName>Mathilde</PCCAD_CD:FirstName>
                <PCCAD_CD:LastName>POYMIRO</PCCAD_CD:LastName>
                <PCCAD_CD:Role>ACT</PCCAD_CD:Role>
                <PCCAD_CD:InRole>Emma</PCCAD_CD:InRole>
            </PCCAD_CD:ArtistItem>
            <PCCAD_CD:ArtistItem  PCCAD_VOD:id='2'>
                <PCCAD_CD:FirstName>Paul-Adrien</PCCAD_CD:FirstName>
                <PCCAD_CD:LastName>FERRE</PCCAD_CD:LastName>
                <PCCAD_CD:Role>ACT2</PCCAD_CD:Role>
                <PCCAD_CD:InRole>Paul</PCCAD_CD:InRole>
            </PCCAD_CD:ArtistItem>
        </PCCAD_CD:ArtistList>
    </PCCAD_CD:Content>");

            DynamicActivity<string> activity = new DynamicActivity<string>()
            {
                Implementation = () => new Sequence()
                {
                    Variables = { xmlVariable },
                    Activities =
                    {
                        new InsertXmlNode()
                        {
                            XmlSource = xmlVariable,
                            Result = xmlVariable,
                            InsertMode = XmlInsertNodeMode.Before,
                            XPath = "/PCCAD_CD:Content/PCCAD_CD:ArtistList",
                            XmlNode = "<myList></myList>"
                        },
                        new CreateXmlChilds()
                        {
                            XmlSource = xmlVariable,
                            Result = xmlVariable,
                            XPath = "/PCCAD_CD:Content/myList",
                            XmlChilds = new ReadXmlValues<List<string>>()
                            {
                                Xml = xmlVariable,
                                XPath = "/PCCAD_CD:Content/PCCAD_CD:ArtistList/PCCAD_CD:ArtistItem"
                            }
                        },
                        new RemoveXmlChildrens()
                        {
                            XmlSource = xmlVariable,
                            Result = xmlVariable,
                            XPath = "/PCCAD_CD:Content/PCCAD_CD:ArtistList"
                        },
                        new Assign<string>()
                        {
                            To = new ArgumentReference<string>("Result"),
                            Value = new FormatXml()
                            {
                                XmlSource = xmlVariable
                            }
                        }
                    }
                }
            };

            Assert.AreEqual(XDocument.Parse(
                @"<PCCAD_CD:Content contentID='DANSLACOURDH0083706Z' contentName='DANSLACOURDH0083706Z' catalogPriority='2' voLanguage='FRA' xmlns:PCCAD_CD='urn:PCCAD:CD:schema:20050901' xmlns:PCCAD_TV='urn:PCCAD:TVLocation:schema:20050901' xmlns:PCCAD_VOD='urn:PCCAD:VOD:schema:20050901' xmlns:PCCAD_gc='urn:PCCAD:GC:schema:20050901' xmlns:PCCAD_st='urn:PCCAD:ST:schema:20050901' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:noNamespaceSchemaLocation='SchemaW3CPCCAD_SC.xsd' name='NMTOKEN' progressionStatus='0' action='INIT' xmlAutoProvisioning='true' priority='false'>
                    <myList>
                        <PCCAD_CD:ArtistItem PCCAD_VOD:id='1' xmlns:PCCAD_VOD='urn:PCCAD:VOD:schema:20050901' xmlns:PCCAD_CD='urn:PCCAD:CD:schema:20050901'>
                            <PCCAD_CD:FirstName>Mathilde</PCCAD_CD:FirstName>
                            <PCCAD_CD:LastName>POYMIRO</PCCAD_CD:LastName>
                            <PCCAD_CD:Role>ACT</PCCAD_CD:Role>
                            <PCCAD_CD:InRole>Emma</PCCAD_CD:InRole>
                        </PCCAD_CD:ArtistItem>
                        <PCCAD_CD:ArtistItem  PCCAD_VOD:id='2' xmlns:PCCAD_VOD='urn:PCCAD:VOD:schema:20050901' xmlns:PCCAD_CD='urn:PCCAD:CD:schema:20050901'>
                            <PCCAD_CD:FirstName>Paul-Adrien</PCCAD_CD:FirstName>
                            <PCCAD_CD:LastName>FERRE</PCCAD_CD:LastName>
                            <PCCAD_CD:Role>ACT2</PCCAD_CD:Role>
                            <PCCAD_CD:InRole>Paul</PCCAD_CD:InRole>
                        </PCCAD_CD:ArtistItem>
                    </myList>
                    <PCCAD_CD:ArtistList></PCCAD_CD:ArtistList>
                   </PCCAD_CD:Content>").ToString(),
                activity.Invoke()["Result"].ToString());
        }
    }
}
