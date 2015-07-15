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


namespace WFT.Core
{
    /// <summary>
    /// Workflow Namespace
    /// </summary>
    internal static class WorkflowNamespace
    {
        // Fields
        #region Constants

        /// <summary>
        /// The base namespace
        /// </summary>
        private const string BaseNamespace = "urn:schemas-microsoft-com:System.Activities/4.0/properties";

        #endregion

        #region Static Fields

        /// <summary>
        /// </summary>
        private static readonly XNamespace outputNamespace =
            XNamespace.Get("urn:schemas-microsoft-com:System.Activities/4.0/properties/output");

        /// <summary>
        /// </summary>
        private static readonly XNamespace variablesNamespace =
            XNamespace.Get("urn:schemas-microsoft-com:System.Activities/4.0/properties/variables");

        /// <summary>
        /// </summary>
        private static readonly XNamespace workflowNamespace =
            XNamespace.Get("urn:schemas-microsoft-com:System.Activities/4.0/properties");

        /// <summary>
        /// </summary>
        private static XName bookmarks;

        /// <summary>
        /// </summary>
        private static XName exception;

        /// <summary>
        /// </summary>
        private static XName keyProvider;

        /// <summary>
        /// </summary>
        private static XName lastUpdate;

        /// <summary>
        /// </summary>
        private static XName status;

        /// <summary>
        /// </summary>
        private static XName workflow;

        /// <summary>
        /// </summary>
        private static XName workflowHostType;

        #endregion

        // Properties
        #region Public Properties

        /// <summary>
        /// Gets Bookmarks.
        /// </summary>
        public static XName Bookmarks
        {
            get
            {
                return bookmarks ?? (bookmarks = workflowNamespace.GetName("Bookmarks"));
            }
        }

        /// <summary>
        /// Gets Exception.
        /// </summary>
        public static XName Exception
        {
            get
            {
                return exception ?? (exception = workflowNamespace.GetName("Exception"));
            }
        }

        /// <summary>
        /// Gets KeyProvider.
        /// </summary>
        public static XName KeyProvider
        {
            get
            {
                return keyProvider ?? (keyProvider = workflowNamespace.GetName("KeyProvider"));
            }
        }

        /// <summary>
        /// Gets LastUpdate.
        /// </summary>
        public static XName LastUpdate
        {
            get
            {
                return lastUpdate ?? (lastUpdate = workflowNamespace.GetName("LastUpdate"));
            }
        }

        /// <summary>
        /// Gets OutputPath.
        /// </summary>
        public static XNamespace OutputPath
        {
            get
            {
                return outputNamespace;
            }
        }

        /// <summary>
        /// Gets Status.
        /// </summary>
        public static XName Status
        {
            get
            {
                return status ?? (status = workflowNamespace.GetName("Status"));
            }
        }

        /// <summary>
        /// Gets VariablesPath.
        /// </summary>
        public static XNamespace VariablesPath
        {
            get
            {
                return variablesNamespace;
            }
        }

        /// <summary>
        /// Gets Workflow.
        /// </summary>
        public static XName Workflow
        {
            get
            {
                return workflow ?? (workflow = workflowNamespace.GetName("Workflow"));
            }
        }

        /// <summary>
        /// Gets WorkflowHostType.
        /// </summary>
        public static XName WorkflowHostType
        {
            get
            {
                return workflowHostType ?? (workflowHostType = workflowNamespace.GetName("WorkflowHostType"));
            }
        }

        #endregion
    }
}
