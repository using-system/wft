using System;
using System.Activities.DurableInstancing;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace WFT.Core
{
    /// <summary>
    /// Xml InstanceStore Provider
    /// <remarks>See http://msdn.microsoft.com/en-us/library/ee829481.aspx for more details about InstanceStore</remarks>
    /// </summary>
    public class XmlInstanceStore : InstanceStore
    {
        public static readonly string InstanceFormatString = "{0}.xml";
        public static readonly string PersistenceDirectory = Path.Combine(Path.GetTempPath(), "FilePersistenceProvider");

        Guid ownerInstanceID;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlInstanceStore"/> class.
        /// </summary>
        public XmlInstanceStore()
            : this(Guid.NewGuid())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlInstanceStore"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        public XmlInstanceStore(Guid id)
        {
            ownerInstanceID = id;
        }

        //Synchronous version of the Begin/EndTryCommand functions
        protected override bool TryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout)
        {
            return EndTryCommand(BeginTryCommand(context, command, timeout, null, null));
        }

        //The persistence engine will send a variety of commands to the configured InstanceStore,
        //such as CreateWorkflowOwnerCommand, SaveWorkflowCommand, and LoadWorkflowCommand.
        //This method is where we will handle those commands
        protected override IAsyncResult BeginTryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
        {
            IDictionary<XName, InstanceValue> data = null;

            //The CreateWorkflowOwner command instructs the instance store to create a new instance owner bound to the instanace handle
            if (command is CreateWorkflowOwnerCommand)
            {
                context.BindInstanceOwner(ownerInstanceID, Guid.NewGuid());
            }
            //The SaveWorkflow command instructs the instance store to modify the instance bound to the instance handle or an instance key
            else if (command is SaveWorkflowCommand)
            {
                SaveWorkflowCommand saveCommand = (SaveWorkflowCommand)command;
                data = saveCommand.InstanceData;

                Save(data);
            }
            //The LoadWorkflow command instructs the instance store to lock and load the instance bound to the identifier in the instance handle
            else if (command is LoadWorkflowCommand)
            {
                string fileName = GetFileName(this.ownerInstanceID);

                try
                {
                    using (FileStream inputStream = new FileStream(fileName, FileMode.Open))
                    {
                        data = LoadInstanceDataFromFile(inputStream);
                        //load the data into the persistence Context
                        context.LoadedInstance(InstanceState.Initialized, data, null, null, null);
                    }
                }
                catch
                {
                    throw;
                }
            }

            return new CompletedAsyncResult<bool>(true, callback, state);
        }

        /// <summary>
        /// Ends an asynchronous operation.
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <returns>
        /// A persistence provider implementation should return false if it doesn’t support the command passed to the BeginTryCommand method. Otherwise it should return true or throw an exception.
        /// </returns>
        protected override bool EndTryCommand(IAsyncResult result)
        {
            return CompletedAsyncResult<bool>.End(result);
        }

        /// <summary>
        /// Loads the instance data from file.
        /// </summary>
        /// <param name="inputStream">The input stream.</param>
        /// <returns></returns>
        IDictionary<XName, InstanceValue> LoadInstanceDataFromFile(Stream inputStream)
        {
            IDictionary<XName, InstanceValue> data = new Dictionary<XName, InstanceValue>();

            NetDataContractSerializer s = new NetDataContractSerializer();

            XmlReader rdr = XmlReader.Create(inputStream);
            XmlDocument doc = new XmlDocument();
            doc.Load(rdr);

            XmlNodeList instances = doc.GetElementsByTagName("InstanceValue");
            foreach (XmlElement instanceElement in instances)
            {
                XmlElement keyElement = (XmlElement)instanceElement.SelectSingleNode("descendant::key");
                XName key = (XName)DeserializeObject(s, keyElement);

                XmlElement valueElement = (XmlElement)instanceElement.SelectSingleNode("descendant::value");
                object value = DeserializeObject(s, valueElement);
                InstanceValue instVal = new InstanceValue(value);

                data.Add(key, instVal);
            }

            return data;
        }

        object DeserializeObject(NetDataContractSerializer serializer, XmlElement element)
        {
            object deserializedObject = null;

            MemoryStream stm = new MemoryStream();
            XmlDictionaryWriter wtr = XmlDictionaryWriter.CreateTextWriter(stm);
            element.WriteContentTo(wtr);
            wtr.Flush();
            stm.Position = 0;

            deserializedObject = serializer.Deserialize(stm);

            return deserializedObject;
        }

        //Saves the persistance data to an xml file.
        void Save(IDictionary<XName, InstanceValue> instanceData)
        {
            string fileName = GetFileName(this.ownerInstanceID);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<InstanceValues/>");

            foreach (KeyValuePair<XName, InstanceValue> valPair in instanceData)
            {
                XmlElement newInstance = doc.CreateElement("InstanceValue");

                XmlElement newKey = SerializeObject("key", valPair.Key, doc);
                newInstance.AppendChild(newKey);

                XmlElement newValue = SerializeObject("value", valPair.Value.Value, doc);
                newInstance.AppendChild(newValue);

                doc.DocumentElement.AppendChild(newInstance);
            }
            doc.Save(fileName);
        }

        XmlElement SerializeObject(string elementName, object o, XmlDocument doc)
        {
            NetDataContractSerializer s = new NetDataContractSerializer();
            XmlElement newElement = doc.CreateElement(elementName);
            MemoryStream stm = new MemoryStream();

            s.Serialize(stm, o);
            stm.Position = 0;
            StreamReader rdr = new StreamReader(stm);
            newElement.InnerXml = rdr.ReadToEnd();

            return newElement;
        }

        private string GetFileName(Guid id)
        {
            EnsurePersistenceFolderExists();
            return Path.Combine(PersistenceDirectory, string.Format(CultureInfo.InvariantCulture, InstanceFormatString, id));
        }

        private void EnsurePersistenceFolderExists()
        {
            if (!Directory.Exists(PersistenceDirectory))
            {
                Directory.CreateDirectory(PersistenceDirectory);
            }
        }

    }
}
