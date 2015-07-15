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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.Text;

namespace WFT.Core
{
    internal class SerializationProvider
    {
        #region Fields

        /// <summary>
        /// </summary>
        private readonly SerializerCache serializerCache;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationProvider"/> class.
        /// </summary>
        /// <param name="knownTypes">
        /// The known types.
        /// </param>
        public SerializationProvider(IEnumerable<Type> knownTypes)
        {
            this.serializerCache = new SerializerCache(knownTypes);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// </summary>
        /// <param name="inputStream">
        /// The input stream.
        /// </param>
        /// <returns>
        /// The read instance.
        /// </returns>
        /// <exception cref="InstancePersistenceException">
        /// </exception>
        public object ReadInstance(Stream inputStream)
        {
            var typeLength = new byte[4];
            if (inputStream.Read(typeLength, 0, typeLength.Length) != typeLength.Length)
            {
                throw new InstancePersistenceException("Bad data on stream.");
            }

            var typeName = new byte[BitConverter.ToInt32(typeLength, 0)];
            if (inputStream.Read(typeName, 0, typeName.Length) != typeName.Length)
            {
                throw new InstancePersistenceException("Bad data on stream.");
            }

            var objectType = Type.GetType(Encoding.UTF8.GetString(typeName));
            return this.LoadFromStream(inputStream, this.serializerCache.GetSerializer(objectType));
        }

        /// <summary>
        /// </summary>
        /// <param name="outputStream">
        /// The output stream.
        /// </param>
        /// <param name="instance">
        /// The instance.
        /// </param>
        public void WriteInstance(Stream outputStream, object instance)
        {
            if (instance != null)
            {
                var typeName = Encoding.UTF8.GetBytes(instance.GetType().AssemblyQualifiedName);
                var typeLength = BitConverter.GetBytes(typeName.Length);
                Debug.Assert(typeLength.Length == 4);
                outputStream.Write(typeLength, 0, typeLength.Length);
                outputStream.Write(typeName, 0, typeName.Length);
            }

            this.SaveToStream(outputStream, instance, this.serializerCache.GetSerializer(instance));
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="inputStream">
        /// The input stream.
        /// </param>
        /// <param name="serializer">
        /// The serializer.
        /// </param>
        /// <returns>
        /// The load from stream.
        /// </returns>
        protected virtual object LoadFromStream(Stream inputStream, XmlObjectSerializer serializer)
        {
            return serializer.ReadObject(inputStream);
        }

        /// <summary>
        /// </summary>
        /// <param name="outputStream">
        /// The output stream.
        /// </param>
        /// <param name="instance">
        /// The instance.
        /// </param>
        /// <param name="serializer">
        /// The serializer.
        /// </param>
        protected virtual void SaveToStream(Stream outputStream, object instance, XmlObjectSerializer serializer)
        {
            serializer.WriteObject(outputStream, instance);
        }

        #endregion

        /// <summary>
        /// </summary>
        private class SerializerCache
        {
            #region Fields

            /// <summary>
            /// </summary>
            private readonly Dictionary<RuntimeTypeHandle, XmlObjectSerializer> cache;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="SerializerCache"/> class.
            /// </summary>
            /// <param name="knownTypes">
            /// The known types.
            /// </param>
            public SerializerCache(IEnumerable<Type> knownTypes)
            {
                this.cache = new Dictionary<RuntimeTypeHandle, XmlObjectSerializer>();
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// </summary>
            /// <param name="objectToSerialize">
            /// The object to serialize.
            /// </param>
            /// <returns>
            /// </returns>
            public XmlObjectSerializer GetSerializer(object objectToSerialize)
            {
                XmlObjectSerializer result = null;
                var objectTypeHandle = Type.GetTypeHandle(objectToSerialize);
                lock (this.cache)
                {
                    if (!this.cache.TryGetValue(objectTypeHandle, out result))
                    {
                        result = CreateSerializer(objectToSerialize.GetType());
                        this.cache[objectTypeHandle] = result;
                    }
                }

                return result;
            }

            /// <summary>
            /// </summary>
            /// <param name="instanceType">
            /// The instance type.
            /// </param>
            /// <returns>
            /// </returns>
            public XmlObjectSerializer GetSerializer(Type instanceType)
            {
                XmlObjectSerializer result = null;
                if (!this.cache.TryGetValue(instanceType.TypeHandle, out result))
                {
                    result = CreateSerializer(instanceType);

                    lock (this.cache)
                    {
                        this.cache[instanceType.TypeHandle] = result;
                    }
                }

                return result;
            }

            #endregion

            #region Methods

            /// <summary>
            /// </summary>
            /// <param name="type">
            /// The type.
            /// </param>
            /// <returns>
            /// </returns>
            private static XmlObjectSerializer CreateSerializer(Type type)
            {
                return new NetDataContractSerializer();
            }

            #endregion
        }
    }
}
