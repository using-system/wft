using System;


namespace WFT.Core
{
    /// <summary>
    /// Completed Async Result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CompletedAsyncResult<T> : AsyncResult
    {
        T data;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompletedAsyncResult{T}"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        public CompletedAsyncResult(T data, AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.data = data;
            Complete(true);
        }

        /// <summary>
        /// Ends the specified result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public static T End(IAsyncResult result)
        {
            CompletedAsyncResult<T> completedResult = AsyncResult.End<CompletedAsyncResult<T>>(result);
            return completedResult.data;
        }
    }

    /// <summary>
    ///   An AsyncResult that completes as soon as it is instantiated.
    /// </summary>
    public class CompletedAsyncResult : AsyncResult
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompletedAsyncResult"/> class.
        /// </summary>
        /// <param name="callback">
        /// The callback. 
        /// </param>
        /// <param name="state">
        /// The state. 
        /// </param>
        public CompletedAsyncResult(AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.Complete(true);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The end.
        /// </summary>
        /// <param name="result">
        /// The result. 
        /// </param>
        public static void End(IAsyncResult result)
        {
            End<CompletedAsyncResult>(result);
        }

        #endregion
    }
}
