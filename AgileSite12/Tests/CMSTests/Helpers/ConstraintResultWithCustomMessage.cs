using System.Collections.Generic;

using NUnit.Framework.Constraints;

namespace CMS.Tests
{
    /// <summary>
    /// Constraint with additional fialure message
    /// </summary>
    public class ConstraintResultWithCustomMessage : ConstraintResult
    {
        /// <summary>
        /// Constructs a <see cref="ConstraintResult"/> for a particular <see cref="Constraint"/>.
        /// </summary>
        /// <param name="constraint">The Constraint to which this result applies.</param>
        /// <param name="actualValue">The actual value to which the Constraint was applied.</param>
        public ConstraintResultWithCustomMessage(IConstraint constraint, object actualValue) : base(constraint, actualValue)
        {
            CustomFailureMessageLines = new List<string>();
        }


        /// <summary>
        /// Constructs a <see cref="ConstraintResult"/> for a particular <see cref="Constraint"/>.
        /// </summary>
        /// <param name="constraint">The Constraint to which this result applies.</param>
        /// <param name="actualValue">The actual value to which the Constraint was applied.</param>
        /// <param name="status">The status of the new ConstraintResult.</param>
        public ConstraintResultWithCustomMessage(IConstraint constraint, object actualValue, ConstraintStatus status) : base(constraint, actualValue, status)
        {
            CustomFailureMessageLines = new List<string>();
        }


        /// <summary>
        /// Constructs a <see cref="ConstraintResult"/> for a particular <see cref="Constraint"/>.
        /// </summary>
        /// <param name="constraint">The Constraint to which this result applies.</param>
        /// <param name="actualValue">The actual value to which the Constraint was applied.</param>
        /// <param name="isSuccess">If true, applies a status of Success to the result, otherwise Failure.</param>
        public ConstraintResultWithCustomMessage(IConstraint constraint, object actualValue, bool isSuccess) : base(constraint, actualValue, isSuccess)
        {
            CustomFailureMessageLines = new List<string>();
        }


        /// <summary>
        /// Additional failure message lines
        /// </summary>
        public IEnumerable<string> CustomFailureMessageLines
        {
            get;
            set;
        }


        /// <summary>
        /// Write the failure message to the MessageWriter provided as an argument.
        /// </summary>
        /// <param name="writer">The MessageWriter on which to display the message</param>
        public override void WriteMessageTo(MessageWriter writer)
        {
            foreach (var line in CustomFailureMessageLines)
            {
                writer.WriteMessageLine(line);
            }
            
            writer.DisplayDifferences(this);
        }
    }
}
