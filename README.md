./WFT
Workflow Foundation Toolbox

0.2.2 Features :

Core (WFT.Core namespace)
Xml InstanceStore & Memory InstanceStore Providers
Toolbox (WFT.Activities namespace)
Common Activities  (WFT.Activities namespace)
For : Run activities (sequential) repeatedly until a specified expression evaluates to false
ParralelFor : Run in parallel activities repeatedly until a specified expression evaluates to falsen evaluates to false
CrontabDelay : Delays execution for a specified crontab expression
GetWorkflowID : Get the Workflow Instance ID
CreateBookmark : Create a bookmark
CreateBookmark<T> : Create a bookmark with a return value
EvaluateExpression : Execute an expression with the activity context in parameter
EvaluateExpressionWithResult<Result> : Execute an expression with result of type Result and with the activity context in parameter
EvaluateLambda : Execute an lambda expression with the activity context in parameter
EvaluateLambdaWithResult<Result> : Execute a lambda expression with result of type Result and with the activity context in parameter
Net Activities (WFT.Activities.Net namespace)
SendMail : Send a mail
Xml Activities (WFT.Activities.Xml namespace)
FormatXml : Format xml string with indentation
XslTransform : Transform a xml content with a xsl stylesheet
ReadXmlValue: Read xml and return a string value with a xpath expression
ReadXmlValues : Read xml and return collection IEnumerable<String>) values with a xpath expression
ReadXmlValues<Collection> : Read xml and return collection values with a xpath expression. The collection must implement IEnumerable<String>
AddXmlAttribute : Add a xml attribute with single value
AddXmlAttributes : Add a xml attribute values ​​to multiple elements
CreateXmlChild : Create a xml child
CreateXmlChilds : Create xml childs
InsertXmlNode : Insert a xml node
InsertXmlNodes : Insert xml nodes
RemoveXmlAttribute : Remove a xml attribute
RemoveXmlChildrens : Remove xml childrens objects
RemoveXmlNode : Remove a xml node
ReplaceXmlNode : Replace an xml node
SetXmlAttributeValue : Set value to an attribute
SetXmlAttributeValues : Set values to  attributes
SetXmlNodeValue : Set value to a xml node
SetXmlNodeValues : Set values to xml nodes
Activity Designers (WFT.Activities.Designers namespace)
IconActivityDesigner : Basic activity designer display a icon (DesignerIconAttribute must be specified in the activity associated to this designer)
Extension Methods (WFT namespace)
For the type System.Activities.Activity
Dictionary<string, object> Invoke(IDictionary<string, object> inputs = null, bool validateActivity = true) : Invoke an activity
void RunSync(Action<WorkflowApplication, BookmarkInfo> resumeBookmarkAction = null, bool validateActivity = true) : Begins the execution (sync) of a workflow instance.
Validate() : Validate an activity
For the type System.Activities.ActivityContext
T GetReferenceValue<T>(LocationReference reference) : Get the value of a location reference (variable) outside of the activity of type T
T GetArgumentValue<T>(string argumentName) : Get a argument value
void SetArgumentValue<T>(string argumentName, T value) : Set argument value
For the type System.String
DateTime? GetNextCronOccurrence() : Gets the next occurrence of a crontab string Expression
string GetXmValue(string xpath, string xPathIterationNode = "") : Get a xml value with a xpath expression
List<string> GetXmlNodes(string xPath, string xPathIterationNode = "") : Get xml values with a xpath expression
string AddXmlAttribute(string sourceXPath, string attributeName, List<string> values) : Add a xml attribute
string SetXmlAttributeValue(string xPath, List<string> values) : Set xml attribute value
string SetXmlValue(string xPath, List<string> values, XmlNodeOperation operation) : Set a xml value
string RemoveXmlObject(string xPath, XmlRemoveOperation operation): Remove a xml object
string TransformXml(this string xml, string xsl) : Transform a xml with a xsl
XmlReader GetXmlReader() : Get a xml reader
For the type System.Type
ImageDrawing GetImageDrawingFromResource(string resourceName) : Get ImageDrawing Instance From Resource Image File
ImageSource GetImageSourceFromResource(string resourceName) : Get ImageSource Instance From Resource Image File
For the type System.Linq.Expressions.ParameterExpression
Expression GetReferenceValue(Type type, LocationReference reference, bool isOutArgument = false) : Get the expression value of a location reference (variable) outside of the activity with the activity context expression
Expression GetReferenceValue<T>(LocationReference reference, bool isOutArgument = false) : Get the expression value of a location reference (variable) of type T outside of the activity with the activity context expression
For the type System.Linq.Expressions.Expression
Expression GetActivityExpressionValue<T>(Expression<Func<ActivityContext, T>> expression) : Get the value expression of an expression with result of type T