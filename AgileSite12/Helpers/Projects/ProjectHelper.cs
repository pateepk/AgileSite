using System;
using System.Text;

using CMS.IO;

namespace CMS.Helpers
{
    /// <summary>
    /// Class provides helper methods used during installation and import/export process.
    /// </summary>
    public class ProjectHelper
    {
        /// <summary>
        /// Changes first occurrence of string to new value in text file.
        /// </summary>
        /// <param name="inFile">Input file</param>
        /// <param name="outFile">Output file</param>
        /// <param name="oldValue">String to be replaced</param>
        /// <param name="newValue">String to replace first occurrence</param>
        public static void ReplaceMarkupAttr(string inFile, string outFile, string oldValue, string newValue)
        {
            bool changed = false;
            StringBuilder content = new StringBuilder(File.ReadAllText(inFile));
            if ((content.Length > 0) && !String.IsNullOrEmpty(oldValue))
            {
                int index = content.ToString().IndexOf(oldValue, StringComparison.InvariantCultureIgnoreCase);
                if (index >= 0)
                {
                    content.Remove(index, oldValue.Length);
                    content.Insert(index, newValue);
                    changed = true;
                }
            }
            if (changed || (inFile != outFile))
            {
                File.WriteAllText(outFile, content.ToString());
            }
        }


        /// <summary>
        /// Changes attribute "CodeBehind" to "CodeFile" in input file and saves changes to output file.
        /// </summary>
        /// <param name="inFile">Input file</param>
        /// <param name="outFile">Output file</param>
        public static void ChangeCodeBehindToCodeFile(string inFile, string outFile = null)
        {
            if (outFile == null)
            {
                outFile = inFile;
            }
            ReplaceMarkupAttr(inFile, outFile, "codebehind=\"", "CodeFile=\"");
        }


        /// <summary>
        /// Changes attribute "CodeFile" to "CodeBehind" in input file and saves changes to output file.
        /// </summary>
        /// <param name="inFile">Input file</param>
        /// <param name="outFile">Output file</param>
        public static void ChangeCodeFileToCodeBehind(string inFile, string outFile)
        {
            ReplaceMarkupAttr(inFile, outFile, "codefile=\"", "Codebehind=\"");
        }


        /// <summary>
        /// Loop through directories structure (recursively) and replace "codebehind" attribute to "codefile".
        /// </summary>
        /// <param name="path">Directory</param>
        public static void ChangeCodeBehindToCodeFileForFolder(string path)
        {
            if (File.Exists(path))
            {
                ChangeCodeBehindToCodeFile(path);
                return;
            }

            if (Directory.Exists(path))
            {
                // Retrieve all directory tree
                string[] dirs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
                int dirIndex = 0;
                int dirCount = dirs.Length;

                for (; ; )
                {
                    foreach (string s in new string[] { "*.ascx", "*.aspx", "*.master", "*.ascx.export", "*.aspx.export", "*.master.export" })
                    {
                        string[] files = Directory.GetFiles(path, s);
                        if (files.Length > 0)
                        {
                            foreach (string fName in files)
                            {
                                ChangeCodeBehindToCodeFile(fName);
                            }
                        }
                    }

                    if (dirIndex >= dirCount)
                    {
                        break;
                    }
                    path = dirs[dirIndex++];
                }
            }
        }
    }
}