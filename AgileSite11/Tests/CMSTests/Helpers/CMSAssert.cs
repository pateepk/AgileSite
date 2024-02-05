using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

using CMS.DataEngine;
using CMS.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;

using Assert = NUnit.Framework.Assert;
using CollectionAssert = NUnit.Framework.CollectionAssert;

namespace CMS.Tests
{
    /// <summary>
    /// Custom assertions
    /// </summary>
    public class CMSAssert
    {
        #region "Serialization methods"

        private static Stream Serialize<TFormatter>(object source)
            where TFormatter : IFormatter, new()
        {
            var formatter = new TFormatter();
            var stream = new MemoryStream();

            formatter.Serialize(stream, source);

            return stream;
        }

        private static T Deserialize<TFormatter, T>(Stream stream)
            where TFormatter : IFormatter, new()
        {
            var formatter = new TFormatter();
            stream.Position = 0;

            return (T)formatter.Deserialize(stream);
        }

        private static T CloneBySerialization<TFormatter, T>(T source)
            where TFormatter : IFormatter, new()
        {
            var serialized = Serialize<TFormatter>(source);

            return Deserialize<TFormatter, T>(serialized);
        }

        #endregion


        #region "Assert methods"

        /// <summary>
        /// Asserts that the serialization / de-serialization of the object is possible, and produces 
        /// </summary>
        /// <param name="obj">Object to test</param>
        public static T Serialization<T>(T obj)
        {
            return Serialization<BinaryFormatter, T>(obj);
        }


        /// <summary>
        /// Asserts that the serialization / de-serialization of the object is possible. Returns the de-serialized clone
        /// </summary>
        /// <param name="obj">Object to test</param>
        private static T Serialization<TFormatter, T>(T obj)
            where TFormatter : IFormatter, new()
        {
            T clone = default(T);

            Assert.DoesNotThrow(() =>
                {
                    clone = CloneBySerialization<TFormatter, T>(obj);
                }, 
                "Unable to serialize / de-serialize the given object."
            );

            Assert.IsFalse(ReferenceEquals(obj, clone), "Failed round-trip serialization, clone points to source");

            return clone;
        }


        /// <summary>
        /// Asserts the given expression for each item in the given collection
        /// </summary>
        /// <param name="items">Items to assert</param>
        /// <param name="action">Assert action for the item</param>
        public static void ForEach<T>(IEnumerable<T> items, Action<T> action)
        {
            List<Action> actions = new List<Action>();

            foreach (var item in items)
            {
                T itemClosure = item;

                actions.Add(() => action(itemClosure));
            }

            All(actions.ToArray());
        }


        /// <summary>
        /// Execute multiple assertions
        /// </summary>
        /// <param name="assertionsToRun">Assertions to be executed</param>
        public static void All(params Action[] assertionsToRun)
        {
            var errorMessages = new List<Exception>();

            // Execute all assertions
            foreach (var action in assertionsToRun)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    errorMessages.Add(ex);
                }
            }

