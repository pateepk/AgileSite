using System;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper for any macro data field (source).
    /// </summary>
    public class MacroField : MacroExtension
    {
        #region "Private properties"

        /// <summary>
        /// Value evaluation function which needs context.
        /// </summary>
        private Func<EvaluationContext, object> ContextValueEvaluator
        {
            get;
            set;
        }


        /// <summary>
        /// Value evaluation function which does not need evaluation context.
        /// </summary>
        private Func<object> SimpleValueEvaluator
        {
            get;
            set;
        }


        /// <summary>
        /// Availability condition.
        /// </summary>
        private Func<EvaluationContext, bool> AvailabilityCondition
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance with value evaluator which needs a context.
        /// </summary>
        /// <param name="name">Name of the field</param>
        /// <param name="valueEvaluator">Value evaluation function which needs context</param>
        /// <param name="availabilityCondition">Availability condition</param>
        public MacroField(string name, Func<EvaluationContext, object> valueEvaluator, Func<EvaluationContext, bool> availabilityCondition = null)
        {
            if (valueEvaluator == null)
            {
                throw new ArgumentNullException("[MacroField]: Value lambda function has to be specified.");
            }

            AvailabilityCondition = availabilityCondition;
            ContextValueEvaluator = valueEvaluator;
            Name = name;
        }


        /// <summary>
        /// Creates new instance with value evaluator which does not need a context.
        /// </summary>
        /// <param name="name">Name of the field</param>
        /// <param name="valueEvaluator">Value evaluation function which does not need evaluation context</param>
        /// <param name="availabilityCondition">Availability condition</param>
        public MacroField(string name, Func<object> valueEvaluator, Func<EvaluationContext, bool> availabilityCondition = null)
        {
            if (valueEvaluator == null)
            {
                throw new ArgumentNullException("[MacroField]: Value lambda function has to be specified.");
            }

            AvailabilityCondition = availabilityCondition;
            SimpleValueEvaluator = valueEvaluator;
            Name = name;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if the field is available in the given context.
        /// </summary>
        /// <param name="context">Evaluation context in which the field is being requested</param>
        public bool IsFieldAvailable(EvaluationContext context)
        {
            if (AvailabilityCondition != null)
            {
                return AvailabilityCondition.Invoke(context);
            }
            return true;
        }


        /// <summary>
        /// Returns the value of the field in the given evaluation context.
        /// </summary>
        /// <param name="context">Evaluation context under which to evaluate the field value</param>
        public object GetValue(EvaluationContext context)
        {
            if (ContextValueEvaluator != null)
            {
                return ContextValueEvaluator.Invoke(context);
            }
            else if (SimpleValueEvaluator != null)
            {
                return SimpleValueEvaluator.Invoke();
            }
            return null;
        }

        #endregion
    }
}