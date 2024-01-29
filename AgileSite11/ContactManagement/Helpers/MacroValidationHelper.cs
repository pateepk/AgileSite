using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.EventLog;

namespace CMS.ContactManagement.Internal
{
    /// <summary>
    /// Provides helper methods for contact management macros.
    /// </summary>
    public class MacroValidationHelper : AbstractHelper<MacroValidationHelper>
    {
        /// <summary>
        /// Logs warning into event log with macro method's name in which input was invalid string representation of <see cref="Guid"/>(s).
        /// </summary>
        /// <remarks>
        /// Should be used, when macro method's input was string representation of a <see cref="Guid"/> which can not be parsed by <see cref="Guid.TryParse(string, out Guid)"/>.
        /// </remarks>
        /// <param name="macroMethodName">Name of the macro method with invalid input.</param>
        /// <param name="invalidGuidParameter">Invalid input from user.</param>
        public static void LogInvalidGuidParameter(string macroMethodName, string invalidGuidParameter)
        {
            HelperObject.LogInvalidGuidParameterInternal(macroMethodName, invalidGuidParameter);
        }


        /// <summary>
        /// Returns collection of <see cref="Guid"/>s from input string, where every string representation of a <see cref="Guid"/> is separated with semicolon.
        /// If any of the string representations of a <see cref="Guid"/> can not be parsed to <see cref="Guid"/>, then false is returned and <paramref name="guids"/> is null.
        /// </summary>
        /// <param name="stringGuids"><see cref="Guid"/>s separated with semicolon.</param>
        /// <param name="guids"><see cref="Guid"/>s parsed from <paramref name="stringGuids"/>.</param>
        public static bool TryParseGuids(string stringGuids, out ICollection<Guid> guids)
        {
            return HelperObject.TryParseGuidsInternal(stringGuids, out guids);
        }


        /// <summary>
        /// Logs warning into event log with macro method's name in which input was invalid string representation of <see cref="Guid"/>(s).
        /// </summary>
        /// <remarks>
        /// Should be used, when macro method's input was string representation of a <see cref="Guid"/> which can not be parsed by <see cref="Guid.TryParse(string, out Guid)"/>.
        /// </remarks>
        /// <param name="macroMethodName">Name of the macro method with invalid input.</param>
        /// <param name="invalidGuidParameter">Invalid input from user.</param>
        protected virtual void LogInvalidGuidParameterInternal(string macroMethodName, string invalidGuidParameter)
        {
            EventLogProvider.LogEvent(EventType.WARNING, "Macro", macroMethodName, 
                "Input '" + invalidGuidParameter + "' in macro or macro rule named " + macroMethodName + 
                " contained invalid string representation of a GUID(s). Stack trace: '" + Environment.StackTrace + "'." );
        }


        /// <summary>
        /// Returns collection of <see cref="Guid"/>s from input string, where every string representation of a <see cref="Guid"/> is separated with semicolon.
        /// If any of the string representations of a <see cref="Guid"/> can not be parsed to <see cref="Guid"/>, then false is returned and <paramref name="guids"/> is null.
        /// </summary>
        /// <param name="stringGuids"><see cref="Guid"/>s separated with semicolon.</param>
        /// <param name="guids"><see cref="Guid"/>s parsed from <paramref name="stringGuids"/>.</param>
        protected virtual bool TryParseGuidsInternal(string stringGuids, out ICollection<Guid> guids)
        {
            var splittedGuids = stringGuids.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            guids = new List<Guid>(splittedGuids.Length);

            foreach (var stringGuid in splittedGuids)
            {
                Guid guid;
                if (!Guid.TryParse(stringGuid, out guid))
                {
                    guids = null;
                    return false;
                }
                guids.Add(guid);
            }

            return true;
        }
    }
}
