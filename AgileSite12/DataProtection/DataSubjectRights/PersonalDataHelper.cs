using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.DataProtection
{
    /// <summary>
    /// Contains helper methods for personal data processing.
    /// </summary>
    public class PersonalDataHelper : AbstractHelper<PersonalDataHelper>
    {
        private const string XML_INDENTATION = "  ";
        private readonly string NEW_LINE_WITH_INDENTATION = Environment.NewLine + XML_INDENTATION;


        /// <summary>
        /// Joins individual results (<see cref="PersonalDataCollectorResult.Text"/>) of personal data collection.
        /// </summary>
        /// <param name="personalData">Enumeration of individual personal data (<see cref="PersonalDataCollectorResult.Text"/>) obtained from collectors.</param>
        /// <param name="outputFormat">Output format to determine proper join method.</param>
        /// <returns>Returns joined <paramref name="personalData"/> with respect to <paramref name="outputFormat"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="personalData"/> or <paramref name="outputFormat"/> is null.</exception>
        /// <exception cref="NotSupportedException">Thrown when given <paramref name="outputFormat"/> is not supported.</exception>
        /// <seealso cref="PersonalDataCollectorRegister.CollectData"/>
        /// <seealso cref="PersonalDataFormat"/>
        /// <remarks>
        /// The default system implementation supports <see cref="PersonalDataFormat.HUMAN_READABLE"/> and <see cref="PersonalDataFormat.MACHINE_READABLE"/> output format.
        /// </remarks>
        public static string JoinPersonalData(IEnumerable<string> personalData, string outputFormat)
        {
            return HelperObject.JoinPersonalDataInternal(personalData, outputFormat);
        }


        /// <summary>
        /// Joins individual results (<see cref="PersonalDataCollectorResult.Text"/>) of personal data collection.
        /// </summary>
        /// <param name="personalData">Enumeration of individual personal data (<see cref="PersonalDataCollectorResult.Text"/>) obtained from collectors.</param>
        /// <param name="outputFormat">Output format to determine proper join method.</param>
        /// <returns>Returns joined <paramref name="personalData"/> with respect to <paramref name="outputFormat"/>, or empty string for empty <paramref name="personalData"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="personalData"/> or <paramref name="outputFormat"/> is null.</exception>
        /// <exception cref="NotSupportedException">Thrown when given <paramref name="outputFormat"/> is not supported.</exception>
        /// <seealso cref="PersonalDataCollectorRegister.CollectData"/>
        /// <seealso cref="PersonalDataFormat"/>
        /// <remarks>
        /// The default system implementation supports <see cref="PersonalDataFormat.HUMAN_READABLE"/> and <see cref="PersonalDataFormat.MACHINE_READABLE"/> output format.
        /// </remarks>
        protected virtual string JoinPersonalDataInternal(IEnumerable<string> personalData, string outputFormat)
        {
            if (personalData == null)
            {
                throw new ArgumentNullException(nameof(personalData));
            }
            if(outputFormat == null)
            {
                throw new ArgumentNullException(nameof(outputFormat));
            }

            if (outputFormat.Equals(PersonalDataFormat.MACHINE_READABLE, StringComparison.OrdinalIgnoreCase))
            {
                var modifiedData = personalData
                        .Select(s => s.Replace(Environment.NewLine, NEW_LINE_WITH_INDENTATION));
                string joinedData = String.Join(NEW_LINE_WITH_INDENTATION, modifiedData);

                if (String.IsNullOrEmpty(joinedData))
                {
                    return String.Empty;
                }

                var resultBuilder = new StringBuilder();

                resultBuilder.AppendLine("<PersonalData>");
                resultBuilder.Append(XML_INDENTATION);
                resultBuilder.AppendLine(joinedData);
                resultBuilder.Append("</PersonalData>");

                return resultBuilder.ToString();
            }
            else if(outputFormat.Equals(PersonalDataFormat.HUMAN_READABLE, StringComparison.OrdinalIgnoreCase))
            {
                return String.Join(Environment.NewLine, personalData);
            }

            throw new NotSupportedException($"Given output format '{outputFormat}' is not supported.");
        }
    }
}
