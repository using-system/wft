using System.Activities;
using System.ComponentModel;

namespace WFT
{
    /// <summary>
    /// Extension methods for the type ActivityContext
    /// </summary>
    public static class ActivityContextExtensions
    {
        /// <summary>
        /// Get the value of a location reference (variable) outside of the activity of type T
        /// </summary>
        /// <typeparam name="T">Type of the variable</typeparam>
        /// <param name="context">Activity context</param>
        /// <param name="reference">Location reference</param>
        /// <returns>Return the reference value</returns>
        public static T GetReferenceValue<T>(this ActivityContext context, LocationReference reference)
        {
            T value = default(T);

            PropertyDescriptor property = context.DataContext.GetProperties()[reference.Name];
            if (property != null)
                value = (T)property.GetValue(context.DataContext);
            else
                value = context.GetValue<T>(reference);

            return value;
        }

        /// <summary>
        /// Get a argument value
        /// </summary>
        /// <typeparam name="T">Type of the argument</typeparam>
        /// <param name="context">Activity context</param>
        /// <param name="argumentName">Argument name</param>
        /// <returns>Return the argument value</returns>
        public static T GetArgumentValue<T>(this ActivityContext context, string argumentName)
        {
            T value = default(T);
            PropertyDescriptor property = context.DataContext.GetProperties()[argumentName];
            if (property != null)
                value = (T)property.GetValue(context.DataContext);

            return value;

        }

        /// <summary>
        /// Set argument value
        /// </summary>
        /// <typeparam name="T">Type of the argument</typeparam>
        /// <param name="context">Activity context</param>
        /// <param name="argumentName">Argument name</param>
        /// <param name="value">Argument value to set</param>
        public static void SetArgumentValue<T>(this ActivityContext context, string argumentName, T value)
        {
            PropertyDescriptor property = context.DataContext.GetProperties()[argumentName];
            if (property != null)
            {
                property.SetValue(context.DataContext, value);
            }
        }
    }
}
