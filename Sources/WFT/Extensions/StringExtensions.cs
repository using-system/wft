using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using WFT.Helpers.Scheduling;

namespace WFT
{
    /// <summary>
    /// Extension methods for the type String
    /// </summary>
    public static class StringExtensions
    {
        #region Xml extension methods

        /// <summary>
        /// Get a xml value with a xpath expression
        /// </summary>
        /// <param name="xml">Source xml</param>
        /// <param name="xpath">XPath expression</param>
        /// <param name="xPathIterationNode">Xpath iteration node expression</param>
        /// <returns>Return the xml value</returns>
        public static string GetXmValue(this string xml, string xpath, string xPathIterationNode = "")
        {
            return GetXmlNodes(xml, xpath, xPathIterationNode).FirstOrDefault();
        }

        /// <summary>
        /// Get xml values with a xpath expression
        /// </summary>
        /// <param name="xml">Source xml</param>
        /// <param name="xPath">XPath expression</param>
        /// <param name="xPathIterationNode">Xpath iteration node expression</param>
        /// <returns>Return the xml values</returns>
        public static List<string> GetXmlNodes(this string xml, string xPath, string xPathIterationNode = "")
        {
            try
            {
                List<string> values = new List<string>();

                if (String.IsNullOrEmpty(xml))
                    return new List<string>();

                List<string> xmls = new List<string>();
                if (String.IsNullOrEmpty(xPathIterationNode))
                    xmls.Add(xml);
                else
                    xmls.AddRange(GetXmlNodes(xml, xPathIterationNode));

                foreach (string currrentXml in xmls)
                {
                    List<string> currentValues = new List<string>();

                    object result = Evaluate(currrentXml, xPath);
                    if (result == null)
                        throw new Exception(
                            String.Format("The XPath expression {0} does not match to any nodes", xPath));
                    else if (result is IEnumerable<object>)
                        currentValues = ((IEnumerable<object>)result)
                            .Select((N) =>
                            {
                                if (N is XAttribute)
                                    return ((XAttribute)N).Value;
                                else
                                    return N.ToString();
                            })
                            .ToList();
                    else
                        currentValues = new List<string>() { result.ToString() };

                    if (currentValues.Count > 0)
                        values.AddRange(currentValues);
                    else if (!String.IsNullOrEmpty(xPathIterationNode))
                        values.Add(null);
                }

                return values;
            }
            catch (Exception exc)
            {
                throw new XmlException(String.Format("Invalid Xml Instruction : {0}", exc.Message));
            }
        }


        /// <summary>
        /// Add a xml attribute
        /// </summary>
        /// <param name="sourceXml">Source xml</param>
        /// <param name="sourceXPath">XPath expression</param>
        /// <param name="attributeName">Attribute name to add</param>
        /// <param name="values">Attribute values</param>
        /// <returns>Return the xml updated</returns>
        public static string AddXmlAttribute(this string sourceXml, string sourceXPath, string attributeName, List<string> values)
        {
            try
            {
                XDocument xDoc;
                XmlNamespaceManager nsMgr;
                IEnumerable<object> result = Evaluate(sourceXml, sourceXPath, out xDoc, out nsMgr) as IEnumerable<object>;
                IDictionary<string, string> ns = nsMgr.GetNamespacesInScope(XmlNamespaceScope.All);
                if (result != null)
                {
                    int i = 0;
                    result
                        .Where((E) => E is XElement)
                        .Select((E) => E as XElement)
                        .ToList().ForEach((E) =>
                        {
                            string value = String.Empty;
                            if (values != null
                                && i < values.Count)
                                value = values[i];

                            if (value == null)
                                value = String.Empty;

                            string[] attrs = attributeName.Split(':');
                            XName name = null;

                            if (attributeName.Contains(':'))
                                if (ns.ContainsKey(attrs[0]))
                                    name = XNamespace.Get(ns[attrs[0]]) + attrs[1];
                                else
                                    throw new XmlException(String.Format("Namespace {0} is not declared", attrs[0]));
                            else
                                name = attributeName;

                            if (E.Attribute(name) == null)
                                E.Add(new XAttribute(name, value));
                            else
                                E.Attribute(name).Value = value;

                            i++;
                        });
                }
                return xDoc.ToString();
            }
            catch (Exception exc)
            {
                throw new XmlException(String.Format("Invalid Xml Instruction : {0}", exc.Message));
            }
        }

