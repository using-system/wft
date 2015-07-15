using System;
using System.Activities;
using System.Activities.Hosting;
using System.Activities.Validation;
using System.Collections.Generic;
using System.Threading;
using WFT.Core;

namespace WFT
{
    /// <summary>
    /// Extension methods for the type Activity
    /// </summary>
    public static class ActivityExtensions
    {
        /// <summary>
        /// Invoke an activity
        /// </summary>
        /// <param name="activity">Activity to invoke</param>
        /// <param name="inputs">Activity inputs</param>
        /// <param name="validateActivity">Defines if the activity should be validated</param>
        /// <returns>Return the outputs of the activity</returns>
        public static IDictionary<string, object> Invoke(
            this Activity activity,
            IDictionary<string, object> inputs = null,
            bool validateActivity = true)
        {
            if (validateActivity)
                activity.Validate();

            WorkflowInvoker invoker = new WorkflowInvoker(activity);
            if (inputs != null)
                return invoker.Invoke(inputs);
            else
                return invoker.Invoke();
        }

        /// <summary>
        /// Begins the execution (sync) of a workflow instance.
        /// </summary>
        /// <param name="activity">Workflow instance</param>
        /// <param name="resumeBookmarkAction">Resume bookmark lambda method</param>
        /// <param name="validateActivity">Defines if the activity should be validated</param>
        public static void RunSync(
            this Activity activity,
            Action<WorkflowApplication, BookmarkInfo> resumeBookmarkAction = null,
            bool validateActivity = true)
        {
            if (validateActivity)
                activity.Validate();

            Exception workflowExc = null;
            bool workflowCompleted = false;
            Queue<BookmarkInfo> bookmarks = new Queue<BookmarkInfo>();

            WorkflowApplication wfApp = new WorkflowApplication(activity);
            wfApp.InstanceStore = new MemoryInstanceStore();
            wfApp.Completed = (E) =>
            {
                workflowCompleted = true;
                workflowExc = E.TerminationException;

            };

            wfApp.Idle = (E) =>
                {
                    foreach (BookmarkInfo bookmark in E.Bookmarks)
                        bookmarks.Enqueue(bookmark);
                };

            wfApp.Run();

            while (!workflowCompleted)
            {
                while (bookmarks.Count > 0)
                {
                    if (resumeBookmarkAction != null)
                        resumeBookmarkAction(wfApp, bookmarks.Dequeue());
                    else
                        wfApp.ResumeBookmark(bookmarks.Dequeue().BookmarkName, null);
                }
                Thread.Sleep(50);
            }


            if (workflowExc != null)
                throw workflowExc;
        }

        /// <summary>
        /// Validate an activity
        /// </summary>
        /// <param name="activity">Activity to validate</param>
        public static void Validate(this Activity activity)
        {
            ValidationResults validation = ActivityValidationServices.Validate(activity);
            if (validation.Errors.Count > 0)
                throw new WorkflowApplicationException(
                    String.Format("There is {0} errors in the workflow", validation.Errors.Count));
            if (validation.Warnings.Count > 0)
                throw new WorkflowApplicationException(
                    String.Format("There is {0} warnings in the workflow", validation.Warnings.Count));
        }
    }
}
