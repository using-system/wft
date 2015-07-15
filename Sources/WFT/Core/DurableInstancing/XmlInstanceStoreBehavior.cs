using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Activities;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;


namespace WFT.Core
{
    /// <summary>
    /// Xml Instance Store Behavior
    /// </summary>
    public class XmlInstanceStoreBehavior : IServiceBehavior
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
                ((WorkflowServiceHost)serviceHostBase).DurableInstancingOptions.InstanceStore = new XmlInstanceStore();
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
    /// Xml Instance Store Wcf Extension
    /// </summary>
    public class XmlInstanceStoreExtension : BehaviorExtensionElement
    {
        #region Properties

        /// <summary>
        /// Gets BehaviorType.
        /// </summary>
        public override Type BehaviorType
        {
            get
            {
                return typeof(XmlInstanceStoreBehavior);
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
            return new XmlInstanceStoreBehavior();
        }

        #endregion
    }
}
