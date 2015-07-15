using System;
using System.Diagnostics;

namespace WFT.Helpers.Scheduling
{
    /// <summary>
    /// Represents the method that will handle an <see cref="Exception"/> object.
    /// </summary>
    internal delegate void ExceptionHandler(Exception e);

    /// <summary>
    /// Represents the method that will generate an <see cref="Exception"/> object.
    /// </summary>
    internal delegate Exception ExceptionProvider();

    /// <summary>
    /// Defines error handling strategies.
    /// </summary>
    internal static class ErrorHandling
    {
        /// <summary>
        /// A stock <see cref="ExceptionHandler"/> that throws.
        /// </summary>
        internal static readonly ExceptionHandler Throw = e => { throw e; };

        internal static ExceptionProvider OnError(ExceptionProvider provider, ExceptionHandler handler)
        {
            Debug.Assert(provider != null);

            if (handler != null)
                handler(provider());

            return provider;
        }
    }
}
