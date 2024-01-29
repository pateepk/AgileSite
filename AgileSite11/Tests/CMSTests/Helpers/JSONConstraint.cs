using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;

using NUnit.Framework.Constraints;

namespace CMS.Tests
{
    /// <summary>
    /// Constraint that compares two JSON sources with each other.
    /// </summary>
    public class JSONConstraint : Constraint
    {
        private readonly object mExpectedValue;
        private readonly ICollection<string> mFailureDescription = new List<string>();


        /// <summary>
        /// JSON comparison constraint
        /// </summary>
        /// <param name="expectedValue">Expected value as string or JObject</param>
        public JSONConstraint(object expectedValue)
        {
            mExpectedValue = expectedValue;
        }


        /// <summary>
        /// The Description of what this constraint tests, for
        /// use in messages and in the ConstraintResult.
        /// </summary>
        public override string Description
        {
            get
            {
                return "is similar to JSON: " + mExpectedValue;
            }
        }


        /// <summary>
        /// Tests whether the constraint is satisfied by a given value
        /// </summary>
        /// <param name="actual">The value to be tested</param>
        /// <returns>True for success, false for failure</returns>
        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            var result = new ConstraintResultWithCustomMessage(this, actual)
            {
                Status = AreSimilar(mExpectedValue, actual) ? ConstraintStatus.Success : ConstraintStatus.Failure,
                CustomFailureMessageLines = mFailureDescription
            };

            return result;
        }


        /// <summary>
        /// Creates JSONConstraint which compares the test-Object with the given string.
        /// </summary>
        public static JSONConstraint IsSimilarTo(string expectedJSON)
        {
            return new JSONConstraint(expectedJSON);
        }


        /// <summary>
        /// Creates JSONConstraint which compares the test-Object with the given JObject.
        /// </summary>
        public static JSONConstraint IsSimilarTo(JObject expectedJObject)
        {
            return new JSONConstraint(expectedJObject);
        }


        #region "JSON Comparison Methods"

        private bool AreSimilar(object expectedObject, object actualObject)
        {
            if ((expectedObject is string) && (actualObject is string))
            {
                var expectedJObject = JObject.Parse(expectedObject.ToString());
                var actualJObject = JObject.Parse(actualObject.ToString());

                return AreSimilar(expectedJObject, actualJObject);
            }

            if ((expectedObject is JObject) && (actualObject is JObject))
            {
                return AreSimilar((JObject)expectedObject, (JObject)actualObject);
            }

            throw new ArgumentException("Actual value must be string or JObject.");
        }


        private bool AreSimilar(JObject expectedJObject, JObject actualJObject)
        {
            // Compare the values, including the values of all descendant tokens
            if (JToken.DeepEquals(expectedJObject, actualJObject))
            {
                return true;
            }

            // Compare number of properties
            var expectedProperties = expectedJObject.Children<JProperty>().Select(property => property.Name).ToList();
            var actualProperties = actualJObject.Children<JProperty>().Select(property => property.Name).ToList();
            var missingProperties = expectedProperties.Except(actualProperties).ToList();
            var exceedingProperties = actualProperties.Except(expectedProperties).ToList();

            if (missingProperties.Any())
            {
                AddFormattedFailureDescription("Object {0} has missing properties: {1}", expectedJObject.Path, String.Join(", ", missingProperties));

                return false;
            }

            if (exceedingProperties.Any())
            {
                AddFormattedFailureDescription("Object {0} has exceeding properties: {1}", expectedJObject.Path, String.Join(", ", exceedingProperties));

                return false;
            }

            // Compare properties
            foreach (var property in expectedJObject)
            {
                var expectedJToken = expectedJObject[property.Key];
                var actualJToken = actualJObject[property.Key];

                if (!AreSimilar(expectedJToken, actualJToken))
                {
                    return false;
                }
            }

            return true;
        }


        private bool AreSimilar(JToken expectedJToken, JToken actualJToken)
        {
            switch (expectedJToken.Type)
            {
                case JTokenType.Array:
                    var expectedJArray = expectedJToken.Value<JArray>();
                    var actualJArray = actualJToken.Value<JArray>();

                    if (expectedJArray.Count != actualJArray.Count)
                    {
                        AddFormattedFailureDescription("Different arrays length at {0}. ", expectedJToken.Path);
                        AddFormattedFailureDescription("Expected: {0} ", expectedJArray.Count);
                        AddFormattedFailureDescription("But was: {0}", actualJArray.Count);

                        return false;
                    }

                    if (expectedJArray.Where((t, i) => !AreSimilar(t, actualJArray[i])).Any())
                    {
                        return false;
                    }
                    break;

                case JTokenType.Object:
                    if (expectedJToken.Type != actualJToken.Type)
                    {
                        AddFormattedFailureDescription("Type mismatch at {0}. \n", expectedJToken.Path);
                        AddFormattedFailureDescription("Expected: {0} ", expectedJToken.Type);
                        AddFormattedFailureDescription("But was: {0}", actualJToken.Type);
                    }

                    return AreSimilar(expectedJToken.Value<JObject>(), actualJToken.Value<JObject>());

                default:
                    var expectedString = expectedJToken.Value<string>();
                    var actualString = actualJToken.Value<string>();

                    if (expectedString != actualString)
                    {
                        AddFormattedFailureDescription("Value of property \"{0}\" does not match. ", expectedJToken.Path);
                        AddFormattedFailureDescription("Expected: {0} ", expectedString);
                        AddFormattedFailureDescription("But was: {0}", actualString);

                        return false;
                    }
                    break;
            }

            return true;
        }
        

        private void AddFormattedFailureDescription(string format, params object[] objectArgs)
        {
            mFailureDescription.Add(String.Format(format, objectArgs));
        }

        #endregion
    }
}