        /// <summary>
        /// Set xml attribute value
        /// </summary>
        /// <param name="sourceXml">Source xml</param>
        /// <param name="xPath">XPath expression</param>
        /// <param name="values">Attributes values</param>
        /// <returns>Return the xml updated</returns>
        public static string SetXmlAttributeValue(this string sourceXml, string xPath, List<string> values)
        {
            try
            {
                XDocument xDoc;
                IEnumerable<object> result = Evaluate(sourceXml, xPath, out xDoc) as IEnumerable<object>;
                if (result != null)
                {
                    int i = 0;
                    result
                        .Where((A) => A is XAttribute)
                        .Select((A) => A as XAttribute)
                        .ToList().ForEach((A) =>
                        {
                            string value = null;
                            if (values != null
                                && i < values.Count)
                                value = values[i];

                            if (value == null)
                                value = String.Empty;

                            A.Value = value;

                            i++;
                        });
                }
                return xDoc.ToString();
            }
            catch (Exception exc)
            {
                throw new XmlException(String.Format("Invalid Xml Instruction : {0}", exc.Message));
            }
        }

        /// <summary>
        /// Set a xml value
        /// </summary>
        /// <param name="sourceXml">Source xml</param>
        /// <param name="xPath">XPath expression</param>
        /// <param name="values">Values</param>
        /// <param name="operation">Set Operation (Insert, Replace, SetValue, CreateChild)</param>
        /// <returns>Return the xml updated</returns>
        public static string SetXmlValue(this string sourceXml, string xPath, List<string> values, XmlNodeOperation operation)
        {
            try
            {
                XDocument xDoc;
                IEnumerable<object> result = Evaluate(sourceXml, xPath, out xDoc) as IEnumerable<object>;
                if (result != null)
                {
                    int i = 0;
                    List<XObject> xObjects = new List<XObject>();
                    xObjects = result
                        .Where((A) => A is XObject)
                        .Select((A) => A as XObject)
                        .ToList();

                    if (xObjects.Count == 1
                        && xObjects.Count < values.Count)
                        while (xObjects.Count < values.Count)
                            xObjects.Add(xObjects[0]);

                    xObjects.ForEach((O) =>
                    {
                        string value = null;
                        if (values != null)
                            if (i < values.Count)
                                value = values[i];
                            else if (values.Count == 1)
                                value = values[0];

                        if (value == null)
                            value = String.Empty;

                        XDocument xDocValue;
                        XmlNamespaceManager nsMgrValue;

                        if (value.Contains('<')
                            && TryParseXDocument(value, out xDocValue, out nsMgrValue))
                        {
                            if (O is XElement
                                || O is XNode)
                            {
                                if (operation == XmlNodeOperation.InsertBefore)
                                    ((XElement)O).AddBeforeSelf(xDocValue.Root);
                                else if (operation == XmlNodeOperation.InsertAfter)
                                    ((XElement)O).AddAfterSelf(xDocValue.Root);
                                else if (operation == XmlNodeOperation.SetValue)
                                {
                                    ((XElement)O).RemoveAll();
                                    ((XElement)O).Add(xDocValue.Root);
                                }
                                else if (operation == XmlNodeOperation.Replace)
                                    ((XElement)O).ReplaceWith(xDocValue.Root);
                                else if (operation == XmlNodeOperation.CreateChild)
                                    ((XElement)O).Add(xDocValue.Root);
                            }
                        }
                        else
                        {
                            if (O is XElement)
                                if (operation == XmlNodeOperation.SetValue)
                                    ((XElement)O).Value = value;
                                else if (O is XNode)
                                    if (operation == XmlNodeOperation.SetValue)
                                        ((XNode)O).ReplaceWith(new XElement(((XElement)O).Name, value));
                        }

                        i++;
                    });
                }
                return xDoc.ToString();
            }
            catch (Exception exc)
            {
                throw new XmlException(String.Format("Invalid Xml Instruction : {0}", exc.Message));
            }
        }

        /// <summary>
        /// Remove a xml object
        /// </summary>
        /// <param name="sourceXml">Source xml</param>
        /// <param name="xPath">XPath expression</param>
        /// <param name="operation">Remove Operation (Self, Childrens)</param>
        /// <returns>Return the xml updated</returns>
        public static string RemoveXmlObject(this string sourceXml, string xPath, XmlRemoveOperation operation)
        {
            try
            {
                XDocument xDoc;
                IEnumerable<object> result = Evaluate(sourceXml, xPath, out xDoc) as IEnumerable<object>;
                if (result != null)
                    result
                        .ToList().ForEach((O) =>
                        {
                            if (O is XAttribute)
                            {
                                if (operation == XmlRemoveOperation.Self)
                                    ((XAttribute)O).Remove();
                            }
                            else if (O is XElement || O is XNode)
                            {
                                if (operation == XmlRemoveOperation.Childrens)
                                    ((XElement)O).Elements().Remove();
                                else if (operation == XmlRemoveOperation.Self)
                                    ((XElement)O).Remove();
                            }
                        });
                return xDoc.ToString();
            }
            catch (Exception exc)
            {
                throw new XmlException(String.Format("Invalid Xml Instruction : {0}", exc.Message));
            }
        }

