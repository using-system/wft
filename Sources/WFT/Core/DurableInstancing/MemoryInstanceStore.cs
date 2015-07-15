#region copyright

//  ----------------------------------------------------------------------------------
//  Microsoft
//  
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  
//  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
//  EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
//  OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//  ----------------------------------------------------------------------------------
//  The example companies, organizations, products, domain names,
//  e-mail addresses, logos, people, places, and events depicted
//  herein are fictitious.  No association with any real company,
//  organization, product, domain name, email address, logo, person,
//  places, or events is intended or should be inferred.
//  ----------------------------------------------------------------------------------

#endregion

using System;
using System.Activities.DurableInstancing;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.DurableInstancing;
using System.Transactions;
using System.Xml.Linq;

namespace WFT.Core
{
    /// <summary>
    /// Memory Instance Store
    /// </summary>
    public class MemoryInstanceStore : InstanceStore
    {
        #region Constants and Fields

        /// <summary>
        /// The lock obj.
        /// </summary>
        private static readonly object LockObj = new object();

        /// <summary>
        /// The notification managers.
        /// </summary>
        private static readonly Dictionary<Transaction, TransactionNotificationManager> NotificationManagers =
            new Dictionary<Transaction, TransactionNotificationManager>();

        /// <summary>
        /// The serialization.
        /// </summary>
        private readonly SerializationProvider serialization;

        /// <summary>
        /// The instances.
        /// </summary>
        private static TransactionAwareDictionary<Instance> instances = new TransactionAwareDictionary<Instance>();

        /// <summary>
        /// The instance keys.
        /// </summary>
        private static TransactionAwareDictionary<Key> keys = new TransactionAwareDictionary<Key>();