            ReportErrors(assertionsToRun.Length, errorMessages);
        }


        /// <summary>
        /// Executes assertion and adds additional message if assertion fails
        /// </summary>
        /// <param name="message">Extra message in case assertion fails</param>
        /// <param name="assertionToRun">Assertion to be executed</param>
        public static void WithMessage(string message, Action assertionToRun)
        {
            try
            {
                assertionToRun();
            }
            catch (Exception ex)
            {
                if (message == null)
                {
                    throw;
                }
                else
                {
                    if (ex is AssertFailedException)
                    {
                        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail(message + Environment.NewLine + GetMessage(ex));
                    }
                    else if (ex is AssertionException)
                    {
                        Assert.Fail(message + Environment.NewLine + GetMessage(ex));
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }


        /// <summary>
        /// Gets the full exception message from all inner exceptions
        /// </summary>
        /// <param name="ex">Exception</param>
        private static string GetMessage(Exception ex)
        {
            var message = new StringBuilder();

            while (ex != null)
            {
                message.AppendLine(ex.Message);
                ex = ex.InnerException;
            }

            return message.ToString();
        }


        private static void ReportErrors(int totalCount, List<Exception> errorMessages)
        {
            // Throw single error message for all failed assertions
            if (errorMessages.Any())
            {
                var separator = String.Format("{0}{0}", Environment.NewLine);
                var errorMessage = String.Join(separator, errorMessages);
                var fullErrorMessage = String.Format("AssertAll: {0}/{1} assertions failed.{2}{3}", errorMessages.Count, totalCount, separator, errorMessage);

                if (errorMessages.OfType<AssertFailedException>().Any())
                {
                    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail(fullErrorMessage);
                }
                else if (errorMessages.OfType<AssertionException>().Any())
                {
                    Assert.Fail(fullErrorMessage);
                }
                else
                {
                    throw new Exception(fullErrorMessage);
                }
            }
        }


        /// <summary>
        /// Runs multiple actions in the given number of threads and reports all failed assertions and errors 
        /// </summary>
        /// <param name="count">Threads count</param>
        /// <param name="action">Thread action</param>
        public static IEnumerable<Thread> InThreads(int count, Action<int> action)
        {
            var threads = new List<Thread>(count);
            var exceptions = new List<Exception>();

            // Create threads that uses cached section to cache data
            for (int i = 0; i < count; i++)
            {
                int threadIndex = i;

                threads.Add(new Thread(() =>
                {
                    try
                    {
                        action(threadIndex);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }));
            }

            // Run threads and wait for them to finish
            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());

            // Report exceptions through global assert
            if (exceptions.Count > 0)
            {
                ReportErrors(count, exceptions);
            }

            return threads;
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Asserts two objects by comparing their property values
        /// </summary>
        /// <param name="actual">Current object</param>
        /// <param name="expected">Expected object</param>
        /// <param name="condition">Property condition</param>
        public static void PropertyValuesEqual(object actual, object expected, Func<PropertyInfo, bool> condition = null)
        {
            var properties = expected.GetType().GetProperties();

            foreach (var property in properties)
            {
                // Skip indexers
                if (property.GetIndexParameters().Length == 0)
                {
                    // Match the condition
                    if ((condition == null) || condition(property))
                    {
                        // Compare the property values
                        object expectedValue = property.GetValue(expected, null);
                        object actualValue = property.GetValue(actual, null);

                        var list = actualValue as IList;
                        if (list != null)
                        {
                            // Special handling for lists
                            CollectionAssert.AreEqual((IList)expectedValue, list, "Collection property {0}.{1} does not match.");
                        }
                        else if (!Equals(expectedValue, actualValue))
                        {
                            Assert.Fail("Property {0}.{1} does not match. Expected: {2} but was: {3}", property.DeclaringType.Name, property.Name, expectedValue, actualValue);
                        }
                    }
                }
            }
        }
        

        /// <summary>
        /// Asserts whether two queries equal
        /// </summary>
        /// <param name="current">Current query</param>
        /// <param name="expected">Expected query</param>
        /// <param name="message">Optional error message</param>
        public static void QueryEquals(WhereCondition current, string expected, string message = null)
        {
            var where = (current != null) ? current.ToString(true) : "";

            QueryEquals(@where, expected, message);
        }


        /// <summary>
        /// Asserts whether two queries equal
        /// </summary>
        /// <param name="current">Current query</param>
        /// <param name="expected">Expected query</param>
        /// <param name="message">Optional error message</param>
        public static void QueryEquals(string current, string expected, string message = null)
        {
            if ((current == null) || (expected == null))
            {
                Assert.Fail("One of the queries has null value.");
            }

            var currentDifference = current;
            var expectedDifference = expected;

            var equal = SqlHelper.QueriesEqual(ref currentDifference, ref expectedDifference, true);

            Assert.IsTrue(equal, String.Format(
@"
{4}

--Difference expected:
{2}

--Difference actual:
{3}

----------------------------

-- Complete expected:
{0}

-- Complete actual:
{1}
",
                expected.Trim(),
                current.Trim(),
                expectedDifference,
                currentDifference,
                message ?? "Queries don't match"
            ));
        }


        /// <summary>
        /// Asserts whether two queries equal
        /// </summary>
        /// <param name="current">Current query</param>
        /// <param name="expected">Expected query</param>
        /// <param name="message">Optional error message</param>
        /// <param name="settings">Text normalization settings</param>
        public static void TextEquals(string current, string expected, string message = null, TextNormalizationSettings settings = null)
        {
            var currentDifference = current;
            var expectedDifference = expected;

            var equal = TextHelper.ContentEquals(ref currentDifference, ref expectedDifference, true, settings);

            Assert.IsTrue(equal, String.Format(
@"
{4}

Difference expected:
{2}

Difference actual:
{3}

----------------------------

-- Complete expected:
{0}

-- Complete actual:
{1}
",
                expected.Trim(),
                current.Trim(),
                expectedDifference,
                currentDifference,
                message ?? "Texts don't match"
            ));
        }

        #endregion
    }
}
