using System;
using System.Runtime.Serialization;


namespace WFT.Helpers.Scheduling
{
    [Serializable]
    internal class CrontabException : Exception
    {
        internal CrontabException() :
            base("Crontab error.") { } // TODO: Fix message and add it to resource.

        internal CrontabException(string message) :
            base(message) { }

        internal CrontabException(string message, Exception innerException) :
            base(message, innerException) { }

        protected CrontabException(SerializationInfo info, StreamingContext context) :
            base(info, context) { }
    }
}
