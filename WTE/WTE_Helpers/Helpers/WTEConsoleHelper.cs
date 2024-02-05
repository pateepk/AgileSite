using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTE.Configuration;

namespace WTE.Helpers
{
    /// <summary>
    /// Console helper
    /// </summary>
    public class WTEConsoleHelper
    {
        #region methods

        /// <summary>
        /// Get section break
        /// </summary>
        /// <returns></returns>
        private static string GetSectionBreak()
        {
            return "============================================================";
        }


        /// <summary>
        /// Show message on the console and wait for user to press a key
        /// </summary>
        /// <param name="p_message"></param>
        public static void GetKeyPress(string p_message)
        {
            Console.WriteLine();
            Console.WriteLine(p_message);
            Console.ReadKey();
            Console.WriteLine();
        }

        /// <summary>
        /// Output message
        /// </summary>
        /// <param name="p_text"></param>
        /// <returns></returns>
        public static void OutputMessage(string p_text)
        {
            OutputMessage(p_text, false);
        }

        /// <summary>
        /// output information to the console and add to log
        /// </summary>
        /// <param name="p_text"></param>
        public static void OutputMessage(string p_text, bool p_addSection)
        {
            if (WTEConsoleConfiguration.OutputToConsole)
            {
                Console.WriteLine(p_text);
            }

            if (WTEConsoleConfiguration.OutputToFile)
            {
                WTELogging.LogMessage(p_text, WTEConsoleConfiguration.OutputFileName, false, p_addSection);
            }
        }

        #endregion methods
    }
}
