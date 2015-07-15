./WFT
Workflow Foundation Toolbox

<p><strong><span style="font-size: 10pt;">0.2.2 Features :</span></strong></p>
<ul>
<li><font size="2">Core (WFT.Core namespace)</font>
<ul>
<li><font size="2">Xml InstanceStore &amp; Memory InstanceStore Providers</font></li>
</ul>
</li>
<li><font size="2">Toolbox (WFT.Activities namespace)</font>
<ul>
<li><font size="2">Common Activities&nbsp;&nbsp;(WFT.Activities namespace)</font>
<ul style="font-size: 10pt;">
<li style="font-size: 10pt;"><em>For&nbsp;</em>:&nbsp;Run activities (sequential) repeatedly until a specified expression evaluates to false</li>
<li style="font-size: 10pt;"><em>ParralelFor</em> :&nbsp;Run in parallel activities repeatedly until a specified expression evaluates to falsen evaluates to false</li>
<li style="font-size: 10pt;"><em>CrontabDelay&nbsp;</em>:<em> Delays execution for a specified crontab expression</em></li>
<li style="font-size: 10pt;"><em>GetWorkflowID&nbsp;:&nbsp;Get the Workflow Instance ID</em></li>
<li style="font-size: 10pt;"><em>CreateBookmark&nbsp;</em>:&nbsp;Create a bookmark</li>
<li style="font-size: 10pt;"><em>CreateBookmark&lt;T&gt;&nbsp;</em>:&nbsp;Create a bookmark with a return value</li>
<li style="font-size: 10pt;"><em>EvaluateExpression&nbsp;</em>:&nbsp;Execute an expression with the activity context in parameter</li>
<li style="font-size: 10pt;"><em>EvaluateExpressionWithResult&lt;Result&gt; </em>:&nbsp;Execute an expression with result of type Result and with the activity context in parameter</li>
<li style="font-size: 10pt;"><em>EvaluateLambda&nbsp;</em>:&nbsp;Execute an lambda expression with the activity context in parameter</li>
<li style="font-size: 10pt;"><em>EvaluateLambdaWithResult&lt;Result&gt;&nbsp;</em>:&nbsp;Execute a lambda expression with result of type Result and with the activity context in parameter</li>
</ul>
</li>
<li><font size="2">Net Activities (WFT.Activities.Net namespace)</font>
<ul>
<li><font size="2"><em>SendMail</em> :&nbsp;Send a mail</font></li>
</ul>
</li>
<li><font size="2">Xml Activities&nbsp;(WFT.Activities.Xml namespace)</font>
<ul>
<li><em><font size="2">FormatXml&nbsp;</font></em><font size="2">:&nbsp;Format xml string with indentation</font></li>
<li><em><font size="2">XslTransform&nbsp;</font></em><font size="2">:&nbsp;</font><font size="2">Transform a xml content with a xsl stylesheet</font></li>
<li><em><font size="2">ReadXmlValue</font></em><font size="2">:&nbsp;Read xml and return a string value with a xpath expression</font></li>
<li><font size="2"><em>ReadXmlValues</em>&nbsp;:&nbsp;Read xml and return collection IEnumerable&lt;String&gt;) values with a xpath expression</font></li>
<li><font size="2"><em>ReadXmlValues&lt;Collection&gt;</em> :&nbsp;Read xml and return collection values with a xpath expression.&nbsp;The collection must implement IEnumerable&lt;String&gt;</font></li>
<li><font size="2"><em>AddXmlAttribute</em>&nbsp;:&nbsp;Add a xml attribute&nbsp;with single value</font></li>
<li><font size="2"><em>AddXmlAttributes</em>&nbsp;:&nbsp;Add a xml attribute values ​​to multiple elements</font></li>
<li><font size="2"><em>CreateXmlChild</em>&nbsp;:&nbsp;Create a xml child</font></li>
<li><font size="2"><em>CreateXmlChilds</em>&nbsp;:&nbsp;Create xml childs</font></li>
<li><font size="2"><em>InsertXmlNode</em>&nbsp;:&nbsp;Insert a xml node</font></li>
<li><font size="2"><em>InsertXmlNodes</em>&nbsp;:&nbsp;Insert xml nodes</font></li>
<li><font size="2"><em>RemoveXmlAttribute</em>&nbsp;:&nbsp;Remove a xml attribute</font></li>
<li><font size="2"><em>RemoveXmlChildrens</em>&nbsp;:&nbsp;Remove xml childrens objects</font></li>
<li><font size="2"><em>RemoveXmlNode</em>&nbsp;:&nbsp;Remove a xml node</font></li>
<li><font size="2"><em>ReplaceXmlNode</em>&nbsp;:&nbsp;Replace an xml node</font></li>
<li><font size="2"><em>SetXmlAttributeValue</em>&nbsp;:&nbsp;Set value to an attribute</font></li>
<li><font size="2"><em>SetXmlAttributeValues</em>&nbsp;:&nbsp;Set values to &nbsp;attributes</font></li>
<li><font size="2"><em>SetXmlNodeValue</em>&nbsp;:&nbsp;Set value to a xml node</font></li>
<li><font size="2"><em>SetXmlNodeValues</em>&nbsp;:&nbsp;Set values to xml nodes</font></li>
</ul>
</li>
</ul>
</li>
<li><font size="2">Activity Designers (WFT.Activities.Designers namespace)</font>
<ul>
<li><font size="2"><em>IconActivityDesigner</em> : Basic activity designer display a icon (DesignerIconAttribute must be specified in the activity associated to this designer)</font></li>
</ul>
</li>
<li><font size="2">Extension Methods (WFT namespace)</font>
<ul>
<li><font size="2">For the type&nbsp;System.Activities.Activity</font>
<ul>
<li><font size="2"><em>Dictionary&lt;string, object&gt; Invoke(IDictionary&lt;string, object&gt; inputs = null, bool validateActivity = true)</em> :&nbsp;Invoke an activity</font></li>
<li><em><font size="2">void RunSync(Action&lt;WorkflowApplication, BookmarkInfo&gt; resumeBookmarkAction = null, bool validateActivity =</font></em><font size="2"> true) :&nbsp;Begins the execution (sync) of a workflow instance.</font></li>
<li><font size="2"><em>Validate()</em> :&nbsp;Validate an activity</font></li>
</ul>
</li>
<li><font size="2">For the type&nbsp;System.Activities.ActivityContext</font>
<ul>
<li><font size="2"><em>T GetReferenceValue&lt;T&gt;(LocationReference reference)</em> :&nbsp;Get the value of a location reference (variable) outside of the activity of type T</font></li>
<li><font size="2"><em>T GetArgumentValue&lt;T&gt;(string argumentName)</em> :&nbsp;Get a argument value</font></li>
<li><font size="2"><em>void SetArgumentValue&lt;T&gt;(string argumentName, T value)</em> :&nbsp;Set argument value</font></li>
</ul>
</li>
<li><font size="2">For the type System.String</font>
<ul>
<li><em><font size="2">DateTime? GetNextCronOccurrence()&nbsp;</font></em><font size="2">:&nbsp;Gets the next occurrence of a&nbsp;crontab string Expression</font></li>
<li><font size="2"><em>string GetXmValue(string xpath, string xPathIterationNode = "")</em> :&nbsp;Get a xml value with a xpath expression</font></li>
<li><font size="2"><em>List&lt;string&gt; GetXmlNodes(string xPath, string xPathIterationNode = "")</em> :&nbsp;Get xml values with a xpath expression</font></li>
<li><font size="2"><em>string AddXmlAttribute(string sourceXPath, string attributeName, List&lt;string&gt; values)</em> :&nbsp;Add a xml attribute</font></li>
<li><font size="2"><em>string SetXmlAttributeValue(string xPath, List&lt;string&gt; values)</em> :&nbsp;Set xml attribute value</font></li>
<li><font size="2"><em>string SetXmlValue(string xPath, List&lt;string&gt; values, XmlNodeOperation operation)</em> :&nbsp;Set a xml value</font></li>
<li><font size="2"><em>string RemoveXmlObject(string xPath, XmlRemoveOperation operation)</em>:&nbsp;Remove a xml object</font></li>
<li><font size="2"><em>string TransformXml(this string xml, string xsl)</em> :&nbsp;Transform a xml with a xsl</font></li>
<li><font size="2"><em>XmlReader GetXmlReader()</em> :&nbsp;Get a xml reader</font></li>
</ul>
</li>
<li><font size="2">For the type System.Type</font>
<ul>
<li><font size="2"><em>ImageDrawing GetImageDrawingFromResource(string resourceName)</em> :&nbsp;Get ImageDrawing Instance From Resource Image File</font></li>
<li><font size="2"><em>ImageSource GetImageSourceFromResource(string resourceName)</em> :&nbsp;Get ImageSource Instance From Resource Image File</font></li>
</ul>
</li>
<li><font size="2">For the type&nbsp;System.Linq.Expressions.ParameterExpression</font>
<ul>
<li><em><font size="2">Expression GetReferenceValue(Type type, LocationReference reference, bool isOutArgument = false)&nbsp;</font></em><font size="2">:&nbsp;Get the expression value of a location reference (variable) outside of the activity with the activity context expression</font></li>
<li><em><font size="2">Expression GetReferenceValue&lt;T&gt;(LocationReference reference, bool isOutArgument = false)&nbsp;</font></em><font size="2">:&nbsp;Get the expression value of a location reference (variable) of type T outside of the activity with the activity context expression</font></li>
</ul>
</li>
<li><font size="2">For the type&nbsp;System.Linq.Expressions.Expression</font>
<ul>
<li><em><font size="2">Expression GetActivityExpressionValue&lt;T&gt;(Expression&lt;Func&lt;ActivityContext, T&gt;&gt; expression)&nbsp;</font></em><font size="2">:&nbsp;Get the value expression of an expression with result of type T</font></li>
</ul>
</li>
</ul>
</li>
</ul>