using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;

namespace WFT.Activities
{
    /// <summary>
    /// Base class associated to For Activities
    /// </summary>
    public abstract class BaseForActivity : NativeActivity
    {
        public BaseForActivity()
        {
            Body = new ActivityAction<int>()
            {
                Argument = new DelegateInArgument<int>("i")
            };

            InitialVariableValue = 0;
            ConditionOperator = Operator.IsLessThan;
            ConditionValue = 10;
            VariableIncrement = 1;
        }

        /// <summary>
        /// Set the initializer value
        /// </summary>
        [RequiredArgument]
        [Description("Set the initializer value")]
        public InArgument<int> InitialVariableValue { get; set; }

        /// <summary>
        /// Set the condition operator
        /// </summary>
        [Description("Set the condition operator")]
        public Operator ConditionOperator { get; set; }

        /// <summary>
        /// Set the right operand of the condition
        /// </summary>
        [RequiredArgument]
        [Description("Set the right operand of the condition")]
        public InArgument<int> ConditionValue { get; set; }

        /// <summary>
        /// Set the iterator
        /// </summary>
        [RequiredArgument]
        [Description("Set the iterator")]
        public InArgument<int> VariableIncrement { get; set; }

        /// <summary>
        /// Body to execute
        /// </summary>
        [Browsable(false)]
        public ActivityAction<int> Body { get; set; }


        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (Body == null
                || Body.Handler == null)
                return;

            int conditionValue = ConditionValue.Get(context);
            Func<int, bool> conditionFunc = null;

            switch (ConditionOperator)
            {
                case Operator.IsGreaterThan:
                    conditionFunc = (I) => I > conditionValue;
                    break;
                case Operator.IsGreaterThanOrEqualTo:
                    conditionFunc = (I) => I >= conditionValue;
                    break;
                case Operator.IsLessThan:
                    conditionFunc = (I) => I < conditionValue;
                    break;
                case Operator.IsLessThanOrEqualTo:
                    conditionFunc = (I) => I <= conditionValue;
                    break;
                default:
                    throw new NotSupportedException(
                        String.Format("Operator {0} is not supported by this activity", ConditionOperator));
            }

            ScheduleBody(
                BuildValues(InitialVariableValue.Get(context), conditionFunc, VariableIncrement.Get(context)).GetEnumerator(), 
                context);
        }

        private IEnumerable<int> BuildValues(int initialValue, Func<int, bool> conditionFunc, int variableIncrement)
        {
            for (int i = initialValue; conditionFunc(i); i += variableIncrement)
                yield return i;
        }

        protected abstract void ScheduleBody(IEnumerator<int> values, NativeActivityContext context);

        public enum Operator
        {
            IsLessThan,
            IsLessThanOrEqualTo,
            IsGreaterThan,
            IsGreaterThanOrEqualTo
        }


    }
}
