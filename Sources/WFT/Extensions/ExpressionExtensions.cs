using System;
using System.Activities;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace WFT
{
    /// <summary>
    /// Extension methods for the type ParamaeterExpression and Expression (activity context) 
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        ///  Get the expression value of a location reference (variable) outside of the activity with the activity context expression
        /// </summary>
        /// <param name="context">Activity context expression</param>
        /// <param name="type">Type of the location reference</param>
        /// <param name="reference">Location reference</param>
        /// <param name="isOutArgument">Define the direction of the reference</param>
        /// <returns>Return the expression value</returns>
        public static Expression GetReferenceValue(this ParameterExpression context, Type type, LocationReference reference, bool isOutArgument = false)
        {
            return (Expression)typeof(ExpressionExtensions).GetMethod(
                "GetReferenceValue",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[3] { typeof(ParameterExpression), typeof(LocationReference), typeof(bool) },
                null).MakeGenericMethod(type).Invoke(null, new object[3] { context, reference, isOutArgument });
        }

        /// <summary>
        /// Get the expression value of a location reference (variable) of type T outside of the activity with the activity context expression
        /// </summary>
        /// <typeparam name="T">Type of the location reference</typeparam>
        /// <param name="context">Activity context expression</param>
        /// <param name="reference">Location reference</param>
        /// <param name="isOutArgument">Define the direction of the reference</param>
        /// <returns>Return the expression value</returns>
        public static Expression GetReferenceValue<T>(this ParameterExpression context, LocationReference reference, bool isOutArgument = false)
        {
            if (isOutArgument)
            {
                MethodInfo getValueMethod = typeof(ActivityContext).GetMethods()
                  .Where((M) => M.IsGenericMethod && M.Name == "GetValue" && M.GetParameters()[0].ParameterType == typeof(LocationReference))
                  .Single();
                getValueMethod = getValueMethod.MakeGenericMethod(typeof(T));
                return
                    Expression.Call(context, getValueMethod, Expression.Constant(reference));
            }
            else
                return Expression.Call(typeof(ActivityContextExtensions).GetMethod("GetReferenceValue").MakeGenericMethod(typeof(T)),
                    context, Expression.Constant(reference));
        }

        /// <summary>
        /// Get the value expression of an expression with result of type T
        /// </summary>
        /// <typeparam name="T">Result value expression type</typeparam>
        /// <param name="context">Activity context expression</param>
        /// <param name="expression">Expression</param>
        /// <returns>Return the expression value</returns>
        public static Expression GetActivityExpressionValue<T>(this Expression context, Expression<Func<ActivityContext, T>> expression)
        {
            Func<ActivityContext, T> callValue = expression.Compile();

            return Expression.Convert(Expression.Call(
                Expression.Constant(callValue),
                typeof(Func<ActivityContext, T>).GetMethod("DynamicInvoke"),
                Expression.NewArrayInit(typeof(object), context)), typeof(T));
        }
    }
}
