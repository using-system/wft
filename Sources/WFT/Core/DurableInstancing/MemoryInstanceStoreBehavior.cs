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

using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace WFT.Core
{
    using System;
    using System.ServiceModel.Configuration;
    using WorkflowServiceHost = System.ServiceModel.Activities.WorkflowServiceHost;

    /// <summary>
    /// A service behavior that allows you to add the MemoryStore to a WorkflowServiceHost
    /// </summary>
    public class MemoryInstanceStoreBehavior : IServiceBehavior
    {
        #region Implemented Interfaces

        #region IServiceBehavior

        /// <summary>
        /// Provides the ability to pass custom data to binding elements to support the contract implementation.
        /// </summary>
        /// <param name="serviceDescription">
        /// The service description.
        /// </param>
        /// <param name="serviceHostBase">
        /// The service host base.
        /// </param>
        /// <param name="endpoints">
        /// The endpoints.
        /// </param>
        /// <param name="bindingParameters">
        /// The binding parameters.
        /// </param>
        public virtual void AddBindingParameters(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Provides the ability to change run-time property values or insert custom extension objects such as error handlers, message or parameter interceptors, security extensions, and other custom extension objects. 
        /// </summary>
        /// <param name="serviceDescription">
        /// The service description.
        /// </param>
        /// <param name="serviceHostBase">
        /// The service host base.
        /// </param>
        public virtual void ApplyDispatchBehavior(
            ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceHostBase is WorkflowServiceHost)
            {
                ((WorkflowServiceHost)serviceHostBase).DurableInstancingOptions.InstanceStore = new MemoryInstanceStore();
            }
        }

        /// <summary>
        /// Provides the ability to inspect the service host and the service description to confirm that the service can run successfully. 
        /// </summary>
        /// <param name="serviceDescription">
        /// The service description.
        /// </param>
        /// <param name="serviceHostBase">
        /// The service host base.
        /// </param>
        public virtual void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Memory Instance Store Wcf Extension
    /// </summary>
    public class MemoryInstanceStoreExtension : BehaviorExtensionElement
    {
        #region Properties

        /// <summary>
        /// Gets BehaviorType.
        /// </summary>
        public override Type BehaviorType
        {
            get
            {
                return typeof(MemoryInstanceStoreBehavior);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The create behavior.
        /// </summary>
        /// <returns>
        /// The create behavior.
        /// </returns>
        protected override object CreateBehavior()
        {
            return new MemoryInstanceStoreBehavior();
        }

        #endregion
    }
}