        /// <summary>
        /// Transform a xml with a xsl
        /// </summary>
        /// <param name="xml">Source xml</param>
        /// <param name="xsl">Xsl stylesheet</param>
        /// <returns>Return the transformation</returns>
        public static string TransformXml(this string xml, string xsl)
        {
            try
            {
                XmlNamespaceManager nsMgr;
                XDocument xDoc = GetXDocument(xml, out nsMgr);

                XDocument result = new XDocument();

                using (XmlWriter writer = result.CreateWriter())
                {
                    XslCompiledTransform xslt = new XslCompiledTransform();
                    xslt.Load(GetXmlReader(xsl));
                    xslt.Transform(xDoc.CreateReader(), writer);
                }

                return result.ToString();

            }
            catch (Exception exc)
            {
                throw new XmlException(String.Format("Invalid Xml Instruction : {0}", exc.Message));
            }
        }

        /// <summary>
        /// Get a xml reader
        /// </summary>
        /// <param name="content">Xml content</param>
        /// <returns>Return the xml reader</returns>
        public static XmlReader GetXmlReader(this string content)
        {
            XmlReader xml = null;

            if (Uri.IsWellFormedUriString(content, UriKind.RelativeOrAbsolute))
                xml = XmlReader.Create(WebRequest.Create(content).GetResponse().GetResponseStream());
            else
                xml = XmlReader.Create(new StringReader(content));

            return xml;
        }


        private static object Evaluate(string xml, string xPath)
        {
            XDocument xDoc;
            return Evaluate(xml, xPath, out xDoc);
        }

        private static object Evaluate(string xml, string xPath, out XDocument xDoc)
        {
            XmlNamespaceManager nsMgr;
            return Evaluate(xml, xPath, out xDoc, out nsMgr);
        }

        private static object Evaluate(string xml, string xPath, out XDocument xDoc, out XmlNamespaceManager nsMgr)
        {
            xDoc = GetXDocument(xml, out nsMgr);
            return xDoc.XPathEvaluate(xPath, nsMgr);
        }

        private static bool TryParseXDocument(string xml, out XDocument xDoc, out XmlNamespaceManager nsMgr)
        {
            try
            {
                xDoc = GetXDocument(xml, out nsMgr);
                return true;
            }
            catch
            {
                xDoc = null;
                nsMgr = null;
                return false;
            }
        }

        private static XDocument GetXDocument(string xml, out XmlNamespaceManager nsMgr)
        {
            var xmlReader = XmlReader.Create(new StringReader(xml));
            XDocument xDoc = XDocument.Load(xmlReader);
            nsMgr = new XmlNamespaceManager(xmlReader.NameTable);
            foreach (var ns in GetAllNamespace(xDoc))
                nsMgr.AddNamespace(ns.Key, ns.Value);

            return xDoc;
        }

        private static IDictionary<string, string> GetAllNamespace(XDocument xDoc)
        {
            XPathNavigator nav = xDoc.CreateNavigator();
            nav.MoveToFollowing(XPathNodeType.Element);
            return nav.GetNamespacesInScope(XmlNamespaceScope.All);
        }

        public enum XmlRemoveOperation
        {
            Self,
            Childrens
        }

        public enum XmlNodeOperation
        {
            InsertBefore,
            InsertAfter,
            Replace,
            SetValue,
            CreateChild
        }

        #endregion

        #region Cron extension methods

        /// <summary>
        ///  Gets the next occurrence of this crontab Expression
        /// </summary>
        /// <param name="cronExpression">Cron tab expression</param>
        /// <returns>Return the next occurrence datetime</returns>
        public static DateTime? GetNextCronOccurrence(this string cronExpression)
        {
            return GetNextCronOccurrence(cronExpression, DateTime.Now);
        }

        /// <summary>
        /// Gets the next occurrence of this crontab Expression starting with a base time.
        /// </summary>
        /// <param name="cronExpression">Cron tab expression</param>
        /// <param name="startDate">Base time</param>
        /// <param name="throwExc">Defines if an exception is throw in case of invalid crontab expression</param>
        /// <returns>Return the next occurrence datetime</returns>
        public static DateTime? GetNextCronOccurrence(this string cronExpression, DateTime startDate, bool throwExc = true)
        {
            try
            {
                CrontabSchedule schedule = CrontabSchedule.Parse(cronExpression);
                return schedule.GetNextOccurrence(startDate);
            }
            catch (Exception)
            {
                if (throwExc)
                    throw new InvalidCastException(String.Format("Invalid cron expression : {0}", cronExpression));
                else
                    return null;
            }

        }

        #endregion
    }
}