        /// <summary>
        /// The owners.
        /// </summary>
        private static TransactionAwareDictionary<Owner> owners = new TransactionAwareDictionary<Owner>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStore"/> class.
        /// </summary>
        public MemoryInstanceStore()
            : this(new Type[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStore"/> class.
        /// </summary>
        /// <param name="knownTypes">
        /// The known types.
        /// </param>
        public MemoryInstanceStore(params Type[] knownTypes)
        {
            this.serialization = new SerializationProvider(knownTypes);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStore"/> class.
        /// </summary>
        /// <param name="knownTypes">
        /// The known types.
        /// </param>
        protected MemoryInstanceStore(IEnumerable<Type> knownTypes)
        {
            this.serialization = new SerializationProvider(knownTypes);
        }

        #endregion

        #region Interfaces

        /// <summary>
        /// pending changes container.
        /// </summary>
        private interface IPendingChangesContainer
        {
            #region Public Methods

            /// <summary>
            /// The apply pending changes.
            /// </summary>
            /// <param name="tx">
            /// The transaction
            /// </param>
            void ApplyPendingChanges(Transaction tx);

            /// <summary>
            /// The cancel pending changes.
            /// </summary>
            /// <param name="tx">
            /// The transaction
            /// </param>
            void CancelPendingChanges(Transaction tx);

            #endregion
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets CreateWorkflowOwnerCommandCount.
        /// </summary>
        public static int CreateWorkflowOwnerCommandCount { get; private set; }

        /// <summary>
        /// Gets DeleteWorkflowOwnerCommandCount.
        /// </summary>
        public static int DeleteWorkflowOwnerCommandCount { get; private set; }

        /// <summary>
        /// Gets LoadWorkflowByInstanceKeyCommandCount.
        /// </summary>
        public static int LoadWorkflowByInstanceKeyCommandCount { get; private set; }

        /// <summary>
        /// Gets LoadWorkflowCommandCount.
        /// </summary>
        public static int LoadWorkflowCommandCount { get; private set; }

        /// <summary>
        /// Gets SaveWorkflowCommandCount.
        /// </summary>
        public static int SaveWorkflowCommandCount { get; private set; }

        /// <summary>
        /// Gets TryLoadRunnableWorkflowCommandCount.
        /// </summary>
        public static int TryLoadRunnableWorkflowCommandCount { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// The contains key.
        /// </summary>
        /// <param name="keyToSearchFor">
        /// The key to search for.
        /// </param>
        /// <returns>
        /// True if the key is in the store
        /// </returns>
        public static bool ContainsKey(Guid keyToSearchFor)
        {
            lock (LockObj)
            {
                Key key;
                return keys.TryGetValueExclusively(keyToSearchFor, out key);
            }
        }

        /// <summary>
        /// Displays command counts.
        /// </summary>
        public static void DisplayCommandCounts()
        {
            Trace.WriteLine(">> MemoryStore Command Counts <<");
            Trace.WriteLine("LoadWorkflowCommand count = " + LoadWorkflowCommandCount);
            Trace.WriteLine("LoadWorkflowByInstanceKeyCommand count = " + LoadWorkflowByInstanceKeyCommandCount);
            Trace.WriteLine("SaveWorkflowCommand count = " + SaveWorkflowCommandCount);
            Trace.WriteLine("CreateWorkflowOwnerCommand count = " + CreateWorkflowOwnerCommandCount);
            Trace.WriteLine("DeleteWorkflowOwnerCommand count = " + DeleteWorkflowOwnerCommandCount);
        }

        /// <summary>
        /// Dumps the store 
        /// </summary>
        public static void Dump()
        {
            Trace.WriteLine(string.Empty);
            Trace.WriteLine(">> Begin MemoryStore Dump <<");
            Trace.WriteLine(">> Owners");
            Debug.Indent();
            foreach (var owner in owners)
            {
                Trace.WriteLine("Owner " + owner.Key);
                Debug.Indent();
                Trace.WriteLine("LockToken: " + owner.Value.LockToken);
                DumpPropertyBag("Metadata", owner.Value.Data);
                Debug.Unindent();
            }

            Debug.Unindent();

            Trace.WriteLine(">> Instances");
            Debug.Indent();
            foreach (var instance in instances)
            {
                Trace.WriteLine("Instance " + instance.Key);
                Debug.Indent();
                Trace.WriteLine("Owner:       " + instance.Value.Owner);
                Trace.WriteLine("LockVersion: " + instance.Value.LockVersion);
                Trace.WriteLine("Completed:   " + (instance.Value.Completed ? "True" : "False"));
                DumpPropertyBag("Data", instance.Value.Data);
                DumpPropertyBag("Metadata", instance.Value.Metadata);
                Debug.Unindent();
            }

            Debug.Unindent();

            Trace.WriteLine(">> Keys");
            Debug.Indent();
            foreach (var key in keys)
            {
                Trace.WriteLine("Key " + key.Key);
                Debug.Indent();
                Trace.WriteLine("Instance:  " + key.Value.Instance);
                Trace.WriteLine("Completed: " + (key.Value.Completed ? "True" : "False"));
                DumpPropertyBag("Metadata", key.Value.Metadata);
            }

            Debug.Unindent();
            Trace.WriteLine(">> End MemoryStore Dump <<");
            Trace.WriteLine(string.Empty);
        }

        /// <summary>
        /// The is instance completed.
        /// </summary>
        /// <param name="instanceId">
        /// The instance id.
        /// </param>
        /// <returns>
        /// True if the instance has completed
        /// </returns>
        public static bool IsInstanceCompleted(Guid instanceId)
        {
            lock (LockObj)
            {
                Instance instance;

                if (!instances.TryGetValue(instanceId, out instance))
                {
                    return false;
                }

                return instance.Completed;
            }
        }

        /// <summary>
        /// The is instance initialized.
        /// </summary>
        /// <param name="instanceId">
        /// The instance id.
        /// </param>
        /// <returns>
        /// true if the instance is initialized.
        /// </returns>
        public static bool IsInstanceInitialized(Guid instanceId)
        {
            lock (LockObj)
            {
                Instance instance;
                if (!instances.TryGetValue(instanceId, out instance))
                {
                    return false;
                }

                return (instance.Data != null) && !instance.Completed;
            }
        }

        /// <summary>
        /// Resets the memory store
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public static void Reset()
        {
            lock (LockObj)
            {
                if (NotificationManagers.Count > 0)
                {
                    throw new InvalidOperationException(
                        "Can't reset the store while having pending transactional changes");
                }

                instances = new TransactionAwareDictionary<Instance>();
                keys = new TransactionAwareDictionary<Key>();
                owners = new TransactionAwareDictionary<Owner>();

                LoadWorkflowCommandCount = 0;
                LoadWorkflowByInstanceKeyCommandCount = 0;
                SaveWorkflowCommandCount = 0;
                CreateWorkflowOwnerCommandCount = 0;
                DeleteWorkflowOwnerCommandCount = 0;
            }
        }

        /// <summary>
        /// The poll events.
        /// </summary>
        public void PollEvents()
        {
            lock (LockObj)
            {
                foreach (var instanceOwner in this.GetInstanceOwners())
                {
                    Owner owner;
                    if (!owners.TryGetValueExclusively(instanceOwner.InstanceOwnerId, out owner))
                    {
                        continue;
                    }

                    XName ownerService = null;
                    InstanceValue value = this.QueryPropertyBag(WorkflowNamespace.WorkflowHostType, owner.Data);
                    if (value != null && value.Value is XName)
                    {
                        ownerService = (XName)value.Value;
                    }

                    foreach (var persistenceEvent in this.GetEvents(instanceOwner))
                    {
                        if (persistenceEvent.Name == HasRunnableWorkflowEvent.Value.Name)
                        {
                            var foundRunnableInstace = false;
                            foreach (var instance in instances)
                            {
                                if (!instance.Value.Completed)
                                {
                                    if (ownerService != null)
                                    {
                                        value = this.QueryPropertyBag(
                                            WorkflowNamespace.WorkflowHostType, instance.Value.Metadata);
                                        if (value == null || ((XName)value.Value) != ownerService)
                                        {
                                            continue;
                                        }
                                    }

                                    value = this.QueryPropertyBag(
                                        WorkflowNamespace.WorkflowHostType, instance.Value.Metadata);
                                    if (value != null && value.Value != null && value.Value is string)
                                    {
                                        continue;
                                    }

                                    value = this.QueryPropertyBag(WorkflowNamespace.Status, instance.Value.Data);
                                    if (value != null && value.Value is string && ((string)value.Value) == "Executing")
                                    {
                                        foundRunnableInstace = true;
                                        break;
                                    }

                                    value =
                                        this.QueryPropertyBag(
                                            XNamespace.Get("urn:schemas-microsoft-com:System.Activities/4.0/properties")
                                                .GetName("TimerExpirationTime"),
                                            instance.Value.Data);
                                    if (value != null && value.Value is DateTime &&
                                        ((DateTime)value.Value) <= DateTime.UtcNow)
                                    {
                                        foundRunnableInstace = true;
                                        break;
                                    }
                                }
                            }

                            if (foundRunnableInstace)
                            {
                                this.SignalEvent(persistenceEvent, instanceOwner);
                            }
                            else
                            {
                                this.ResetEvent(persistenceEvent, instanceOwner);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The begin try command.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="timeout">
        /// The timeout.
        /// </param>
        /// <param name="callback">
        /// The callback.
        /// </param>
        /// <param name="state">
        /// The state.
        /// </param>
        /// <returns>
        /// </returns>
        protected override IAsyncResult BeginTryCommand(
            InstancePersistenceContext context,
            InstancePersistenceCommand command,
            TimeSpan timeout,
            AsyncCallback callback,
            object state)
        {
            long reclaimLockAtVersion = 0;

            var save = command as SaveWorkflowCommand;
            if (save != null)
            {
                Trace.WriteLine("MemoryStore: SaveWorkflowCommand");
                SaveWorkflowCommandCount = SaveWorkflowCommandCount + 1;

                lock (LockObj)
                {
                    reclaimLockAtVersion = this.ProcessSaveCommand(context, save);
                }
            }

            var load = command as LoadWorkflowCommand;
            if (load != null)
            {
                Trace.WriteLine("MemoryStore: LoadWorkflowCommand");
                LoadWorkflowCommandCount = LoadWorkflowCommandCount + 1;

                lock (LockObj)
                {
                    reclaimLockAtVersion = this.ProcessLoadCommand(context, load);
                }
            }

            var loadByKey = command as LoadWorkflowByInstanceKeyCommand;
            if (loadByKey != null)
            {
                Trace.WriteLine("MemoryStore: LoadWorkflowByInstanceKeyCommand");
                LoadWorkflowByInstanceKeyCommandCount = LoadWorkflowByInstanceKeyCommandCount + 1;

                lock (LockObj)
                {
                    reclaimLockAtVersion = this.ProcessLoadByKeyCommand(context, loadByKey);
                }
            }

            if (save != null || load != null || loadByKey != null)
            {
                if (reclaimLockAtVersion != 0)
                {
                    return new BindReclaimedLockAsyncResult(
                        this, context, save, load, loadByKey, reclaimLockAtVersion, callback, state);
                }
                else
                {
                    return new CompletedAsyncResult(callback, state);
                }
            }

            var createOwner = command as CreateWorkflowOwnerCommand;
            if (createOwner != null)
            {
                Trace.WriteLine("MemoryStore: CreateWorkflowOwnerCommand");

                CreateWorkflowOwnerCommandCount = CreateWorkflowOwnerCommandCount + 1;

                var ownerId = Guid.NewGuid();
                var owner = new Owner();
                lock (LockObj)
                {
                    owner.LockToken = Guid.NewGuid();
                    owner.Data = this.SerializePropertyBag(createOwner.InstanceOwnerMetadata);
                    owners.Add(ownerId, owner);
                }

                context.BindInstanceOwner(ownerId, owner.LockToken);
                context.BindEvent(HasRunnableWorkflowEvent.Value);
                return new CompletedAsyncResult(callback, state);
            }

            var deleteOwner = command as DeleteWorkflowOwnerCommand;
            if (deleteOwner != null)
            {
                Trace.WriteLine("MemoryStore: DeleteWorkflowOwnerCommand");
                DeleteWorkflowOwnerCommandCount = DeleteWorkflowOwnerCommandCount + 1;

                var ownerId = context.InstanceView.InstanceOwner.InstanceOwnerId;

                lock (LockObj)
                {
                    Owner owner;
                    if (owners.TryGetValueExclusively(ownerId, out owner) && owner.LockToken == context.LockToken)
                    {
                        owners.Remove(ownerId);
                    }
                }

                context.InstanceHandle.Free();
                return new CompletedAsyncResult(callback, state);
            }

            var tryLoad = command as TryLoadRunnableWorkflowCommand;
            if (tryLoad != null)
            {
                Trace.WriteLine("MemoryStore: TryLoadRunnableWorkflowCommand");
                TryLoadRunnableWorkflowCommandCount = TryLoadRunnableWorkflowCommandCount + 1;

                lock (LockObj)
                {
                    this.ProcessTryLoadRunnableCommand(context, tryLoad);
                }

                return new CompletedAsyncResult(callback, state);
            }

            return base.BeginTryCommand(context, command, timeout, callback, state);
        }

        /// <summary>
        /// The end try command.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// The end try command.
        /// </returns>
        protected override bool EndTryCommand(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result);
            }
            else if (result is BindReclaimedLockAsyncResult)
            {
                BindReclaimedLockAsyncResult.End(result);
            }
            else
            {
                return base.EndTryCommand(result);
            }

            return true;
        }

        /// <summary>
        /// The on new instance handle.
        /// </summary>
        /// <param name="instanceHandle">
        /// The instance handle.
        /// </param>
        /// <returns>
        /// The on new instance handle.
        /// </returns>
        protected override object OnNewInstanceHandle(InstanceHandle instanceHandle)
        {
            return new MemoryContext(instanceHandle);
        }

        /// <summary>
        /// The clone memory stream.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// </returns>
        private static MemoryStream CloneMemoryStream(MemoryStream source)
        {
            var destination = new MemoryStream();
            var currentSourcePosition = source.Position;
            source.Position = 0;
            var buffer = new byte[4096];
            int bytesToCopy;
            do
            {
                bytesToCopy = source.Read(buffer, 0, 4096);
                if (bytesToCopy > 0)
                {
                    destination.Write(buffer, 0, bytesToCopy);
                }
            }
            while (bytesToCopy > 0);
            destination.Position = source.Position = currentSourcePosition;
            return destination;
        }

        /// <summary>
        /// The dump property bag.
        /// </summary>
        /// <param name="bagName">
        /// The bag name.
        /// </param>
        /// <param name="bag">
        /// The bag.
        /// </param>
        private static void DumpPropertyBag(string bagName, Dictionary<XName, SerializedValue> bag)
        {
            if (bag == null)
            {
                return;
            }

            Trace.WriteLine(bagName);
            Debug.Indent();
            foreach (var property in bag)
            {
                Trace.WriteLine("Property: " + property.Key + (property.Value.WriteOnly ? " (WriteOnly)" : string.Empty));
                Trace.WriteLine(">> Begin Data -------------------------------------------------------");
                if (property.Value.Buffer != null)
                {
                    Trace.WriteLine(new StreamReader(property.Value.Buffer).ReadToEnd());
                    property.Value.Buffer.Seek(0, SeekOrigin.Begin);
                }

                Trace.WriteLine(">> End Data-------------------------------------------------------");
            }

            Debug.Unindent();
        }

        /// <summary>
        /// The notify pending changes.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        private static void NotifyPendingChanges(IPendingChangesContainer container)
        {
            TransactionNotificationManager manager;
            if (!NotificationManagers.TryGetValue(Transaction.Current, out manager))
            {
                manager = new TransactionNotificationManager(Transaction.Current);
                NotificationManagers.Add(Transaction.Current, manager);
            }

            manager.AddContainer(container);
        }

        /// <summary>
        /// The check owner.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="commandName">
        /// The command name.
        /// </param>
        /// <param name="owner">
        /// The owner.
        /// </param>
        /// <exception cref="InstanceOwnerException">
        /// </exception>
        private void CheckOwner(InstancePersistenceContext context, XName commandName, out Owner owner)
        {
            if (!owners.TryGetValue(context.InstanceView.InstanceOwner.InstanceOwnerId, out owner) ||
                owner.LockToken != context.LockToken)
            {
                context.InstanceHandle.Free();
                throw new InstanceOwnerException(commandName, context.InstanceView.InstanceOwner.InstanceOwnerId);
            }
        }

        /// <summary>
        /// The create instance locked exception.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="commandName">
        /// The command name.
        /// </param>
        /// <param name="instanceOwner">
        /// The instance owner.
        /// </param>
        /// <returns>
        /// </returns>
        private InstanceLockedException CreateInstanceLockedException(
            InstancePersistenceContext context, XName commandName, Guid instanceOwner)
        {
            // Query the owner metadata.
            Owner foreignOwner;
            Dictionary<XName, InstanceValue> metadata = null;
            if (owners.TryGetValueExclusively(instanceOwner, out foreignOwner))
            {
                metadata = this.DeserializePropertyBag(foreignOwner.Data);
            }

            context.QueriedInstanceStore(new InstanceOwnerQueryResult(instanceOwner, metadata));

            Dictionary<XName, object> serializableMetadata = null;
            if (metadata != null && metadata.Count > 0)
            {
                serializableMetadata = new Dictionary<XName, object>(metadata.Count);
                foreach (var value in metadata)
                {
                    if (value.Value.Value == null || value.Value.Value.GetType().IsSerializable)
                    {
                        serializableMetadata.Add(value.Key, value.Value.Value);
                    }
                }
            }

            if (serializableMetadata != null && serializableMetadata.Count > 0)
            {
                return new InstanceLockedException(
                    commandName, context.InstanceView.InstanceId, instanceOwner, serializableMetadata);
            }
            else
            {
                return new InstanceLockedException(commandName, context.InstanceView.InstanceId, null);
            }
        }

        /// <summary>
        /// The deserialize instance value.
        /// </summary>
        /// <param name="serial">
        /// The serial.
        /// </param>
        /// <returns>
        /// </returns>
        private InstanceValue DeserializeInstanceValue(SerializedValue serial)
        {
            if (serial.Buffer == null)
            {
                return new InstanceValue(null);
            }
            else
            {
                var value = new InstanceValue(this.serialization.ReadInstance(serial.Buffer));
                serial.Buffer.Seek(0, SeekOrigin.Begin);
                return value;
            }
        }

        /// <summary>
        /// The deserialize property bag.
        /// </summary>
        /// <param name="serial">
        /// The serial.
        /// </param>
        /// <returns>
        /// </returns>
        private Dictionary<XName, InstanceValue> DeserializePropertyBag(Dictionary<XName, SerializedValue> serial)
        {
            if (serial == null)
            {
                return null;
            }

            return serial.Where(property => !property.Value.WriteOnly).ToDictionary(
                property => property.Key, property => this.DeserializeInstanceValue(property.Value));
        }

        /// <summary>
        /// The process load by key command.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <returns>
        /// The process load by key command.
        /// </returns>
        /// <exception cref="InstanceLockLostException">
        /// </exception>
        /// <exception cref="InstanceKeyNotReadyException">
        /// </exception>
        /// <exception cref="InstanceLockLostException">
        /// </exception>
        /// <exception cref="InvalidProgramException">
        /// </exception>
        /// <exception cref="InstanceLockLostException">
        /// </exception>
        /// <exception cref="InstanceCollisionException">
        /// </exception>
        /// <exception cref="InstanceLockedException">
        /// </exception>
        /// <exception cref="InstanceLockLostException">
        /// </exception>
        /// <exception cref="InstanceKeyCompleteException">
        /// </exception>
        /// <exception cref="InvalidProgramException">
        /// </exception>
        /// <exception cref="InstanceLockLostException">
        /// </exception>
        /// <exception cref="InstanceNotReadyException">
        /// </exception>
        /// <exception cref="InstanceLockedException">
        /// </exception>
        /// <exception cref="Exception">
        /// </exception>
        private long ProcessLoadByKeyCommand(
            InstancePersistenceContext context, LoadWorkflowByInstanceKeyCommand command)
        {
            Owner owner;
            this.CheckOwner(context, command.Name, out owner);

            Key key;
            Instance instance;
            if (!keys.TryGetValue(command.LookupInstanceKey, out key))
            {
                if (context.InstanceView.IsBoundToLock &&
                    context.InstanceView.InstanceId != command.AssociateInstanceKeyToInstanceId)
                {
                    // This happens in the bind reclaimed lock case.
                    context.InstanceHandle.Free();
                    throw new InstanceLockLostException(command.Name, context.InstanceView.InstanceId);
                }

                if (command.AssociateInstanceKeyToInstanceId == Guid.Empty)
                {
                    throw new InstanceKeyNotReadyException(command.Name, new InstanceKey(command.LookupInstanceKey));
                }

                if (!instances.TryGetValue(command.AssociateInstanceKeyToInstanceId, out instance))
                {
                    // Checking instance.Owner is like an InstanceLockQueryResult.
                    context.QueriedInstanceStore(
                        new InstanceLockQueryResult(command.AssociateInstanceKeyToInstanceId, Guid.Empty));

                    if (context.InstanceView.IsBoundToLock)
                    {
                        // This happens in the bind reclaimed lock case.
                        context.InstanceHandle.Free();
                        throw new InstanceLockLostException(command.Name, context.InstanceView.InstanceId);
                    }

                    context.BindInstance(command.AssociateInstanceKeyToInstanceId);
                    instance.Completed = false;
                    instance.LockVersion = 1;
                    instance.Owner = context.InstanceView.InstanceOwner.InstanceOwnerId;
                    instance.Metadata = new Dictionary<XName, SerializedValue>();
                    instances.Add(command.AssociateInstanceKeyToInstanceId, instance);
                    context.BindAcquiredLock(1);
                }
                else
                {
                    // Checking instance.Owner is like an InstanceLockQueryResult.
                    context.QueriedInstanceStore(
                        new InstanceLockQueryResult(command.AssociateInstanceKeyToInstanceId, instance.Owner));

                    if (context.InstanceView.IsBoundToLock)
                    {
                        if (instance.LockVersion != context.InstanceVersion ||
                            instance.Owner != context.InstanceView.InstanceOwner.InstanceOwnerId)
                        {
                            if (context.InstanceVersion > instance.LockVersion)
                            {
                                throw new InvalidProgramException(
                                    "This is a bug, the context should never be bound higher than the lock.");
                            }

                            context.InstanceHandle.Free();
                            throw new InstanceLockLostException(command.Name, context.InstanceView.InstanceId);
                        }
                    }

                    if (instance.Data != null)
                    {
                        // LoadByInstanceKeyCommand only allows auto-association to an uninitialized instance.
                        throw new InstanceCollisionException(command.Name, command.AssociateInstanceKeyToInstanceId);
                    }

                    if (!context.InstanceView.IsBoundToLock)
                    {
                        if (instance.Owner == Guid.Empty)
                        {
                            context.BindInstance(command.AssociateInstanceKeyToInstanceId);
                            instance.LockVersion++;
                            instance.Owner = context.InstanceView.InstanceOwner.InstanceOwnerId;
                            instances[command.AssociateInstanceKeyToInstanceId] = instance;
                            context.BindAcquiredLock(instance.LockVersion);
                        }
                        else if (instance.Owner == context.InstanceView.InstanceOwner.InstanceOwnerId)
                        {
                            // This is a pretty weird case - maybe it's a retry?
                            context.BindInstance(command.AssociateInstanceKeyToInstanceId);
                            return instance.LockVersion;
                        }
                        else
                        {
                            throw this.CreateInstanceLockedException(context, command.Name, instance.Owner);
                        }
                    }
                }

                IDictionary<XName, InstanceValue> lookupKeyMetadata;
                key.Metadata = command.InstanceKeysToAssociate.TryGetValue(command.LookupInstanceKey, out lookupKeyMetadata) ? this.SerializePropertyBag(lookupKeyMetadata) : new Dictionary<XName, SerializedValue>();

                key.Instance = command.AssociateInstanceKeyToInstanceId;
                keys.Add(command.LookupInstanceKey, key);
                context.AssociatedInstanceKey(command.LookupInstanceKey);
                if (lookupKeyMetadata != null)
                {
                    foreach (var property in lookupKeyMetadata)
                    {
                        context.WroteInstanceKeyMetadataValue(command.LookupInstanceKey, property.Key, property.Value);
                    }
                }
            }
            else
            {
                if (context.InstanceView.IsBoundToLock &&
                    (key.Completed || key.Instance != context.InstanceView.InstanceId))
                {
                    // This happens in the bind reclaimed lock case.
                    context.InstanceHandle.Free();
                    throw new InstanceLockLostException(command.Name, context.InstanceView.InstanceId);
                }

                if (key.Completed)
                {
                    throw new InstanceKeyCompleteException(command.Name, new InstanceKey(command.LookupInstanceKey));
                }

                instance = instances[key.Instance];
                Debug.Assert(!instance.Completed, "The key would be completed in this case.");

                // Checking instance.Owner is like an InstanceLockQueryResult.
                context.QueriedInstanceStore(new InstanceLockQueryResult(key.Instance, instance.Owner));

                if (context.InstanceView.IsBoundToLock)
                {
                    if (instance.LockVersion != context.InstanceVersion ||
                        instance.Owner != context.InstanceView.InstanceOwner.InstanceOwnerId)
                    {
                        if (context.InstanceVersion > instance.LockVersion)
                        {
                            throw new InvalidProgramException(
                                "This is a bug, the context should never be bound higher than the lock.");
                        }

                        context.InstanceHandle.Free();
                        throw new InstanceLockLostException(command.Name, context.InstanceView.InstanceId);
                    }
                }

                if (instance.Data == null && !command.AcceptUninitializedInstance)
                {
                    throw new InstanceNotReadyException(command.Name, key.Instance);
                }

                if (!context.InstanceView.IsBoundToLock)
                {
                    if (instance.Owner == Guid.Empty)
                    {
                        context.BindInstance(key.Instance);
                        instance.LockVersion++;
                        instance.Owner = context.InstanceView.InstanceOwner.InstanceOwnerId;
                        instances[key.Instance] = instance;
                        context.BindAcquiredLock(instance.LockVersion);
                    }
                    else if (instance.Owner == context.InstanceView.InstanceOwner.InstanceOwnerId)
                    {
                        // This is the very interesting parallel-convoy conflicting handle race resolution case.  Two handles
                        // can get bound to the same lock, which is necessary to allow parallel convoy to succeed without preventing
                        // zombied locked instances from being reclaimed.
                        context.BindInstance(key.Instance);
                        return instance.LockVersion;
                    }
                    else
                    {
                        throw this.CreateInstanceLockedException(context, command.Name, instance.Owner);
                    }
                }
            }

            Exception exception = null;
            foreach (var keyEntry in command.InstanceKeysToAssociate)
            {
                Key newKey;
                if (!keys.TryGetValue(keyEntry.Key, out newKey))
                {
                    newKey.Completed = false;
                    newKey.Metadata = this.SerializePropertyBag(keyEntry.Value);
                    newKey.Instance = key.Instance;
                    keys.Add(keyEntry.Key, newKey);
                    context.AssociatedInstanceKey(keyEntry.Key);
                    if (keyEntry.Value != null)
                    {
                        foreach (var property in keyEntry.Value)
                        {
                            context.WroteInstanceKeyMetadataValue(keyEntry.Key, property.Key, property.Value);
                        }
                    }
                }
                else
                {
                    if (newKey.Instance != key.Instance && exception == null)
                    {
                        exception = new InstanceKeyCollisionException(
                            command.Name, key.Instance, new InstanceKey(keyEntry.Key), newKey.Instance);
                    }
                }
            }

            if (exception != null)
            {
                throw exception;
            }

            var associatedKeys = new Dictionary<Guid, IDictionary<XName, InstanceValue>>();
            var completedKeys = new Dictionary<Guid, IDictionary<XName, InstanceValue>>();
            foreach (var keyEntry in keys.Where(keyEntry => keyEntry.Value.Instance == key.Instance))
            {
                if (keyEntry.Value.Completed)
                {
                    completedKeys.Add(keyEntry.Key, this.DeserializePropertyBag(keyEntry.Value.Metadata));
                }
                else
                {
                    associatedKeys.Add(keyEntry.Key, this.DeserializePropertyBag(keyEntry.Value.Metadata));
                }
            }

            context.LoadedInstance(
                instance.Data == null ? InstanceState.Uninitialized : InstanceState.Initialized,
                this.DeserializePropertyBag(instance.Data),
                this.DeserializePropertyBag(instance.Metadata),
                associatedKeys,
                completedKeys);

            return 0;
        }

        /// <summary>
        /// The process load command.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <returns>
        /// The process load command.
        /// </returns>
        /// <exception cref="InstanceLockLostException">
        /// </exception>
        /// <exception cref="InstanceNotReadyException">
        /// </exception>
        /// <exception cref="InvalidProgramException">
        /// </exception>
        /// <exception cref="InstanceLockLostException">
        /// </exception>
        /// <exception cref="InstanceCompleteException">
        /// </exception>
        /// <exception cref="InstanceNotReadyException">
        /// </exception>
        /// <exception cref="InstanceLockedException">
        /// </exception>
        private long ProcessLoadCommand(InstancePersistenceContext context, LoadWorkflowCommand command)
        {
            Owner owner;
            this.CheckOwner(context, command.Name, out owner);

            Instance instance;
            if (!instances.TryGetValue(context.InstanceView.InstanceId, out instance))
            {
                // Checking instance.Owner is like an InstanceLockQueryResult.
                context.QueriedInstanceStore(new InstanceLockQueryResult(context.InstanceView.InstanceId, Guid.Empty));

                if (context.InstanceView.IsBoundToLock)
                {
                    context.InstanceHandle.Free();
                    throw new InstanceLockLostException(command.Name, context.InstanceView.InstanceId);
                }

                if (!command.AcceptUninitializedInstance)
                {
                    throw new InstanceNotReadyException(command.Name, context.InstanceView.InstanceId);
                }

                instance.Completed = false;
                instance.LockVersion = 1;
                instance.Owner = context.InstanceView.InstanceOwner.InstanceOwnerId;
                instance.Metadata = new Dictionary<XName, SerializedValue>();
                instances.Add(context.InstanceView.InstanceId, instance);
                context.BindAcquiredLock(1);
            }
            else
            {
                // Checking instance.Owner is like an InstanceLockQueryResult.
                context.QueriedInstanceStore(
                    new InstanceLockQueryResult(context.InstanceView.InstanceId, instance.Owner));

                if (context.InstanceView.IsBoundToLock)
                {
                    if (instance.LockVersion != context.InstanceVersion ||
                        instance.Owner != context.InstanceView.InstanceOwner.InstanceOwnerId)
                    {
                        if (context.InstanceVersion > instance.LockVersion)
                        {
                            throw new InvalidProgramException(
                                "This is a bug, the context should never be bound higher than the lock.");
                        }

                        context.InstanceHandle.Free();
                        throw new InstanceLockLostException(command.Name, context.InstanceView.InstanceId);
                    }
                }

                if (instance.Completed)
                {
                    throw new InstanceCompleteException(command.Name, context.InstanceView.InstanceId);
                }

                if (instance.Data == null && !command.AcceptUninitializedInstance)
                {
                    throw new InstanceNotReadyException(command.Name, context.InstanceView.InstanceId);
                }

                if (!context.InstanceView.IsBoundToLock)
                {
                    if (instance.Owner == Guid.Empty)
                    {
                        instance.LockVersion++;
                        instance.Owner = context.InstanceView.InstanceOwner.InstanceOwnerId;
                        instances[context.InstanceView.InstanceId] = instance;
                        context.BindAcquiredLock(instance.LockVersion);
                    }
                    else if (instance.Owner == context.InstanceView.InstanceOwner.InstanceOwnerId)
                    {
                        // This is the very interesting parallel-convoy conflicting handle race resolution case.  Two handles
                        // can get bound to the same lock, which is necessary to allow parallel convoy to succeed without preventing
                        // zombied locked instances from being reclaimed.
                        return instance.LockVersion;
                    }
                    else
                    {
                        throw this.CreateInstanceLockedException(context, command.Name, instance.Owner);
                    }
                }
            }

            var associatedKeys = new Dictionary<Guid, IDictionary<XName, InstanceValue>>();
            var completedKeys = new Dictionary<Guid, IDictionary<XName, InstanceValue>>();
            foreach (var keyEntry in keys)
            {
                if (keyEntry.Value.Instance == context.InstanceView.InstanceId)
                {
                    if (keyEntry.Value.Completed)
                    {
                        completedKeys.Add(keyEntry.Key, this.DeserializePropertyBag(keyEntry.Value.Metadata));
                    }
                    else
                    {
                        associatedKeys.Add(keyEntry.Key, this.DeserializePropertyBag(keyEntry.Value.Metadata));
                    }
                }
            }

            context.LoadedInstance(
                instance.Data == null ? InstanceState.Uninitialized : InstanceState.Initialized,
                this.DeserializePropertyBag(instance.Data),
                this.DeserializePropertyBag(instance.Metadata),
                associatedKeys,
                completedKeys);

            return 0;
        }

        /// <summary>
        /// The process save command.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <returns>
        /// The process save command.
        /// </returns>
        /// <exception cref="InstanceLockLostException">
        /// </exception>
        /// <exception cref="InstanceCompleteException">
        /// </exception>
        /// <exception cref="InstanceLockLostException">
        /// </exception>
        /// <exception cref="InstanceLockLostException">
        /// </exception>
        /// <exception cref="InstanceLockedException">
        /// </exception>
        /// <exception cref="InvalidProgramException">
        /// </exception>
        /// <exception cref="InstanceLockLostException">
        /// </exception>
        /// <exception cref="InstanceKeyCollisionException">
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        private long ProcessSaveCommand(InstancePersistenceContext context, SaveWorkflowCommand command)
        {
            Owner owner;
            this.CheckOwner(context, command.Name, out owner);

            Instance instance;
            if (!instances.TryGetValueExclusively(context.InstanceView.InstanceId, out instance))
            {
                // Checking instance.Owner is like an InstanceLockQueryResult.
                context.QueriedInstanceStore(new InstanceLockQueryResult(context.InstanceView.InstanceId, Guid.Empty));

                if (context.InstanceView.IsBoundToLock)
                {
                    context.InstanceHandle.Free();
                    throw new InstanceLockLostException(command.Name, context.InstanceView.InstanceId);
                }

                instance.Completed = false;
                instance.LockVersion = 1;
                instance.Owner = context.InstanceView.InstanceOwner.InstanceOwnerId;
                instance.Metadata = new Dictionary<XName, SerializedValue>();
                instances.Add(context.InstanceView.InstanceId, instance);
                context.BindAcquiredLock(1);
            }
            else
            {
                // Checking instance.Owner is like an InstanceLockQueryResult.
                context.QueriedInstanceStore(
                    new InstanceLockQueryResult(context.InstanceView.InstanceId, instance.Owner));

                if (instance.Completed)
                {
                    throw new InstanceCompleteException(command.Name, context.InstanceView.InstanceId);
                }

                if (instance.Owner == Guid.Empty)
                {
                    if (context.InstanceView.IsBoundToLock)
                    {
                        context.InstanceHandle.Free();
                        throw new InstanceLockLostException(command.Name, context.InstanceView.InstanceId);
                    }

                    instance.LockVersion++;
                    instance.Owner = context.InstanceView.InstanceOwner.InstanceOwnerId;
                    instances[context.InstanceView.InstanceId] = instance;
                    context.BindAcquiredLock(instance.LockVersion);
                }
                else
                {
                    if (instance.Owner != context.InstanceView.InstanceOwner.InstanceOwnerId)
                    {
                        if (context.InstanceView.IsBoundToLock)
                        {
                            context.InstanceHandle.Free();
                            throw new InstanceLockLostException(command.Name, context.InstanceView.InstanceId);
                        }

                        throw this.CreateInstanceLockedException(context, command.Name, instance.Owner);
                    }

                    if (context.InstanceView.IsBoundToLock)
                    {
                        if (context.InstanceVersion != instance.LockVersion)
                        {
                            if (context.InstanceVersion > instance.LockVersion)
                            {
                                throw new InvalidProgramException(
                                    "This is a bug, the context should never be bound higher than the lock.");
                            }

                            context.InstanceHandle.Free();
                            throw new InstanceLockLostException(command.Name, context.InstanceView.InstanceId);
                        }
                    }
                    else
                    {
                        // This is the very interesting parallel-convoy conflicting handle race resolution case.  Two handles
                        // can get bound to the same lock, which is necessary to allow parallel convoy to succeed without preventing
                        // zombied locked instances from being reclaimed.
                        return instance.LockVersion;
                    }
                }
            }

            foreach (var keyEntry in command.InstanceKeysToAssociate)
            {
                Key key;
                if (keys.TryGetValueExclusively(keyEntry.Key, out key))
                {
                    if (key.Instance != context.InstanceView.InstanceId)
                    {
                        throw new InstanceKeyCollisionException(
                            command.Name, context.InstanceView.InstanceId, new InstanceKey(keyEntry.Key), key.Instance);
                    }

                    // The SaveWorkflowCommand treats this as a no-op, whether completed or not.
                }
                else
                {
                    key.Completed = false;
                    key.Instance = context.InstanceView.InstanceId;
                    key.Metadata = this.SerializePropertyBag(keyEntry.Value);
                    keys.Add(keyEntry.Key, key);
                    context.AssociatedInstanceKey(keyEntry.Key);
                    if (keyEntry.Value != null)
                    {
                        foreach (var property in keyEntry.Value)
                        {
                            context.WroteInstanceKeyMetadataValue(keyEntry.Key, property.Key, property.Value);
                        }
                    }
                }
            }

            foreach (var keyGuid in command.InstanceKeysToComplete)
            {
                Key key;
                if (keys.TryGetValueExclusively(keyGuid, out key) && key.Instance == context.InstanceView.InstanceId)
                {
                    if (!key.Completed)
                    {
                        key.Completed = true;
                        keys[keyGuid] = key;
                        context.CompletedInstanceKey(keyGuid);
                    }
                }
                else
                {
                    // The SaveWorkflowCommand does not allow this.  (Should it validate against it?)
                    throw new InvalidOperationException("Attempting to complete a key which is not associated.");
                }
            }

            foreach (var keyGuid in command.InstanceKeysToFree)
            {
                Key key;
                if (keys.TryGetValueExclusively(keyGuid, out key) && key.Instance == context.InstanceView.InstanceId)
                {
                    if (!key.Completed)
                    {
                        context.CompletedInstanceKey(keyGuid);
                    }

                    keys.Remove(keyGuid);
                    context.UnassociatedInstanceKey(keyGuid);
                }
                else
                {
                    // The SaveWorkflowCommand does not allow this.  (Should it validate against it?)
                    throw new InvalidOperationException("Attempting to complete a key which is not associated.");
                }
            }

            foreach (var keyEntry in command.InstanceKeyMetadataChanges)
            {
                Key key;
                if (keys.TryGetValueExclusively(keyEntry.Key, out key) &&
                    key.Instance == context.InstanceView.InstanceId)
                {
                    if (keyEntry.Value != null)
                    {
                        foreach (var property in keyEntry.Value)
                        {
                            if (property.Value.IsDeletedValue)
                            {
                                key.Metadata.Remove(property.Key);
                            }
                            else
                            {
                                key.Metadata[property.Key] = this.SerializeInstanceValue(property.Value);
                            }

                            context.WroteInstanceKeyMetadataValue(keyEntry.Key, property.Key, property.Value);
                        }
                    }
                }
                else
                {
                    // The SaveWorkflowCommand does not allow this.  (Should it validate against it?)
                    throw new InvalidOperationException("Attempting to complete a key which is not associated.");
                }
            }

            foreach (var property in command.InstanceMetadataChanges)
            {
                if (property.Value.IsDeletedValue)
                {
                    instance.Metadata.Remove(property.Key);
                }
                else
                {
                    instance.Metadata[property.Key] = this.SerializeInstanceValue(property.Value);
                }

                context.WroteInstanceMetadataValue(property.Key, property.Value);
            }

            if (command.InstanceData.Count > 0)
            {
                instance.Data = this.SerializePropertyBag(command.InstanceData);
                instances[context.InstanceView.InstanceId] = instance;
                context.PersistedInstance(command.InstanceData);
            }

            // The command does the implicit advancement of everything into safe completed states.
            if (command.CompleteInstance)
            {
                if (instance.Data == null)
                {
                    instance.Data = new Dictionary<XName, SerializedValue>();
                    instances[context.InstanceView.InstanceId] = instance;
                    context.PersistedInstance(new Dictionary<XName, InstanceValue>());
                }

                if (keys.Count > 0)
                {
                    var keysToComplete = new Queue<Guid>();

                    foreach (var keyEntry in context.InstanceView.InstanceKeys)
                    {
                        if (keyEntry.Value.InstanceKeyState == InstanceKeyState.Associated)
                        {
                            keysToComplete.Enqueue(keyEntry.Key);
                        }
                    }

                    foreach (var keyToComplete in keysToComplete)
                    {
                        var key = keys[keyToComplete];
                        key.Completed = true;
                        keys[keyToComplete] = key;
                        context.CompletedInstanceKey(keyToComplete);
                    }
                }

                instance.Completed = true;
                instance.Owner = Guid.Empty;
                instances[context.InstanceView.InstanceId] = instance;
                context.CompletedInstance();
                context.InstanceHandle.Free();
            }
            else if (command.UnlockInstance)
            {
                instance.Owner = Guid.Empty;
                instances[context.InstanceView.InstanceId] = instance;
                context.InstanceHandle.Free();
            }

            return 0;
        }

        /// <summary>
        /// The process try load runnable command.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <exception cref="InvalidProgramException">
        /// </exception>
        private void ProcessTryLoadRunnableCommand(
            InstancePersistenceContext context, TryLoadRunnableWorkflowCommand command)
        {
            Owner owner;
            this.CheckOwner(context, command.Name, out owner);

            // Checking instance.Owner is like an InstanceLockQueryResult.
            context.QueriedInstanceStore(
                new InstanceLockQueryResult(
                    context.InstanceView.InstanceId, context.InstanceView.InstanceOwner.InstanceOwnerId));

            XName ownerService = null;
            var runnableInstance = default(Instance);
            var foundRunnableInstance = false;

            var value = this.QueryPropertyBag(WorkflowNamespace.WorkflowHostType, owner.Data);
            if (value != null && value.Value is XName)
            {
                ownerService = (XName)value.Value;
            }

            foreach (var instance in instances)
            {
                if (instance.Value.Owner != Guid.Empty || instance.Value.Completed)
                {
                    continue;
                }

                if (ownerService != null)
                {
                    value = this.QueryPropertyBag(WorkflowNamespace.WorkflowHostType, instance.Value.Metadata);
                    if (value == null || ((XName)value.Value) != ownerService)
                    {
                        continue;
                    }
                }

                value = this.QueryPropertyBag(WorkflowServiceNamespace.SuspendReason, instance.Value.Metadata);
                if (value != null && value.Value != null && value.Value is string)
                {
                    continue;
                }

                value = this.QueryPropertyBag(WorkflowNamespace.Status, instance.Value.Data);
                if (value != null && value.Value is string && ((string)value.Value) == "Executing")
                {
                    runnableInstance = instance.Value;
                    foundRunnableInstance = true;
                }

                if (!foundRunnableInstance)
                {
                    value =
                        this.QueryPropertyBag(
                            XNamespace.Get("urn:schemas-microsoft-com:System.Activities/4.0/properties").GetName(
                                "TimerExpirationTime"),
                            instance.Value.Data);
                    if (value != null && value.Value is DateTime && ((DateTime)value.Value) <= DateTime.UtcNow)
                    {
                        runnableInstance = instance.Value;
                        foundRunnableInstance = true;
                    }
                }

                if (foundRunnableInstance)
                {
                    if (runnableInstance.Data == null)
                    {
                        throw new InvalidProgramException(
                            "This is a bug, we shouldn't have a runnable, uninitilized instance");
                    }

                    runnableInstance.LockVersion++;
                    runnableInstance.Owner = context.InstanceView.InstanceOwner.InstanceOwnerId;
                    instances[instance.Key] = runnableInstance;
                    context.BindInstance(instance.Key);
                    context.BindAcquiredLock(runnableInstance.LockVersion);

                    var associatedKeys = new Dictionary<Guid, IDictionary<XName, InstanceValue>>();
                    var completedKeys = new Dictionary<Guid, IDictionary<XName, InstanceValue>>();
                    foreach (var keyEntry in keys)
                    {
                        if (keyEntry.Value.Instance == context.InstanceView.InstanceId)
                        {
                            if (keyEntry.Value.Completed)
                            {
                                completedKeys.Add(keyEntry.Key, this.DeserializePropertyBag(keyEntry.Value.Metadata));
                            }
                            else
                            {
                                associatedKeys.Add(keyEntry.Key, this.DeserializePropertyBag(keyEntry.Value.Metadata));
                            }
                        }
                    }

                    context.LoadedInstance(
                        InstanceState.Initialized,
                        this.DeserializePropertyBag(runnableInstance.Data),
                        this.DeserializePropertyBag(runnableInstance.Metadata),
                        associatedKeys,
                        completedKeys);
                    break;
                }
            }
        }

        /// <summary>
        /// The query property bag.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="serials">
        /// The serials.
        /// </param>
        /// <returns>
        /// </returns>
        private InstanceValue QueryPropertyBag(XName name, Dictionary<XName, SerializedValue> serials)
        {
            SerializedValue serial;
            if (serials == null || !serials.TryGetValue(name, out serial))
            {
                return null;
            }

            if (serial.Buffer == null)
            {
                return new InstanceValue(
                    null, serial.WriteOnly ? InstanceValueOptions.WriteOnly : InstanceValueOptions.None);
            }
            else
            {
                var value = new InstanceValue(
                    this.serialization.ReadInstance(serial.Buffer),
                    serial.WriteOnly ? InstanceValueOptions.WriteOnly : InstanceValueOptions.None);
                serial.Buffer.Seek(0, SeekOrigin.Begin);
                return value;
            }
        }

        /// <summary>
        /// The serialize instance value.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// </returns>
        private SerializedValue SerializeInstanceValue(InstanceValue value)
        {
            SerializedValue serial;
            serial.WriteOnly = (value.Options & InstanceValueOptions.WriteOnly) != 0;
            if (value.Value == null)
            {
                serial.Buffer = null;
            }
            else
            {
                serial.Buffer = new MemoryStream();
                this.serialization.WriteInstance(serial.Buffer, value.Value);
                serial.Buffer.Seek(0, SeekOrigin.Begin);
            }

            return serial;
        }

        /// <summary>
        /// The serialize property bag.
        /// </summary>
        /// <param name="bag">
        /// The bag.
        /// </param>
        /// <returns>
        /// </returns>
        private Dictionary<XName, SerializedValue> SerializePropertyBag(IEnumerable<KeyValuePair<XName, InstanceValue>> bag)
        {
            if (bag == null)
            {
                return null;
            }

            return bag.ToDictionary(property => property.Key, property => this.SerializeInstanceValue(property.Value));
        }

        #endregion

        /// <summary>
        /// The instance.
        /// </summary>
        private struct Instance : ICloneable
        {
            #region Constants and Fields

            /// <summary>
            /// The completed.
            /// </summary>
            public bool Completed;

            /// <summary>
            /// The data.
            /// </summary>
            public Dictionary<XName, SerializedValue> Data;

            /// <summary>
            /// The lock version.
            /// </summary>
            public long LockVersion;

            /// <summary>
            /// The metadata.
            /// </summary>
            public Dictionary<XName, SerializedValue> Metadata;

            /// <summary>
            /// The owner.
            /// </summary>
            public Guid Owner;

            #endregion

            #region Implemented Interfaces

            #region ICloneable

            /// <summary>
            /// The clone.
            /// </summary>
            /// <returns>
            /// The clone.
            /// </returns>
            public object Clone()
            {
                var clone = new Instance { Completed = this.Completed, Owner = this.Owner, LockVersion = this.LockVersion };
                if (this.Data != null)
                {
                    clone.Data = new Dictionary<XName, SerializedValue>(this.Data.Count);
                    foreach (var entry in this.Data)
                    {
                        clone.Data.Add(entry.Key, (SerializedValue)entry.Value.Clone());
                    }
                }

                if (this.Metadata != null)
                {
                    clone.Metadata = new Dictionary<XName, SerializedValue>(this.Metadata.Count);
                    foreach (var entry in this.Metadata)
                    {
                        clone.Metadata.Add(entry.Key, (SerializedValue)entry.Value.Clone());
                    }
                }

                return clone;
            }

            #endregion

            #endregion
        }

        /// <summary>
        /// The key.
        /// </summary>
        private struct Key : ICloneable
        {
            #region Constants and Fields

            /// <summary>
            /// The completed.
            /// </summary>
            public bool Completed;

            /// <summary>
            /// The instance.
            /// </summary>
            public Guid Instance;

            /// <summary>
            /// The metadata.
            /// </summary>
            public Dictionary<XName, SerializedValue> Metadata;

            #endregion

            #region Implemented Interfaces

            #region ICloneable

            /// <summary>
            /// The clone.
            /// </summary>
            /// <returns>
            /// The clone.
            /// </returns>
            public object Clone()
            {
                var clone = new Key { Instance = this.Instance, Completed = this.Completed };
                if (this.Metadata != null)
                {
                    clone.Metadata = new Dictionary<XName, SerializedValue>(this.Metadata.Count);
                    foreach (var entry in this.Metadata)
                    {
                        clone.Metadata.Add(entry.Key, (SerializedValue)entry.Value.Clone());
                    }
                }

                return clone;
            }

            #endregion

            #endregion
        }

        /// <summary>
        /// The owner.
        /// </summary>
        private struct Owner : ICloneable
        {
            #region Constants and Fields

            /// <summary>
            /// The data.
            /// </summary>
            public Dictionary<XName, SerializedValue> Data;

            /// <summary>
            /// The lock token.
            /// </summary>
            public Guid LockToken;

            #endregion

            #region Implemented Interfaces

            #region ICloneable

            /// <summary>
            /// The clone.
            /// </summary>
            /// <returns>
            /// The clone.
            /// </returns>
            public object Clone()
            {
                var clone = new Owner { LockToken = this.LockToken };
                if (this.Data != null)
                {
                    clone.Data = new Dictionary<XName, SerializedValue>(this.Data.Count);
                    foreach (var entry in this.Data)
                    {
                        clone.Data.Add(entry.Key, (SerializedValue)entry.Value.Clone());
                    }
                }

                return clone;
            }

            #endregion

            #endregion
        }

        /// <summary>
        /// The serialized value.
        /// </summary>
        private struct SerializedValue : ICloneable
        {
            #region Constants and Fields

            /// <summary>
            /// The buffer.
            /// </summary>
            public MemoryStream Buffer;

            /// <summary>
            /// The write only.
            /// </summary>
            public bool WriteOnly;

            #endregion

            #region Implemented Interfaces

            #region ICloneable

            /// <summary>
            /// The clone.
            /// </summary>
            /// <returns>
            /// The clone.
            /// </returns>
            public object Clone()
            {
                var clone = new SerializedValue { WriteOnly = this.WriteOnly };
                if (this.Buffer != null)
                {
                    clone.Buffer = CloneMemoryStream(this.Buffer);
                }

                return clone;
            }

            #endregion

            #endregion
        }

        /// <summary>
        /// The bind reclaimed lock async result.
        /// </summary>
        private class BindReclaimedLockAsyncResult : AsyncResult
        {
            #region Constants and Fields

            /// <summary>
            /// The on bind reclaimed.
            /// </summary>
            private static readonly AsyncCompletion OnBindReclaimed = HandleBindReclaimed;

            /// <summary>
            /// The context.
            /// </summary>
            private readonly InstancePersistenceContext context;

            /// <summary>
            /// The load.
            /// </summary>
            private readonly LoadWorkflowCommand load;

            /// <summary>
            /// The load by key.
            /// </summary>
            private readonly LoadWorkflowByInstanceKeyCommand loadByKey;

            /// <summary>
            /// The save.
            /// </summary>
            private readonly SaveWorkflowCommand save;

            /// <summary>
            /// The store.
            /// </summary>
            private readonly MemoryInstanceStore store;

            /// <summary>
            /// The transaction.
            /// </summary>
            private readonly DependentTransaction transaction;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="BindReclaimedLockAsyncResult"/> class.
            /// </summary>
            /// <param name="store">
            /// The store.
            /// </param>
            /// <param name="context">
            /// The context.
            /// </param>
            /// <param name="save">
            /// The save.
            /// </param>
            /// <param name="load">
            /// The load.
            /// </param>
            /// <param name="loadByKey">
            /// The load by key.
            /// </param>
            /// <param name="lockVersion">
            /// The lock version.
            /// </param>
            /// <param name="callback">
            /// The callback.
            /// </param>
            /// <param name="state">
            /// The state.
            /// </param>
            public BindReclaimedLockAsyncResult(
                MemoryInstanceStore store,
                InstancePersistenceContext context,
                SaveWorkflowCommand save,
                LoadWorkflowCommand load,
                LoadWorkflowByInstanceKeyCommand loadByKey,
                long lockVersion,
                AsyncCallback callback,
                object state)
                : base(callback, state)
            {
                this.store = store;
                this.context = context;
                this.save = save;
                this.load = load;
                this.loadByKey = loadByKey;

                var current = Transaction.Current;
                if (current != null)
                {
                    this.transaction = current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                }

                IAsyncResult result = null;
                try
                {
                    result = this.context.BeginBindReclaimedLock(
                        lockVersion, TimeSpan.MaxValue, this.PrepareAsyncCompletion(OnBindReclaimed), this);
                }
                finally
                {
                    if (result == null)
                    {
                        this.CompleteTransaction();
                    }
                }

                if (result.CompletedSynchronously)
                {
                    this.Complete(true);
                }
            }

            #endregion

            #region Public Methods

            /// <summary>
            /// The end.
            /// </summary>
            /// <param name="result">
            /// The result.
            /// </param>
            public static void End(IAsyncResult result)
            {
                End<BindReclaimedLockAsyncResult>(result);
            }

            #endregion

            #region Methods

            /// <summary>
            /// The handle bind reclaimed.
            /// </summary>
            /// <param name="result">
            /// The result.
            /// </param>
            /// <returns>
            /// The handle bind reclaimed.
            /// </returns>
            private static bool HandleBindReclaimed(IAsyncResult result)
            {
                return ((BindReclaimedLockAsyncResult)result.AsyncState).DoBindReclaimed(result);
            }

            /// <summary>
            /// The complete transaction.
            /// </summary>
            private void CompleteTransaction()
            {
                if (this.transaction != null)
                {
                    this.transaction.Complete();
                }
            }

            /// <summary>
            /// The do bind reclaimed.
            /// </summary>
            /// <param name="result">
            /// The result.
            /// </param>
            /// <returns>
            /// The do bind reclaimed.
            /// </returns>
            /// <exception cref="InvalidProgramException">
            /// </exception>
            private bool DoBindReclaimed(IAsyncResult result)
            {
                try
                {
                    this.context.EndBindReclaimedLock(result);

                    long reclaimedLockVersion;
                    TransactionScope scope = null;
                    try
                    {
                        if (this.transaction != null)
                        {
                            scope = new TransactionScope(this.transaction);
                        }

                        lock (LockObj)
                        {
                            if (this.save != null)
                            {
                                reclaimedLockVersion = this.store.ProcessSaveCommand(this.context, this.save);
                            }
                            else if (this.load != null)
                            {
                                reclaimedLockVersion = this.store.ProcessLoadCommand(this.context, this.load);
                            }
                            else
                            {
                                reclaimedLockVersion = this.store.ProcessLoadByKeyCommand(
                                    this.context, this.loadByKey);
                            }
                        }
                    }
                    finally
                    {
                        if (scope != null)
                        {
                            scope.Complete();
                            scope.Dispose();
                        }
                    }

                    if (reclaimedLockVersion != 0)
                    {
                        throw new InvalidProgramException(
                            "Should not be able to bind a reclaimed lock twice, since the handle will already be bound the second time.");
                    }
                }
                finally
                {
                    this.CompleteTransaction();
                }

                return true;
            }

            #endregion
        }

        /// <summary>
        /// The memory context.
        /// </summary>
        private class MemoryContext
        {
            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="MemoryContext"/> class.
            /// </summary>
            /// <param name="instanceHandle">
            /// The instance handle.
            /// </param>
            public MemoryContext(InstanceHandle instanceHandle)
            {
                this.Handle = instanceHandle;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Gets Handle.
            /// </summary>
            public InstanceHandle Handle { get; private set; }

            #endregion
        }

        /// <summary>
        /// The transaction aware dictionary.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        private class TransactionAwareDictionary<T> : IEnumerable<KeyValuePair<Guid, T>>, IPendingChangesContainer
            where T : ICloneable
        {
            #region Constants and Fields

            /// <summary>
            /// The items.
            /// </summary>
            private readonly Dictionary<Guid, T> items;

            /// <summary>
            /// The items owners.
            /// </summary>
            private readonly Dictionary<Guid, Transaction> itemsOwners;

            /// <summary>
            /// The pending transactions.
            /// </summary>
            private readonly Dictionary<Transaction, PendingChangeSet> pendingTransactions;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="TransactionAwareDictionary{T}"/> class.
            /// </summary>
            public TransactionAwareDictionary()
            {
                this.items = new Dictionary<Guid, T>();
                this.itemsOwners = new Dictionary<Guid, Transaction>();
                this.pendingTransactions = new Dictionary<Transaction, PendingChangeSet>();
            }

            #endregion

            #region Properties

            /// <summary>
            /// Gets Count.
            /// </summary>
            public int Count
            {
                get
                {
                    var count = this.items.Count;
                    if (Transaction.Current != null)
                    {
                        PendingChangeSet set;
                        if (this.pendingTransactions.TryGetValue(Transaction.Current, out set))
                        {
                            set.UpdateCount(ref count);
                        }
                    }

                    return count;
                }
            }

            /// <summary>
            /// Gets Keys.
            /// </summary>
            public IEnumerable<Guid> Keys
            {
                get
                {
                    var enumerator = this.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        yield return enumerator.Current.Key;
                    }
                }
            }

            #endregion

            #region Indexers

            /// <summary>
            /// The this.
            /// </summary>
            /// <param name="key">
            /// The key.
            /// </param>
            /// <exception cref="ArgumentException">
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// </exception>
            public T this[Guid key]
            {
                get
                {
                    T value;
                    if (!this.TryGetValue(false, key, out value))
                    {
                        throw new ArgumentException("Invalid key", "key");
                    }

                    return value;
                }

                set
                {
                    if (Transaction.Current != null)
                    {
                        PendingChangeSet set;
                        if (!this.pendingTransactions.TryGetValue(Transaction.Current, out set))
                        {
                            set = new PendingChangeSet(this);
                            this.pendingTransactions.Add(Transaction.Current, set);
                        }

                        set.Update(key, value);
                    }
                    else
                    {
                        if (this.itemsOwners[key] != null)
                        {
                            throw new InvalidOperationException(
                                "Can't update an item, which is locked under transaction!");
                        }

                        this.items[key] = value;
                    }
                }
            }

            #endregion

            #region Public Methods

            /// <summary>
            /// The add.
            /// </summary>
            /// <param name="key">
            /// The key.
            /// </param>
            /// <param name="value">
            /// The value.
            /// </param>
            public void Add(Guid key, T value)
            {
                if (Transaction.Current != null)
                {
                    PendingChangeSet set;
                    if (!this.pendingTransactions.TryGetValue(Transaction.Current, out set))
                    {
                        set = new PendingChangeSet(this);
                        this.pendingTransactions.Add(Transaction.Current, set);
                    }

                    set.Add(key, value);
                }
                else
                {
                    this.items.Add(key, value);
                    this.itemsOwners.Add(key, null);
                }
            }

            /// <summary>
            /// The remove.
            /// </summary>
            /// <param name="key">
            /// The key.
            /// </param>
            /// <exception cref="InvalidOperationException">
            /// </exception>
            public void Remove(Guid key)
            {
                if (Transaction.Current != null)
                {
                    PendingChangeSet set;
                    if (!this.pendingTransactions.TryGetValue(Transaction.Current, out set))
                    {
                        set = new PendingChangeSet(this);
                        this.pendingTransactions.Add(Transaction.Current, set);
                    }

                    set.Delete(key);
                }
                else
                {
                    if (this.itemsOwners[key] != null)
                    {
                        throw new InvalidOperationException("Can't remove an item, which is locked under transaction!");
                    }

                    this.items.Remove(key);
                    this.itemsOwners.Remove(key);
                }
            }

            /// <summary>
            /// The try get value.
            /// </summary>
            /// <param name="key">
            /// The key.
            /// </param>
            /// <param name="value">
            /// The value.
            /// </param>
            /// <returns>
            /// The try get value.
            /// </returns>
            public bool TryGetValue(Guid key, out T value)
            {
                return this.TryGetValue(false, key, out value);
            }

            /// <summary>
            /// The try get value exclusively.
            /// </summary>
            /// <param name="key">
            /// The key.
            /// </param>
            /// <param name="value">
            /// The value.
            /// </param>
            /// <returns>
            /// The try get value exclusively.
            /// </returns>
            public bool TryGetValueExclusively(Guid key, out T value)
            {
                return this.TryGetValue(true, key, out value);
            }

            #endregion

            #region Implemented Interfaces

            #region IEnumerable

            /// <summary>
            /// The get enumerator.
            /// </summary>
            /// <returns>
            /// </returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            #endregion

            #region IEnumerable<KeyValuePair<Guid,T>>

            /// <summary>
            /// The get enumerator.
            /// </summary>
            /// <returns>
            /// </returns>
            public IEnumerator<KeyValuePair<Guid, T>> GetEnumerator()
            {
                return new Enumerator(this);
            }

            #endregion

            #region IPendingchangesContainer

            /// <summary>
            /// The apply pending changes.
            /// </summary>
            /// <param name="tx">
            /// The transaction
            /// </param>
            public void ApplyPendingChanges(Transaction tx)
            {
                this.pendingTransactions[tx].Apply();
                this.pendingTransactions.Remove(tx);
            }

            /// <summary>
            /// The cancel pending changes.
            /// </summary>
            /// <param name="tx">
            /// The transaction
            /// </param>
            public void CancelPendingChanges(Transaction tx)
            {
                this.pendingTransactions[tx].Cancel();
                this.pendingTransactions.Remove(tx);
            }

            #endregion

            #endregion

            #region Methods

            /// <summary>
            /// The try get unlocked value.
            /// </summary>
            /// <param name="key">
            /// The key.
            /// </param>
            /// <param name="value">
            /// The value.
            /// </param>
            /// <returns>
            /// The try get unlocked value.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// </exception>
            private bool TryGetUnlockedValue(Guid key, out T value)
            {
                if (!this.items.TryGetValue(key, out value))
                {
                    return false;
                }

                if (this.itemsOwners[key] != null)
                {
                    throw new InvalidOperationException("Can't get an item, which is locked under transaction!");
                }

                return true;
            }

            /// <summary>
            /// The try get value.
            /// </summary>
            /// <param name="isExclusively">
            /// The is exclusively.
            /// </param>
            /// <param name="key">
            /// The key.
            /// </param>
            /// <param name="value">
            /// The value.
            /// </param>
            /// <returns>
            /// The try get value.
            /// </returns>
            private bool TryGetValue(bool isExclusively, Guid key, out T value)
            {
                if (Transaction.Current != null)
                {
                    PendingChangeSet set;
                    if (this.pendingTransactions.TryGetValue(Transaction.Current, out set))
                    {
                        bool hasPendingDelete;
                        if (set.TryGetValue(key, out value, out hasPendingDelete))
                        {
                            return !hasPendingDelete;
                        }
                    }
                    else
                    {
                        set = null;
                    }

                    if (!this.TryGetUnlockedValue(key, out value))
                    {
                        return false;
                    }

                    if (isExclusively)
                    {
                        value = (T)value.Clone();
                        if (set == null)
                        {
                            set = new PendingChangeSet(this);
                            this.pendingTransactions.Add(Transaction.Current, set);
                        }

                        set.Update(key, value);
                    }

                    return true;
                }
                else
                {
                    return this.TryGetUnlockedValue(key, out value);
                }
            }

            #endregion

            /// <summary>
            /// The enumerator.
            /// </summary>
            private class Enumerator : IEnumerator<KeyValuePair<Guid, T>>
            {
                #region Constants and Fields

                /// <summary>
                /// The owner.
                /// </summary>
                private readonly TransactionAwareDictionary<T> owner;

                /// <summary>
                /// The keys.
                /// </summary>
                private readonly List<Guid> keys;

                /// <summary>
                /// The current.
                /// </summary>
                private KeyValuePair<Guid, T> current;

                /// <summary>
                /// The index.
                /// </summary>
                private int index;

                #endregion

                #region Constructors and Destructors

                /// <summary>
                /// Initializes a new instance of the <see cref="Enumerator"/> class.
                /// </summary>
                /// <param name="owner">
                /// The owner.
                /// </param>
                public Enumerator(TransactionAwareDictionary<T> owner)
                {
                    this.owner = owner;
                    this.keys = new List<Guid>(owner.items.Keys);
                    if (Transaction.Current != null)
                    {
                        PendingChangeSet set;
                        if (this.owner.pendingTransactions.TryGetValue(Transaction.Current, out set))
                        {
                            set.UpdateKeys(this.keys);
                        }
                    }

                    this.Reset();
                }

                #endregion

                #region Properties

                /// <summary>
                /// Gets CurrentStateMachine.
                /// </summary>
                public KeyValuePair<Guid, T> Current
                {
                    get
                    {
                        return this.current;
                    }
                }

                /// <summary>
                /// Gets CurrentStateMachine.
                /// </summary>
                object IEnumerator.Current
                {
                    get
                    {
                        return this.current;
                    }
                }

                #endregion

                #region Implemented Interfaces

                #region IDisposable

                /// <summary>
                /// The dispose.
                /// </summary>
                public void Dispose()
                {
                }

                #endregion

                #region IEnumerator

                /// <summary>
                /// The move next.
                /// </summary>
                /// <returns>
                /// The move next.
                /// </returns>
                public bool MoveNext()
                {
                    if (this.index < this.keys.Count)
                    {
                        this.current = new KeyValuePair<Guid, T>(
                            this.keys[this.index], this.owner[this.keys[this.index]]);
                        this.index++;
                        return true;
                    }
                    else
                    {
                        this.current = new KeyValuePair<Guid, T>(Guid.Empty, default(T));
                        return false;
                    }
                }

                /// <summary>
                /// The reset.
                /// </summary>
                public void Reset()
                {
                    this.current = new KeyValuePair<Guid, T>(Guid.Empty, default(T));
                    this.index = 0;
                }

                #endregion

                #endregion
            }

            /// <summary>
            /// The pending change.
            /// </summary>
            private class PendingChange
            {
                #region Constants and Fields

                /// <summary>
                /// The item.
                /// </summary>
                private readonly T item;

                /// <summary>
                /// The type.
                /// </summary>
                private readonly ChangeType type;

                #endregion

                #region Constructors and Destructors

                /// <summary>
                /// Initializes a new instance of the <see cref="PendingChange"/> class.
                /// </summary>
                /// <param name="type">
                /// The type.
                /// </param>
                /// <param name="item">
                /// The item.
                /// </param>
                public PendingChange(ChangeType type, T item)
                {
                    this.type = type;
                    this.item = item;
                }

                #endregion

                #region Enums

                /// <summary>
                /// The change type.
                /// </summary>
                public enum ChangeType
                {
                    /// <summary>
                    /// The add.
                    /// </summary>
                    Add,

                    /// <summary>
                    /// The update.
                    /// </summary>
                    Update,

                    /// <summary>
                    /// The delete.
                    /// </summary>
                    Delete
                }

                #endregion

                #region Properties

                /// <summary>
                /// Gets Item.
                /// </summary>
                public T Item
                {
                    get
                    {
                        return this.item;
                    }
                }

                /// <summary>
                /// Gets Type.
                /// </summary>
                public ChangeType Type
                {
                    get
                    {
                        return this.type;
                    }
                }

                #endregion
            }

            /// <summary>
            /// The pending change set.
            /// </summary>
            private class PendingChangeSet
            {
                #region Constants and Fields

                /// <summary>
                /// The changes.
                /// </summary>
                private readonly Dictionary<Guid, PendingChange> changes;

                /// <summary>
                /// The owner.
                /// </summary>
                private readonly TransactionAwareDictionary<T> owner;

                #endregion

                #region Constructors and Destructors

                /// <summary>
                /// Initializes a new instance of the <see cref="PendingChangeSet"/> class.
                /// </summary>
                /// <param name="owner">
                /// The owner.
                /// </param>
                public PendingChangeSet(TransactionAwareDictionary<T> owner)
                {
                    Debug.Assert(
                        Transaction.Current != null, "Why create a PendingChangeSet if there is no transaction?");

                    this.owner = owner;
                    this.changes = new Dictionary<Guid, PendingChange>();

                    NotifyPendingChanges(this.owner);
                }

                #endregion

                #region Public Methods

                /// <summary>
                /// The add.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                public void Add(Guid key, T value)
                {
                    Debug.Assert(!this.owner.items.ContainsKey(key), "Trying to add an item with existing key!");
                    this.changes.Add(key, new PendingChange(PendingChange.ChangeType.Add, value));
                }

                /// <summary>
                /// The apply.
                /// </summary>
                public void Apply()
                {
                    foreach (var entry in this.changes)
                    {
                        switch (entry.Value.Type)
                        {
                            case PendingChange.ChangeType.Add:
                                this.owner.items.Add(entry.Key, entry.Value.Item);
                                this.owner.itemsOwners.Add(entry.Key, null);
                                break;
                            case PendingChange.ChangeType.Update:
                                this.owner.items[entry.Key] = entry.Value.Item;
                                this.owner.itemsOwners[entry.Key] = null;
                                break;
                            case PendingChange.ChangeType.Delete:
                                this.owner.items.Remove(entry.Key);
                                this.owner.itemsOwners.Remove(entry.Key);
                                break;
                        }
                    }
                }

                /// <summary>
                /// The cancel.
                /// </summary>
                public void Cancel()
                {
                    foreach (var entry in this.changes)
                    {
                        switch (entry.Value.Type)
                        {
                            case PendingChange.ChangeType.Add:
                                break;
                            case PendingChange.ChangeType.Update:
                            case PendingChange.ChangeType.Delete:
                                this.owner.itemsOwners[entry.Key] = null;
                                break;
                        }
                    }
                }

                /// <summary>
                /// The delete.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <exception cref="InvalidOperationException">
                /// </exception>
                /// <exception cref="InvalidOperationException">
                /// </exception>
                public void Delete(Guid key)
                {
                    PendingChange currentChange;
                    if (this.changes.TryGetValue(key, out currentChange))
                    {
                        switch (currentChange.Type)
                        {
                            case PendingChange.ChangeType.Add:
                                this.changes.Remove(key);
                                break;
                            case PendingChange.ChangeType.Update:
                                Debug.Assert(this.owner.itemsOwners[key] == Transaction.Current);
                                this.changes[key] = new PendingChange(PendingChange.ChangeType.Delete, default(T));
                                break;
                            case PendingChange.ChangeType.Delete:
                                throw new InvalidOperationException("Cannot delete an already deleted item");
                        }
                    }
                    else
                    {
                        if (this.owner.itemsOwners[key] != null)
                        {
                            throw new InvalidOperationException(
                                "Can't delete an item, which is locked under transaction!");
                        }

                        this.owner.itemsOwners[key] = Transaction.Current;
                        this.changes.Add(key, new PendingChange(PendingChange.ChangeType.Delete, default(T)));
                    }
                }

                /// <summary>
                /// The try get value.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <param name="hasPendingDelete">
                /// The has pending delete.
                /// </param>
                /// <returns>
                /// The try get value.
                /// </returns>
                public bool TryGetValue(Guid key, out T value, out bool hasPendingDelete)
                {
                    PendingChange currentChange;
                    if (this.changes.TryGetValue(key, out currentChange))
                    {
                        value = currentChange.Item;
                        hasPendingDelete = currentChange.Type == PendingChange.ChangeType.Delete;
                        return true;
                    }
                    else
                    {
                        value = default(T);
                        hasPendingDelete = false;
                        return false;
                    }
                }

                /// <summary>
                /// The update.
                /// </summary>
                /// <param name="key">
                /// The key.
                /// </param>
                /// <param name="value">
                /// The value.
                /// </param>
                /// <exception cref="InvalidOperationException">
                /// </exception>
                /// <exception cref="InvalidOperationException">
                /// </exception>
                public void Update(Guid key, T value)
                {
                    PendingChange currentChange;
                    if (this.changes.TryGetValue(key, out currentChange))
                    {
                        if (currentChange.Type == PendingChange.ChangeType.Delete)
                        {
                            throw new InvalidOperationException("Cannot update an already deleted item");
                        }

                        this.changes[key] = new PendingChange(currentChange.Type, value);
                    }
                    else
                    {
                        if (this.owner.itemsOwners[key] != null)
                        {
                            throw new InvalidOperationException(
                                "Can't update an item, which is locked under transaction!");
                        }

                        this.owner.itemsOwners[key] = Transaction.Current;
                        this.changes.Add(key, new PendingChange(PendingChange.ChangeType.Update, value));
                    }
                }

                /// <summary>
                /// The update count.
                /// </summary>
                /// <param name="count">
                /// The count.
                /// </param>
                public void UpdateCount(ref int count)
                {
                    foreach (var change in this.changes.Values)
                    {
                        switch (change.Type)
                        {
                            case PendingChange.ChangeType.Add:
                                count++;
                                break;
                            case PendingChange.ChangeType.Delete:
                                count--;
                                break;
                        }
                    }
                }

                /// <summary>
                /// The update keys.
                /// </summary>
                /// <param name="keys">
                /// The keys.
                /// </param>
                public void UpdateKeys(IList<Guid> keys)
                {
                    foreach (var entry in this.changes)
                    {
                        switch (entry.Value.Type)
                        {
                            case PendingChange.ChangeType.Add:
                                keys.Add(entry.Key);
                                break;
                            case PendingChange.ChangeType.Delete:
                                keys.Remove(entry.Key);
                                break;
                        }
                    }
                }

                #endregion
            }
        }

        /// <summary>
        /// The transaction notification manager.
        /// </summary>
        private class TransactionNotificationManager : ISinglePhaseNotification
        {
            #region Constants and Fields

            /// <summary>
            /// The change containers.
            /// </summary>
            private readonly List<IPendingChangesContainer> changeContainers;

            /// <summary>
            /// The transaction
            /// </summary>
            private readonly Transaction tx;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="TransactionNotificationManager"/> class.
            /// </summary>
            /// <param name="tx">
            /// The transaction
            /// </param>
            public TransactionNotificationManager(Transaction tx)
            {
                this.changeContainers = new List<IPendingChangesContainer>();
                this.tx = tx;
                this.tx.EnlistVolatile(this, EnlistmentOptions.None);
            }

            #endregion

            #region Public Methods

            /// <summary>
            /// The add container.
            /// </summary>
            /// <param name="container">
            /// The container.
            /// </param>
            public void AddContainer(IPendingChangesContainer container)
            {
                this.changeContainers.Add(container);
            }

            #endregion

            #region Implemented Interfaces

            #region IEnlistmentNotification

            /// <summary>
            /// The commit.
            /// </summary>
            /// <param name="enlistment">
            /// The enlistment.
            /// </param>
            void IEnlistmentNotification.Commit(Enlistment enlistment)
            {
                this.Apply();
                enlistment.Done();
            }

            /// <summary>
            /// The in doubt.
            /// </summary>
            /// <param name="enlistment">
            /// The enlistment.
            /// </param>
            void IEnlistmentNotification.InDoubt(Enlistment enlistment)
            {
                this.Cancel();
                enlistment.Done();
            }

            /// <summary>
            /// The prepare.
            /// </summary>
            /// <param name="preparingEnlistment">
            /// The preparing enlistment.
            /// </param>
            void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
            {
                preparingEnlistment.Prepared();
            }

            /// <summary>
            /// The rollback.
            /// </summary>
            /// <param name="enlistment">
            /// The enlistment.
            /// </param>
            void IEnlistmentNotification.Rollback(Enlistment enlistment)
            {
                this.Cancel();
                enlistment.Done();
            }

            #endregion

            #region ISinglePhaseNotification

            /// <summary>
            /// The single phase commit.
            /// </summary>
            /// <param name="singlePhaseEnlistment">
            /// The single phase enlistment.
            /// </param>
            void ISinglePhaseNotification.SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
            {
                this.Apply();
                singlePhaseEnlistment.Done();
            }

            #endregion

            #endregion

            #region Methods

            /// <summary>
            /// The apply.
            /// </summary>
            private void Apply()
            {
                lock (LockObj)
                {
                    foreach (var t in this.changeContainers)
                    {
                        t.ApplyPendingChanges(this.tx);
                    }

                    NotificationManagers.Remove(this.tx);
                }
            }

            /// <summary>
            /// The cancel.
            /// </summary>
            private void Cancel()
            {
                lock (LockObj)
                {
                    foreach (var t in this.changeContainers)
                    {
                        t.CancelPendingChanges(this.tx);
                    }

                    NotificationManagers.Remove(this.tx);
                }
            }

            #endregion
        }
    }
}
