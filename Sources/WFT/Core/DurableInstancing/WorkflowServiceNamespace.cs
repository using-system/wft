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

using System.Xml.Linq;
using System.Diagnostics;

namespace WFT.Core
{
    internal static class WorkflowServiceNamespace
    {
        // Fields
        private const string baseNamespace = "urn:schemas-microsoft-com:System.ServiceModel.Activities/4.0/properties";
        private static XName controlEndpoint;
        private static XName creationContext;

        private static readonly XNamespace endpointsNamespace =
            XNamespace.Get("urn:schemas-microsoft-com:System.ServiceModel.Activities/4.0/properties/endpoints");

        private static XName relativeApplicationPath;
        private static XName relativeServicePath;
        private static XName service;
        private static XName siteName;
        private static XName suspendException;
        private static XName suspendReason;

        private static readonly XNamespace workflowServiceNamespace =
            XNamespace.Get("urn:schemas-microsoft-com:System.ServiceModel.Activities/4.0/properties");

        // Properties
        public static XName ControlEndpoint
        {
            get
            {
                Debug.Assert(workflowServiceNamespace != null, "workflowServiceNamespace != null");
                return controlEndpoint ?? (controlEndpoint = workflowServiceNamespace.GetName("ControlEndpoint"));
            }
        }

        public static XName CreationContext
        {
            get
            {
                if (creationContext == null)
                {
                    creationContext = workflowServiceNamespace.GetName("CreationContext");
                }
                return creationContext;
            }
        }

        public static XNamespace EndpointsPath
        {
            get { return endpointsNamespace; }
        }

        public static XName RelativeApplicationPath
        {
            get
            {
                if (relativeApplicationPath == null)
                {
                    relativeApplicationPath = workflowServiceNamespace.GetName("RelativeApplicationPath");
                }
                return relativeApplicationPath;
            }
        }

        public static XName RelativeServicePath
        {
            get
            {
                if (relativeServicePath == null)
                {
                    relativeServicePath = workflowServiceNamespace.GetName("RelativeServicePath");
                }
                return relativeServicePath;
            }
        }

        public static XName Service
        {
            get
            {
                if (service == null)
                {
                    service = workflowServiceNamespace.GetName("Service");
                }
                return service;
            }
        }

        public static XName SiteName
        {
            get
            {
                if (siteName == null)
                {
                    siteName = workflowServiceNamespace.GetName("SiteName");
                }
                return siteName;
            }
        }

        public static XName SuspendException
        {
            get
            {
                if (suspendException == null)
                {
                    suspendException = workflowServiceNamespace.GetName("SuspendException");
                }
                return suspendException;
            }
        }

        public static XName SuspendReason
        {
            get
            {
                if (suspendReason == null)
                {
                    suspendReason = workflowServiceNamespace.GetName("SuspendReason");
                }
                return suspendReason;
            }
        }
    }
}
