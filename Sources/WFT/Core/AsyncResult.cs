using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;


namespace WFT.Core
{
    /// <summary>
    /// Async Result
    /// </summary>
    public abstract class AsyncResult : IAsyncResult
    {
        static AsyncCallback asyncCompletionWrapperCallback;
        AsyncCallback callback;
        bool completedSynchronously;
        bool endCalled;
        Exception exception;
        bool isCompleted;
        ManualResetEvent manualResetEvent;
        AsyncCompletion nextAsyncCompletion;
        object state;
        object thisLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncResult"/> class.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        protected AsyncResult(AsyncCallback callback, object state)
        {
            this.callback = callback;
            this.state = state;
            this.thisLock = new object();
        }

        /// <summary>
        /// Gets a user-defined object that qualifies or contains information about an asynchronous operation.
        /// </summary>
        /// <returns>A user-defined object that qualifies or contains information about an asynchronous operation.</returns>
        public object AsyncState
        {
            get
            {
                return state;
            }
        }

        /// <summary>
        /// Gets a <see cref="T:System.Threading.WaitHandle" /> that is used to wait for an asynchronous operation to complete.
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.WaitHandle" /> that is used to wait for an asynchronous operation to complete.</returns>
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (manualResetEvent != null)
                {
                    return manualResetEvent;
                }

                lock (ThisLock)
                {
                    if (manualResetEvent == null)
                    {
                        manualResetEvent = new ManualResetEvent(isCompleted);
                    }
                }

                return manualResetEvent;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation completed synchronously.
        /// </summary>
        /// <returns>true if the asynchronous operation completed synchronously; otherwise, false.</returns>
        public bool CompletedSynchronously
        {
            get
            {
                return completedSynchronously;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has callback.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has callback; otherwise, <c>false</c>.
        /// </value>
        public bool HasCallback
        {
            get
            {
                return this.callback != null;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation has completed.
        /// </summary>
        /// <returns>true if the operation is complete; otherwise, false.</returns>
        public bool IsCompleted
        {
            get
            {
                return isCompleted;
            }
        }

        object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        /// <summary>
        /// Completes the specified completed synchronously.
        /// </summary>
        /// <param name="completedSynchronously">if set to <c>true</c> [completed synchronously].</param>
        /// <exception cref="System.InvalidProgramException">
        /// Async callback threw an Exception
        /// </exception>
        protected void Complete(bool completedSynchronously)
        {
            if (isCompleted)
            {
                // It's a bug to call Complete twice.
                throw new InvalidProgramException();
            }

            this.completedSynchronously = completedSynchronously;

            if (completedSynchronously)
            {
                // If we completedSynchronously, then there's no chance that the manualResetEvent was created so
                // we don't need to worry about a race
                this.isCompleted = true;
            }
            else
            {
                lock (ThisLock)
                {
                    this.isCompleted = true;
                    if (this.manualResetEvent != null)
                    {
                        this.manualResetEvent.Set();
                    }
                }
            }

            if (callback != null)
            {
                try
                {
                    callback(this);
                }
                catch (Exception e)
                {
                    throw new InvalidProgramException("Async callback threw an Exception", e);
                }
            }
        }

        /// <summary>
        /// Completes the specified completed synchronously.
        /// </summary>
        /// <param name="completedSynchronously">if set to <c>true</c> [completed synchronously].</param>
        /// <param name="exception">The exception.</param>
        protected void Complete(bool completedSynchronously, Exception exception)
        {
            this.exception = exception;
            Complete(completedSynchronously);
        }

        static void AsyncCompletionWrapperCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            AsyncResult thisPtr = (AsyncResult)result.AsyncState;
            AsyncCompletion callback = thisPtr.GetNextCompletion();

            bool completeSelf = false;
            Exception completionException = null;
            try
            {
                completeSelf = callback(result);
            }
            catch (Exception e)
            {
                if (IsFatal(e))
                {
                    throw;
                }
                completeSelf = true;
                completionException = e;
            }

            if (completeSelf)
            {
                thisPtr.Complete(false, completionException);
            }
        }

        /// <summary>
        /// Determines whether the specified exception is fatal.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        ///   <c>true</c> if the specified exception is fatal; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsFatal(Exception exception)
        {
            while (exception != null)
            {
                if ((exception is OutOfMemoryException && !(exception is InsufficientMemoryException)) ||
                    exception is ThreadAbortException ||
                    exception is AccessViolationException ||
                    exception is SEHException)
                {
                    return true;
                }

                // These exceptions aren't themselves fatal, but since the CLR uses them to wrap other exceptions,
                // we want to check to see whether they've been used to wrap a fatal exception.  If so, then they
                // count as fatal.
                if (exception is TypeInitializationException ||
                    exception is TargetInvocationException)
                {
                    exception = exception.InnerException;
                }
                else
                {
                    break;
                }
            }

            return false;
        }

        /// <summary>
        /// Prepares the async completion.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <returns></returns>
        protected AsyncCallback PrepareAsyncCompletion(AsyncCompletion callback)
        {
            this.nextAsyncCompletion = callback;
            if (AsyncResult.asyncCompletionWrapperCallback == null)
            {
                AsyncResult.asyncCompletionWrapperCallback = new AsyncCallback(AsyncCompletionWrapperCallback);
            }
            return AsyncResult.asyncCompletionWrapperCallback;
        }

        AsyncCompletion GetNextCompletion()
        {
            AsyncCompletion result = this.nextAsyncCompletion;
            this.nextAsyncCompletion = null;
            return result;
        }

        /// <summary>
        /// Ends the specified result.
        /// </summary>
        /// <typeparam name="TAsyncResult">The type of the async result.</typeparam>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">result</exception>
        /// <exception cref="System.ArgumentException">Invalid AsyncResult;result</exception>
        /// <exception cref="System.InvalidOperationException">AsyncResult already ended</exception>
        protected static TAsyncResult End<TAsyncResult>(IAsyncResult result)
            where TAsyncResult : AsyncResult
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            TAsyncResult asyncResult = result as TAsyncResult;

            if (asyncResult == null)
            {
                throw new ArgumentException("Invalid AsyncResult", "result");
            }

            if (asyncResult.endCalled)
            {
                throw new InvalidOperationException("AsyncResult already ended");
            }

            asyncResult.endCalled = true;

            if (!asyncResult.isCompleted)
            {
                asyncResult.AsyncWaitHandle.WaitOne();
            }

            if (asyncResult.manualResetEvent != null)
            {
                asyncResult.manualResetEvent.Close();
            }

            if (asyncResult.exception != null)
            {
                throw asyncResult.exception;
            }

            return asyncResult;
        }

        // can be utilized by subclasses to write core completion code for both the sync and async paths
        // in one location, signaling chainable synchronous completion with the boolean result,
        // and leveraging PrepareAsyncCompletion for conversion to an AsyncCallback.
        // NOTE: requires that "this" is passed in as the state object to the asynchronous sub-call being used with a completion routine.
        protected delegate bool AsyncCompletion(IAsyncResult result);
    }
}
