using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Methods for the enhanced controls management.
    /// </summary>
    public static class ControlsHelper
    {
        #region "Variables"

        /// <summary>
        /// Dynamic control search regular expression.
        /// </summary>
        private static Regex mRegExControl;


        /// <summary>
        /// Regular expression for strings.
        /// </summary>
        private static Regex mRegExStrings;


        /// <summary>
        /// Allowed forum dynamic controls.
        /// </summary>
        public static string ALLOWED_FORUM_CONTROLS = "image;media;youtubevideo";

        #endregion


        #region "Properties"

        /// <summary>
        /// Dynamic control search regular expression.
        /// </summary>
        public static Regex RegExControl
        {
            get
            {
                // Expression groups:                                             (type             )(macro                  )          (index                    )(type      )(macro                                                         )
                return mRegExControl ?? (mRegExControl = RegexHelper.GetRegex("(?:(?<type>%%control:)(?<macro>(?:[^%]|%[^%])+)%%)|(?:\\{(?<index>(?:\\([0-9]+\\))?)(?<type>\\^)(?<macro>(?:(?:(?!\\k<type>).)|(?:\\k<type>(?!\\k<index>\\})))*)(?:\\k<type>)(?:\\k<index>)(?:\\}))"));
            }
            set
            {
                mRegExControl = value;
            }
        }


        /// <summary>
        /// Regular expression for strings.
        /// </summary>
        public static Regex RegExStrings
        {
            get
            {
                // Expression groups:                                              (1:name   )
                return mRegExStrings ?? (mRegExStrings = RegexHelper.GetRegex(@"\{%(?<1>[^%]*)\%}"));
            }
            set
            {
                mRegExStrings = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Converts the given object to a Data source valid for data bound control
        /// </summary>
        /// <param name="source">Data source</param>
        public static object GetDataSourceForControl(object source)
        {
            var result = source;

            var ds = source as DataSet;
            if (ds != null)
            {
                // DataSet
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    result = ds.Tables[0].DefaultView;
                }
            }
            else
            {
                DataTable table = source as DataTable;
                if (table != null)
                {
                    // Data table
                    result = table.DefaultView;
                }
                else if ((source == null) || (source is IEnumerable) || (source is IListSource))
                {
                    // Enumerable or list source
                }
                else
                {
                    // Single object - Convert to enumerable
                    result = new[] { source };
                }
            }

            return result;
        }


        /// <summary>
        /// Ensures the Script manager on the page.
        /// </summary>
        /// <param name="page">Page</param>
        public static ScriptManager EnsureScriptManager(Page page)
        {
            // If the page is content page, ensure the script manager
            var cmsPage = page as ICMSPage;

            return cmsPage?.EnsureScriptManager();
        }


        /// <summary>
        /// Returns the scrollbars enum for the given code.
        /// </summary>
        /// <param name="code">Code</param>
        public static ScrollBars GetScrollbarsEnum(string code)
        {
            if (String.IsNullOrEmpty(code))
            {
                return ScrollBars.Auto;
            }

            switch (code.ToLowerInvariant())
            {
                case "vertical":
                    return ScrollBars.Vertical;

                case "horizontal":
                    return ScrollBars.Horizontal;

                case "both":
                    return ScrollBars.Both;

                case "none":
                    return ScrollBars.None;

                default:
                    return ScrollBars.Auto;
            }
        }


        /// <summary>
        /// Returns controls collection for specified type
        /// </summary>
        /// <typeparam name="T">Control type</typeparam>
        /// <param name="parent">Parent control</param>
        public static IEnumerable<T> GetControlsOfType<T>(Control parent) where T : Control
        {
            var t = parent as T;
            if (t != null)
            {
                yield return t;
            }

            if (parent != null)
            {
                foreach (Control c in parent.Controls)
                {
                    foreach (var i in GetControlsOfType<T>(c))
                    {
                        yield return i;
                    }
                }
            }
        }


        /// <summary>
        /// Gets the control of specified type recursively to parent control
        /// </summary>
        /// <typeparam name="T">Required control type</typeparam>
        /// <param name="parent">Parent control</param>
        public static T GetControlOfTypeRecursive<T>(Control parent) where T : Control
        {
            if (parent != null)
            {
                foreach (Control ctrl in parent.Controls)
                {
                    // Compare current item
                    T result = ctrl as T;
                    if (result != null)
                    {
                        return result;
                    }

                    // Try find in child controls
                    result = GetControlOfTypeRecursive<T>(ctrl);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Replaces macros in text with appropriate value of the datarow and returns the result.
        /// </summary>
        /// <param name="sourceText">Source text to be processed</param>
        /// <param name="dr">DataRow containing data that will be used for replacements</param>
        /// <param name="encode">Indicates whether macro should be encoded</param>
        /// <remarks>It also escapes apostrophes ("'") to "\'" so that they do not break JavaScript.</remarks>
        public static string ResolveMacros(string sourceText, DataRow dr, bool encode = false)
        {
            int i;

            if (dr == null)
            {
                return "";
            }
            if (sourceText.IndexOf("{%", StringComparison.Ordinal) < 0)
            {
                return sourceText;
            }

            string[,] replacements = GetReplacedStrings(sourceText, dr);
            if (replacements == null)
            {
                return sourceText;
            }

            string result = sourceText;
            for (i = replacements.GetLowerBound(0); i <= replacements.GetUpperBound(0); i++)
            {
                result = result.Replace("{%" + replacements[i, 0] + "%}", HTMLHelper.HTMLEncode(replacements[i, 1].Replace("'", @"\'")));
                i++;
            }

            return result;
        }


        /// <summary>
        /// Translates the controls ArrayList to the Hashtable indexed by the controls ID [Control.ID.ToLowerCSafe()] -> [Control]
        /// </summary>
        /// <param name="controls">Controls to translate</param>
        /// <param name="idFunc">Function that transforms the control to its ID</param>
        public static SafeDictionary<string, TControl> GetControlsHashtable<TControl>(List<TControl> controls, Func<TControl, string> idFunc) where TControl : Control
        {
            // Build the HashTable
            var result = new SafeDictionary<string, TControl>();
            if (controls != null)
            {
                foreach (TControl c in controls)
                {
                    string id = idFunc(c);
                    result[id.ToLowerInvariant()] = c;
                }
            }
            return result;
        }


        /// <summary>
        /// Translates the controls ArrayList to the Hashtable indexed by the controls ID [Control.ID.ToLowerCSafe()] -> [Control]
        /// </summary>
        /// <param name="controls">Controls to translate</param>
        public static SafeDictionary<string, TControl> GetControlsHashtable<TControl>(List<TControl> controls) where TControl : Control
        {
            // Build the HashTable
            var result = new SafeDictionary<string, TControl>();
            if (controls != null)
            {
                foreach (TControl c in controls)
                {
                    result[c.ID.ToLowerInvariant()] = c;
                }
            }
            return result;
        }


        /// <summary>
        /// Gets property value from parent control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="propFunc">Function that gets property value</param>
        /// <param name="defaultValue">Default property value if no suitable parent control found</param>
        public static PropertyType GetParentProperty<ParentControlType, PropertyType>(Control control, Func<ParentControlType, PropertyType> propFunc, PropertyType defaultValue) where ParentControlType : Control
        {
            var ctrl = control.Parent as ParentControlType;
            if (ctrl != null)
            {
                return propFunc(ctrl);
            }

            // Skip to next parent of given class
            Control parent = control.Parent;
            while (parent != null)
            {
                parent = parent.Parent;
                ctrl = parent as ParentControlType;
                if (ctrl != null)
                {
                    return propFunc(ctrl);
                }
            }

            return defaultValue;
        }


        /// <summary>
        /// Resolves the dynamic control macros within the parent controls collection and loads the dynamic controls instead.
        /// </summary>
        /// <param name="parent">Parent control of the control tree to resolve</param>
        /// <param name="allowedControls">List of the allowed controls separated by semicolon</param>
        public static bool ResolveDynamicControls(Control parent, string allowedControls = null)
        {
            if (parent != null)
            {
                bool hierarchyChanged = false;

                // Go through all the controls
                if (parent.Controls.Count > 0)
                {
                    // New controls collection, Null == not changed, Arraylist == changed
                    var ctrls = new List<ArrayList>();

                    bool changed = false;
                    int controlIndex = 1;

                    // Loop thru all items in collection
                    foreach (Control child in parent.Controls)
                    {
                        // If literal control, get the content to resolve
                        string content = null;

                        if (child is LiteralControl)
                        {
                            content = ((LiteralControl)child).Text;
                        }
                        else if (child is DataBoundLiteralControl)
                        {
                            content = ((DataBoundLiteralControl)child).Text;
                        }
                        else if (child is Literal)
                        {
                            content = ((Literal)child).Text;
                        }
                        else
                        {
                            // Resolve the controls of the child control and add to the resulting controls collection
                            if (ResolveDynamicControls(child))
                            {
                                hierarchyChanged = true;
                            }
                            ctrls.Add(null);
                        }

                        // Resolve the text content if found
                        if (content != null)
                        {
                            var resolved = ResolveDynamicControls(content, parent, allowedControls, ref changed, ref controlIndex);

                            ctrls.Add(resolved);
                        }
                    }

                    // Rebuild the controls collection
                    if (changed)
                    {
                        int offset = 0;
                        // Loop thru all parent controls
                        for (int i = 0; i < ctrls.Count; i++)
                        {
                            // Convert to list of sub controls
                            ArrayList al = ctrls[i];

                            // Check whether controls hierarchy changed
                            if (al != null)
                            {
                                // Get new position with previous offset
                                int position = i + offset;

                                // Remove old simple control
                                parent.Controls.RemoveAt(position);

                                // Add ne controls to the original control position
                                for (int j = 0; j < al.Count; j++)
                                {
                                    var ctrl = al[j] as Control;
                                    if (ctrl != null)
                                    {
                                        parent.Controls.AddAt(position + j, ctrl);
                                    }
                                }

                                // Increase offset for next run
                                offset += al.Count - 1;
                            }
                        }

                        hierarchyChanged = true;
                    }
                }

                return hierarchyChanged;
            }

            return false;
        }


        /// <summary>
        /// Returns true if the content contains dynamic controls.
        /// </summary>
        /// <param name="content">Content to check</param>
        public static bool ContainsDynamicControl(string content)
        {
            if (String.IsNullOrEmpty(content))
            {
                return false;
            }

            // Check old macros
            if (content.Contains("%%"))
            {
                return true;
            }
            // Check new macro
            else if (content.Contains("{^"))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Resolves the dynamic control macros within the parent controls collection and loads the dynamic controls instead.
        /// </summary>
        /// <param name="content">Content to resolve</param>
        /// <param name="parent">Parent control</param>
        /// <param name="allowedControls">List of the allowed controls separated by semicolon</param>
        /// <param name="changed">Returns true if the collection has been changed (some controls were resolved)</param>
        /// <param name="controlIndex">Returns current control index which is used for dynamic control IDs</param>
        private static ArrayList ResolveDynamicControls(string content, Control parent, string allowedControls, ref bool changed, ref int controlIndex)
        {
            // Quick check for dynamic controls
            if (ContainsDynamicControl(content))
            {
                // Match the control expressions
                var matches = RegExControl.Matches(content);
                if (matches.Count > 0)
                {
                    LiteralControl newLiteral = null;
                    ArrayList result = new ArrayList();

                    int lastIndex = 0;

                    // Some controls found, resolve the collection
                    foreach (Match m in matches)
                    {
                        string type = m.Groups["type"].ToString();
                        string expression = m.Groups["macro"].ToString();

                        // If some additional text found, add it as a literal control
                        if (lastIndex < m.Index)
                        {
                            newLiteral = new LiteralControl(content.Substring(lastIndex, m.Index - lastIndex));
                            newLiteral.EnableViewState = false;

                            result.Add(newLiteral);
                        }

                        Hashtable parameters = null;

                        string controlName = expression;
                        string controlParameter = null;

                        // Old macro %%control:name?parameter%%
                        if (type.StartsWith("%%", StringComparison.Ordinal))
                        {
                            // Check the parameter presence
                            int queryIndex = expression.IndexOf("?", StringComparison.Ordinal);
                            if (queryIndex >= 0)
                            {
                                controlParameter = expression.Substring(queryIndex + 1);
                                controlName = expression.Substring(0, queryIndex);
                            }
                        }
                        // New macro {^name|(param1)value^}
                        else
                        {
                            // Parse inline parameters
                            parameters = ParseInlineParameters(expression, ref controlParameter, ref controlName);
                        }

                        changed = true;

                        // Check if allowed
                        bool allow = false;
                        if (String.IsNullOrEmpty(allowedControls))
                        {
                            allow = true;
                        }
                        else
                        {
                            // Check if the control is allowed
                            string allowed = ";" + allowedControls.ToLowerInvariant() + ";";
                            if (allowed.Contains(controlName.ToLowerInvariant()))
                            {
                                allow = true;
                            }
                        }

                        if (allow)
                        {
                            string path = GetInlineControlPath(controlName);

                            if (path != null)
                            {
                                // Load the control
                                try
                                {
                                    InlineUserControl newControl = (InlineUserControl)(parent.Page.LoadUserControl(path));
                                    newControl.ID = controlName + controlIndex;

                                    controlIndex++;

                                    // Single parameter
                                    if (controlParameter != null)
                                    {
                                        newControl.Parameter = controlParameter;
                                    }

                                    // Dynamic parameters
                                    if (parameters != null)
                                    {
                                        foreach (DictionaryEntry param in parameters)
                                        {
                                            // Set the parameter
                                            string parameterName = (string)param.Key;
                                            string parameterValue = (string)param.Value;

                                            newControl.SetValue(parameterName, parameterValue);
                                        }
                                    }

                                    // Add literal if not found before
                                    if (newLiteral == null)
                                    {
                                        newLiteral = new LiteralControl(String.Empty);
                                        newLiteral.EnableViewState = false;

                                        result.Add(newLiteral);
                                    }

                                    result.Add(newControl);

                                    // Notify on the loaded content
                                    newControl.OnContentLoaded();
                                }
                                catch (Exception ex)
                                {
                                    // In case of an error replace with the error message label
                                    var lblError = new Label
                                    {
                                        CssClass = "InlineControlError",
                                        EnableViewState = false,
                                        Text = "Error loading the control '" + controlName + "': " + ex.Message,
                                        ToolTip = EventLogProvider.GetExceptionLogMessage(ex)
                                    };

                                    result.Add(lblError);

                                    // Log the exception to event log
                                    EventLogProvider.LogException("InlineControls", "LoadControl", ex);
                                }
                            }
                        }
                        else
                        {
                            // Dynamic control does not exists - Render just the original macro
                            newLiteral = new LiteralControl(m.ToString());
                            newLiteral.EnableViewState = false;

                            result.Add(newLiteral);
                        }

                        // Set the end of processed area behind the control
                        lastIndex = m.Index + m.Length;
                    }

                    // Add the rest of the text
                    if (lastIndex < content.Length)
                    {
                        newLiteral = new LiteralControl(content.Substring(lastIndex));
                        newLiteral.EnableViewState = false;

                        result.Add(newLiteral);
                    }

                    return result;
                }
            }

            return null;
        }


        /// <summary>
        /// Parses inline macro values, creates collection of this parameters, sets control name and control parameter if is available.
        /// </summary>
        /// <param name="expression">Inline macro without starting {^ and trailing ^}</param>
        /// <param name="controlParameter">Control parameter</param>
        /// <param name="controlName">Control name</param>
        /// <returns>Returns collection of parameters</returns>
        public static Hashtable ParseInlineParameters(string expression, ref string controlParameter, ref string controlName)
        {
            if (!String.IsNullOrEmpty(expression))
            {
                Hashtable parameters = null;

                // Replace escaped slash due to parsing of parameters
                expression = expression.Replace("\\|", "##ESCSLASH##");

                // Parse the parameters
                int paramIndex = expression.LastIndexOf("|", StringComparison.Ordinal);
                while (paramIndex >= 0)
                {
                    // Get the parameter
                    string parameter = expression.Substring(paramIndex + 1).Replace("##ESCSLASH##", "\\|");

                    int nameStart = parameter.IndexOf("(", StringComparison.Ordinal) + 1;
                    int nameEnd = parameter.IndexOf(")", StringComparison.Ordinal);

                    if ((nameStart > 0) && (nameEnd >= 0))
                    {
                        // Create parameters array
                        if (parameters == null)
                        {
                            parameters = new Hashtable();
                        }

                        // Get the parameter parts
                        string parameterName = parameter.Substring(nameStart, nameEnd - nameStart);
                        string parameterValue = parameter.Substring(nameEnd + 1);

                        parameters[parameterName] = MacroProcessor.UnescapeParameterValue(parameterValue);
                    }
                    else
                    {
                        // Unnamed parameter - default one
                        controlParameter = MacroProcessor.UnescapeParameterValue(parameter);
                    }

                    // Get next parameter
                    expression = expression.Substring(0, paramIndex);
                    paramIndex = expression.LastIndexOf("|", StringComparison.Ordinal);
                }

                controlName = expression;

                return parameters;
            }

            return null;
        }


        /// <summary>
        /// Removes all dynamic control macros from string.
        /// </summary>
        /// <param name="content">Content to remove macros</param>
        /// <returns>Content with removed macros</returns>
        public static string RemoveDynamicControls(string content)
        {
            // Quick check for dynamic controls
            if (ContainsDynamicControl(content))
            {
                // Match the control expressions
                return RegExControl.Replace(content, String.Empty);
            }

            return content;
        }


        /// <summary>
        /// Gets the reversed column sizes.
        /// </summary>
        /// <param name="columns">Columns to reverse</param>
        public static string GetReversedColumns(string columns)
        {
            // Change the size order
            List<string> sizes = columns.Split(',').ToList();
            sizes.Reverse();
            return String.Join(",", sizes.ToArray());
        }


        /// <summary>
        /// Reverses the frames within given frameset control.
        /// </summary>
        /// <param name="frameset">Frameset control</param>
        public static void ReverseFrames(HtmlGenericControl frameset)
        {
            // Switch frames
            ArrayList controls = new ArrayList();
            foreach (Control c in frameset.Controls)
            {
                controls.Add(c);
            }
            frameset.Controls.Clear();
            foreach (Control c in controls)
            {
                frameset.Controls.AddAt(0, c);
            }

            // Change the size order
            string newsizes = GetReversedColumns(frameset.Attributes["cols"]);

            frameset.Attributes.Remove("cols");
            frameset.Attributes.Add("cols", newsizes);
        }


        /// <summary>
        /// Moves the child controls between two controls.
        /// </summary>
        /// <param name="sourceControl">Source control</param>
        /// <param name="targetControl">Target control</param>
        /// <param name="excludeControl">Exclude control</param>
        public static void MoveControls(Control sourceControl, Control targetControl, Control excludeControl)
        {
            ArrayList controls = new ArrayList();

            // Create list of controls to move
            foreach (Control c in sourceControl.Controls)
            {
                if (c != excludeControl)
                {
                    controls.Add(c);
                }
            }

            // Move the controls to other control
            foreach (Control c in controls)
            {
                sourceControl.Controls.Remove(c);
                targetControl.Controls.Add(c);
            }
        }


        /// <summary>
        /// Determines whether control contains another one.
        /// </summary>
        /// <param name="parentControl">Control to look within</param>
        /// <param name="nestedControl">Control to look for</param>
        public static bool ContainsControl(Control parentControl, Control nestedControl)
        {
            if (parentControl == nestedControl)
            {
                return true;
            }
            else if (parentControl.Controls.Count > 0)
            {
                return parentControl.Controls.Cast<Control>().Any(childControl => ContainsControl(childControl, nestedControl));
            }
            return false;
        }


        /// <summary>
        /// Raises postback on the given control
        /// </summary>
        /// <param name="ctrl">Control to raise postback on</param>
        /// <param name="argument">Argument</param>
        public static void RaisePostback(Control ctrl, string argument = null)
        {
            ScriptHelper.RegisterStartupScript(ctrl.Page, typeof(string), "Postback_" + ctrl.ClientID, GetPostBackEventReference(ctrl, argument), true);
        }


        /// <summary>
        /// Gets the postback event reference to the given control.
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="argument">Argument</param>
        /// <param name="registerForEventValidation">Indicates if the event reference should be registred for validation</param>
        /// <param name="registerAsyncPostbackInUpdatePanel">If true registers async postback in update panel, otherwise registers full postback</param>
        public static string GetPostBackEventReference(Control control, string argument = null, bool registerForEventValidation = false, bool registerAsyncPostbackInUpdatePanel = true)
        {
            if (control == null)
            {
                return null;
            }

            // Get the page
            Page page = control.Page;
            if (page == null)
            {
                return null;
            }

            return GetPostBackEventReference(control, new PostBackOptions(control, argument), registerForEventValidation, registerAsyncPostbackInUpdatePanel);
        }


        /// <summary>
        /// Gets the postback event reference to the given control.
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="options">Options</param>
        /// <param name="registerForEventValidation">Indicates if the event reference should be registered for validation</param>
        /// <param name="registerAsyncPostbackInUpdatePanel">If true registers async postback in update panel, otherwise registers full postback</param>
        /// <param name="locationControl">Optional control used as reference for postback source. Postback target can be different than postback source.</param>
        public static string GetPostBackEventReference(Control control, PostBackOptions options, bool registerForEventValidation, bool registerAsyncPostbackInUpdatePanel, Control locationControl = null)
        {
            Page page = control?.Page;
            if (page == null)
            {
                return null;
            }

            // Register for async postbacks
            if (IsInUpdatePanel(locationControl ?? control))
            {
                if (registerAsyncPostbackInUpdatePanel)
                {
                    ScriptManager.GetCurrent(page)?.RegisterAsyncPostBackControl(control);
                }
                else
                {
                    RegisterPostbackControl(control, locationControl);
                }
            }

            // Get the reference
            return page.ClientScript.GetPostBackEventReference(options);
        }


        /// <summary>
        /// Gets the ID of the control that caused page postback.
        /// </summary>
        /// <param name="page">Page</param>
        public static string GetPostBackControlID(Page page)
        {
            if ((page == null) || !RequestHelper.IsPostBack())
            {
                return null;
            }

            // Try to get standard way
            string ctrlname = page.Request.Params[Page.postEventSourceID];
            if (String.IsNullOrEmpty(ctrlname))
            {
                // Get from the script manager hidden field (async postback)
                ScriptManager manager = ScriptManager.GetCurrent(page);
                if (manager != null)
                {
                    string managerId = manager.UniqueID;
                    string valueField = page.Request.Form[managerId];

                    if (!String.IsNullOrEmpty(valueField) && valueField.Contains("|"))
                    {
                        ctrlname = valueField.Split('|')[1];
                    }
                }
            }

            return ctrlname;
        }


        /// <summary>
        /// Gets the control which caused postback.
        /// </summary>
        /// <param name="page">Page</param>
        public static Control GetPostBackControl(Page page)
        {
            Control control = null;

            // Find by event target
            string ctrlname = page.Request.Params[Page.postEventSourceID];
            if (!String.IsNullOrEmpty(ctrlname))
            {
                control = page.FindControl(ctrlname);
            }
            else
            {
                // Find by submit
                foreach (string ctl in page.Request.Form)
                {
                    Control c = page.FindControl(ctl);
                    if (c is Button)
                    {
                        control = c;
                        break;
                    }
                }
            }

            return control;
        }


        /// <summary>
        /// Determines whether any child controls of given control caused the postback.
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <param name="recursive">Indicates whether the whole control tree should be searched or just first level of children.</param>
        private static bool ChildCausedPostBackInternal(Control parent, bool recursive = true)
        {
            if (parent != null)
            {
                HttpRequest request = parent.Page.Request;
                string target = request.Params[Page.postEventSourceID];

                // Buttons tend to be at the end of forms so loop the collection backwards.
                foreach (Control ctrl in parent.Controls.OfType<Control>().Reverse())
                {
                    // Control unique ID is event target or in request form - caused PostBack.
                    if ((ctrl is IButtonControl) && ((ctrl.UniqueID == target) || !String.IsNullOrEmpty(request.Form[ctrl.UniqueID])))
                    {
                        return true;
                    }

                    // Recursively search child controls.
                    if (recursive && ChildCausedPostBackInternal(ctrl))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Determines whether any of given controls caused the postback.
        /// </summary>
        /// <param name="controls">Controls to examine</param>
        /// <returns>TRUE if any of given controls caused the postback</returns>
        public static bool CausedPostBack(params Control[] controls)
        {
            return CausedPostBack(false, controls);
        }


        /// <summary>
        /// Determines whether any of given controls caused the postback.
        /// </summary>
        /// <param name="checkChildren">Whether to check children within control hierarchy</param>
        /// <param name="controls">Controls to examine</param>
        /// <returns>TRUE if any of given controls caused the postback</returns>
        public static bool CausedPostBack(bool checkChildren, params Control[] controls)
        {
            var postbackId = GetPostBackControlID(controls[0].Page);
            if (postbackId == null)
            {
                return false;
            }

            // If the checked control is not naming container, the check must proceed on individual control base
            if (checkChildren && controls.Any(c => !(c is INamingContainer)))
            {
                return controls.Any(c => CausedPostBack(c, postbackId, false) || ChildCausedPostBackInternal(c));
            }

            return controls.Any(control => CausedPostBack(control, postbackId, checkChildren));
        }


        /// <summary>
        /// Determines whether given control (or its child) caused the postback.
        /// </summary>
        /// <param name="postbackId">Identifier of control that caused postback</param>
        /// <param name="checkChildren">Whether to check children within control hierarchy</param>
        /// <param name="control">Control to examine</param>
        /// <returns>TRUE if given control caused the postback</returns>
        private static bool CausedPostBack(Control control, string postbackId, bool checkChildren)
        {
            // If the ID matches, the control caused postback
            if (control.UniqueID == postbackId)
            {
                return true;
            }

            if (checkChildren && (control.Controls.Count > 0))
            {
                return control.Controls.Cast<Control>().Any(childControl => CausedPostBack(childControl, postbackId, true));
            }

            return false;
        }


        /// <summary>
        /// Updates current update panel.
        /// </summary>
        /// <param name="control">Control to update</param>
        public static void UpdateCurrentPanel(Control control)
        {
            while (control != null)
            {
                var panel = control as UpdatePanel;
                if (panel != null)
                {
                    // Update the panel if conditional
                    if (panel.UpdateMode == UpdatePanelUpdateMode.Conditional)
                    {
                        panel.Update();
                    }

                    break;
                }

                control = control.Parent;
            }
        }


        /// <summary>
        /// Returns true if the control is located inside of the update panel.
        /// </summary>
        /// <param name="control">Control</param>
        public static bool IsInUpdatePanel(Control control)
        {
            return (GetUpdatePanel(control) != null);
        }


        /// <summary>
        /// Returns true if the page is in the process of asynchronous postback.
        /// </summary>
        /// <param name="page">Page</param>
        public static bool IsInAsyncPostback(Page page)
        {
            ScriptManager manager = ScriptManager.GetCurrent(page);
            if (manager != null)
            {
                return manager.IsInAsyncPostBack;
            }

            return false;
        }


        /// <summary>
        /// Searches for the DropDownList item with given value.
        /// </summary>
        /// <param name="control">Control to search</param>
        /// <param name="value">Value to search</param>
        /// <param name="caseSensitive">Case sensitive search</param>
        public static ListItem FindItemByValue(DropDownList control, string value, bool caseSensitive)
        {
            // Set the comparison type
            StringComparison comparison = StringComparison.InvariantCulture;
            if (!caseSensitive)
            {
                comparison = StringComparison.InvariantCultureIgnoreCase;
            }

            // Go through all the items
            foreach (ListItem item in control.Items)
            {
                if (item.Value.Equals(value, comparison))
                {
                    return item;
                }
            }

            return null;
        }


        /// <summary>
        ///  Searches the container for a server control with the specified id parameter.
        /// </summary>
        /// <param name="controlId">The identifier for the control to be found</param>
        /// <param name="control">Starting control</param>
        public static Control FindParentControl(string controlId, Control control)
        {
            while ((control != null) && (control.Parent != null))
            {
                control = control.Parent;
                Control ctrl = control.FindControl(controlId);
                if (ctrl != null)
                {
                    return ctrl;
                }
            }
            return null;
        }


        /// <summary>
        /// Gets the parent control of specified type for given control.
        /// </summary>
        /// <param name="control">Control</param>
        public static ParentType GetParentControl<ParentType>(Control control)
        {
            return (ParentType)(object)GetParentControl(control, typeof(ParentType));
        }


        /// <summary>
        /// Gets the parent control of specified type for given control.
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="type">Control type</param>
        public static Control GetParentControl(Control control, Type type)
        {
            // Get the first parent control
            if (control != null)
            {
                control = control.Parent;
            }

            while (control != null)
            {
                if (type.IsInstanceOfType(control))
                {
                    return control;
                }

                control = control.Parent;
            }

            return null;
        }


        /// <summary>
        /// Gets the first child control of specified type for given control.
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ensureChildControls">Indicates whether child controls should be ensured when looking for a given control</param>
        public static ChildType GetChildControl<ChildType>(Control control, bool ensureChildControls = true) where ChildType : Control
        {
            return (ChildType)GetChildControl(control, typeof(ChildType), ensureChildControls);
        }


        /// <summary>
        /// Gets the first child control of specified type for given control.
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="type">Control type</param>
        /// <param name="ensureChildControls">Indicates whether child controls should be ensured when looking for a given control</param>
        public static Control GetChildControl(Control control, Type type, bool ensureChildControls = true)
        {
            if (control != null)
            {
                // Ensure child controls
                var ensure = control as IEnsureControls;
                if (ensureChildControls && (ensure != null))
                {
                    ensure.EnsureControls();
                }

                // Check immediate child controls
                foreach (Control c in control.Controls)
                {
                    // If the control matches, return the control
                    if (type.IsInstanceOfType(c))
                    {
                        return c;
                    }
                }

                // Check nested child controls
                foreach (Control c in control.Controls)
                {
                    Control found = GetChildControl(c, type, ensureChildControls);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Gets the last child control of specified type for given control.
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="type">Control type</param>
        public static Control GetLastChildControl(Control control, Type type)
        {
            // Check immediate child controls
            for (int i = control.Controls.Count - 1; i >= 0; i--)
            {
                // If the control matches, return the control
                if (type.IsInstanceOfType(control.Controls[i]))
                {
                    return control.Controls[i];
                }
            }

            // Check nested child controls
            for (int i = control.Controls.Count - 1; i >= 0; i--)
            {
                Control found = GetLastChildControl(control.Controls[i], type);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }


        /// <summary>
        /// Gets the first child control of specified ID for given control.
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="type">Control type</param>
        /// <param name="controlId">Child control ID</param>
        public static Control GetChildControl(Control control, Type type, string controlId)
        {
            return GetChildControl(control, new[] { type }, controlId);
        }


        /// <summary>
        /// Gets the first child control of specified ID for given control.
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="types">Array of controls types to find</param>
        /// <param name="controlId">Child control ID</param>
        public static Control GetChildControl(Control control, Type[] types, string controlId)
        {
            if ((!String.IsNullOrEmpty(controlId)) && (control != null))
            {
                // Check immediate child controls
                foreach (Control c in control.Controls)
                {
                    // If control was founded, return the control
                    Control founded = c.FindControl(controlId);
                    if (founded != null)
                    {
                        foreach (Type type in types)
                        {
                            if (type.IsInstanceOfType(founded))
                            {
                                return founded;
                            }
                        }
                    }
                }

                // Check nested child controls
                foreach (Control c in control.Controls)
                {
                    Control found = GetChildControl(c, types, controlId);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Gets the update panel for given control.
        /// </summary>
        /// <param name="control">Control</param>
        public static UpdatePanel GetUpdatePanel(Control control)
        {
            return (UpdatePanel)GetParentControl(control, typeof(UpdatePanel));
        }


        /// <summary>
        /// Gets the update panel for given control.
        /// </summary>
        /// <param name="control">Control</param>
        public static IDataControl GetDataControl(Control control)
        {
            return (IDataControl)GetParentControl(control, typeof(IDataControl));
        }


        /// <summary>
        /// Register client script block. If current control is under update panel, script is registered
        /// in the update panel
        /// </summary>
        /// <param name="ctrl">Current control</param>
        /// <param name="page">Current page</param>
        /// <param name="type">Type</param>
        /// <param name="key">Script key</param>
        /// <param name="script">Script</param>
        public static void RegisterClientScriptBlock(Control ctrl, Page page, Type type, string key, string script)
        {
            if (IsInUpdatePanel(ctrl))
            {
                ScriptManager.RegisterStartupScript(page, type, key, script, false);
            }
            else
            {
                ScriptHelper.RegisterClientScriptBlock(page, type, key, script);
            }
        }


        /// <summary>
        /// Registers the control which causes postback with the script manager.
        /// </summary>
        /// <param name="control">Control to register</param>
        /// <param name="locationControl">Optional control used as reference for postback source. Postback target can be different than postback source.</param>
        public static void RegisterPostbackControl(Control control, Control locationControl = null)
        {
            Page page = control?.Page;
            if (page == null)
            {
                return;
            }

            if (IsInUpdatePanel(locationControl ?? control))
            {
                ScriptManager.GetCurrent(page)?.RegisterPostBackControl(control);
            }
        }


        /// <summary>
        /// Gets the unique ID for the control.
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <param name="baseId">Base control ID</param>
        /// <param name="control">Control which will be assigned with the ID</param>
        public static string GetUniqueID(Control parent, string baseId, Control control = null)
        {
            // Check if ID already exists
            Control found = parent.FindControl(baseId);
            if ((found == null) || (found == control))
            {
                return baseId;
            }

            // Remove the numbers from the base Id
            baseId = Regex.Replace(baseId, "\\d+$", "");

            // Check if ID already exists
            found = parent.FindControl(baseId);
            if ((found == null) || (found == control))
            {
                return baseId;
            }

            int index = 1;
            string currentId = baseId + index;

            // Get the proper index for uniqueness
            found = parent.FindControl(currentId);
            while ((found != null) && (found != control))
            {
                index++;
                currentId = baseId + index;
                found = parent.FindControl(currentId);
            }

            return currentId;
        }


        /// <summary>
        /// Returns context manager for specified control.
        /// </summary>
        /// <param name="control">Target control</param>
        public static IControlContextManager GetContextManager(Control control)
        {
            return GetParentControl(control, typeof(IControlContextManager)) as IControlContextManager;
        }


        /// <summary>
        /// Returns true if specified control is under specified context name.
        /// </summary>
        /// <param name="control">Target control</param>
        /// <param name="contextName">Context name</param>
        public static bool CheckControlContext(Control control, string contextName)
        {
            // Get current manager
            IControlContextManager manager = GetContextManager(control);

            // Check if manager is defined and contains control context object
            if ((manager != null) && (manager.ControlContext != null))
            {
                if (CMSString.Compare(manager.ControlContext.ContextName, contextName, true) == 0)
                {
                    return true;
                }
            }

            return false;
        }


        #region "List enum methods"

        /// <summary>
        /// Fills the specified <see cref="ListControl"/> with the items created from the specified enum type.
        /// </summary>
        /// <param name="control">List control</param>
        /// <param name="resourcePrefix">
        /// The resource prefix used for the item text localization.
        /// Defaults to null.
        /// </param>
        /// <param name="sort">
        /// Indicates whether the items should be sorted.
        /// If true, the items are sorted by the item text after localization, otherwise the order of items corresponds to the enum declaration.
        /// Defaults to false.
        /// </param>
        /// <param name="useStringRepresentation">
        /// Indicates if string representation specified by the <see cref="EnumStringRepresentationAttribute"/> attribute will be used.
        /// Defaults to false.
        /// </param>
        /// <param name="excludedValues">Exclude values from enumeration</param>
        /// <remarks>
        /// Takes the <see cref="EnumStringRepresentationAttribute"/> into account when working with the enum values.
        /// </remarks>
        public static void FillListControlWithEnum<TEnum>(ListControl control, string resourcePrefix = null, bool sort = false, bool useStringRepresentation = false, List<string> excludedValues = null)
        {
            FillListControlWithEnum(control, typeof(TEnum), resourcePrefix, sort, useStringRepresentation, excludedValues);
        }


        /// <summary>
        /// Fills the specified <see cref="ListControl"/> with the items created from the specified enum type.
        /// </summary>
        /// <param name="control">List control</param>
        /// <param name="enumType">Enum type</param>
        /// <param name="resourcePrefix">
        /// The resource prefix used for the item text localization.
        /// Defaults to null.
        /// </param>
        /// <param name="sort">
        /// Indicates whether the items should be sorted.
        /// If true, the items are sorted by the item text after localization, otherwise the order of items corresponds to the enum declaration.
        /// Defaults to false.
        /// </param>
        /// <param name="useStringRepresentation">
        /// Indicates if string representation specified by the <see cref="EnumStringRepresentationAttribute"/> attribute will be used for the item value.
        /// Defaults to false.
        /// </param>
        /// <param name="excludedValues">Exclude values from enumeration</param>
        /// <remarks>
        /// Takes the <see cref="EnumStringRepresentationAttribute"/> into account when working with the enum values.
        /// </remarks>
        /// <param name="selectedCategories">
        /// Only enums from selected enum categories will be used.
        /// </param>
        public static void FillListControlWithEnum(ListControl control, Type enumType, string resourcePrefix = null, bool sort = false, bool useStringRepresentation = false, List<string> excludedValues = null, List<string> selectedCategories = null)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }

            if (!enumType.IsEnum)
            {
                throw new ArgumentException("The specified type is not an enum type.");
            }

            // Get the enum names, if there are selected categories, get names only from
            var names = (selectedCategories == null || selectedCategories.Count < 1) ? Enum.GetNames(enumType).ToList() : EnumHelper.GetEnumsByCategories(enumType, selectedCategories).Select(e => e.ToStringRepresentation());

            // Sort the enum names by order
            names = names.OrderBy(name =>
                {
                    var value = (Enum)Enum.Parse(enumType, name);
                    return value.GetOrder();
                }).ToList();

            // Get the list items and filter excluded values
            var listItems = names.Select(name => GetEnumListItem(control, enumType, name, resourcePrefix, useStringRepresentation));

            // Filter excluded values
            if ((excludedValues != null) && (excludedValues.Count > 0))
            {
                listItems = listItems.Where(t => !excludedValues.Exists(v => v.Equals(t.Value, StringComparison.InvariantCulture)));
            }

            // Sort the list items if required
            if (sort)
            {
                listItems = listItems.OrderBy(i => i.Text);
            }

            // Add the list items to the control
            control.Items.AddRange(listItems.ToArray());
        }


        /// <summary>
        /// Gets list item for enumeration.
        /// </summary>
        /// <param name="listControl">List control</param>
        /// <param name="enumType">Enum type</param>
        /// <param name="name">Enum item name</param>
        /// <param name="resourcePrefix">
        /// The resource prefix used for the item text localization.
        /// Defaults to null.
        /// </param>
        /// <param name="useStringRepresentation">
        /// Indicates if string representation specified by the <see cref="EnumStringRepresentationAttribute"/> attribute will be used for the item value.
        /// Defaults to false.
        /// </param>
        private static ListItem GetEnumListItem(ListControl listControl, Type enumType, string name, string resourcePrefix, bool useStringRepresentation)
        {
            // Get prefix
            var resourcePrefixExists = !String.IsNullOrEmpty(resourcePrefix);
            var namePrefix = (resourcePrefixExists ? resourcePrefix : enumType.Name) + ".";

            // Get string representation
            var value = (Enum)Enum.Parse(enumType, name);
            var stringRepresentation = value.ToStringRepresentation();
            var stringRepresentationExists = !String.IsNullOrEmpty(stringRepresentation);

            // Get item value
            var itemValue = (stringRepresentationExists && useStringRepresentation) ? stringRepresentation : value.ToString("D");

            // Get item name
            var nameValue = (stringRepresentationExists ? stringRepresentation : name);
            var nameResource = namePrefix + nameValue;
            if (resourcePrefixExists)
            {
                // Get default resource string
                var nameDefaultResource = enumType.Name + "." + nameValue;
                nameResource += "|" + nameDefaultResource;
            }

            var itemName = listControl.GetString(nameResource);

            // Create list item
            return new ListItem(itemName, itemValue);
        }

        #endregion


        /// <summary>
        /// Loads the control extender of the specified type for the specified control.
        /// </summary>
        /// <param name="assemblyName">Assembly name where the extender type is located</param>
        /// <param name="className">Class name of the extender type</param>
        /// <param name="control">Control to be extended</param>
        public static ControlExtender LoadExtender(string assemblyName, string className, Control control)
        {
            var extender = (ControlExtender)ClassHelper.GetClass(assemblyName, className);
            if (extender != null)
            {
                extender.Init(control);
                return extender;
            }

            throw new Exception(string.Format("Extender with assembly name '{0}' and class '{1}' was not found.", assemblyName, className));
        }


        /// <summary>
        /// Fill list control with SQL operators (Like, NotLike, Equals, NotEquals) as
        /// value and text with their appropriate text eqivalent.
        /// </summary>
        public static void FillListWithTextSqlOperators(ListControl listControl)
        {
            listControl.AddLocalizedItem("filter.like", WhereBuilder.LIKE);
            listControl.AddLocalizedItem("filter.notlike", WhereBuilder.NOT_LIKE);
            listControl.AddLocalizedItem("filter.equals", WhereBuilder.EQUAL);
            listControl.AddLocalizedItem("filter.notequals", WhereBuilder.NOT_EQUAL);
        }


        /// <summary>
        /// Fill list control with SQL operators as number value
        /// (Like = 0, NotLike = 1, Equals = 2, NotEquals = 3)
        /// and text with their appropriate text eqivalent.
        /// </summary>
        public static void FillListWithNumberedSqlOperators(ListControl listControl)
        {
            listControl.AddLocalizedItem("filter.like", "0");
            listControl.AddLocalizedItem("filter.notlike", "1");
            listControl.AddLocalizedItem("filter.equals", "2");
            listControl.AddLocalizedItem("filter.notequals", "3");
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Searches provided string for {%%} macros and returns list of replacable strings separated with semicolon.
        /// </summary>
        /// <param name="txt">String to be searched</param>
        private static string GetReplacableStrings(string txt)
        {
            // Read page source
            string result = ";";

            Match m = RegExStrings.Match(txt);
            while (m.Success)
            {
                if (result.IndexOf(";" + m.Groups[1].Value + ";", StringComparison.InvariantCulture) < 0)
                {
                    result += m.Groups[1].Value + ";";
                }
                m = m.NextMatch();
            }

            result = result.Substring(1);
            if (result.Length > 0)
            {
                result = result.Substring(0, result.Length - 1);
            }

            return result;
        }


        /// <summary>
        /// Returns a 2-dimensional string array of pairs - replacables string and its value.
        /// </summary>
        /// <param name="txt">Text to be searched</param>
        /// <param name="dr">DataRow object with item data</param>
        private static string[,] GetReplacedStrings(string txt, DataRow dr)
        {
            var replacablesArr = GetReplacableStrings(txt).Split(';');

            string[,] replacs = new string[replacablesArr.GetUpperBound(0) + 1, 2];

            int replIndex;
            for (replIndex = replacablesArr.GetLowerBound(0); replIndex <= replacablesArr.GetUpperBound(0); replIndex++)
            {
                if (replacablesArr[replIndex] != null && replacablesArr[replIndex] != "")
                {
                    replacs[replIndex, 0] = replacablesArr[replIndex];
                    replacs[replIndex, 1] = Convert.ToString(dr[replacablesArr[replIndex]]) + "";
                }
            }
            return replacs;
        }


        /// <summary>
        /// Returns path to file system location of special dynamic controls.
        /// </summary>
        /// <param name="controlName">dynamic control code name</param>
        private static string GetInlineControlPath(string controlName)
        {
            string path = null;

            switch (controlName.ToLowerInvariant())
            {
                // Report graph
                case "reportgraph":
                    path = "~/CMSModules/Reporting/Controls/ReportGraph.ascx";
                    break;

                // Report HTML graph
                case "reporthtmlgraph":
                    path = "~/CMSModules/Reporting/Controls/HtmlBarGraph.ascx";
                    break;

                // Report table
                case "reporttable":
                    path = "~/CMSModules/Reporting/Controls/ReportTable.ascx";
                    break;

                // Report value
                case "reportvalue":
                    path = "~/CMSModules/Reporting/Controls/ReportValue.ascx";
                    break;

                // Web part zone
                case "webpartzone":
                    path = "~/CMSInlineControls/WebPartZone.ascx";
                    break;

                // Media
                case "mediacontrol":
                case "media":
                    path = "~/CMSInlineControls/MediaControl.ascx";
                    break;

                // Image
                case "imagecontrol":
                    path = "~/CMSInlineControls/ImageControl.ascx";
                    break;

                // YouTube
                case "youtubevideo":
                    path = "~/CMSInlineControls/YouTubeControl.ascx";
                    break;

                // Inline widget
                case "widget":
                    path = "~/CMSModules/Widgets/InlineControl/InlineWidget.ascx";
                    break;

                // Sub-level placeholder
                case "sublevelplaceholder":
                    path = "~/CMSInlineControls/SubLevelPlaceHolder.ascx";
                    break;
            }

            return path;
        }

        #endregion
    }
}