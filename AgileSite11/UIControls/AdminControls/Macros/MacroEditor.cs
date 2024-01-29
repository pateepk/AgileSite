using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.PortalEngine;

using TreeNode = CMS.DocumentEngine.TreeNode;

namespace CMS.UIControls
{
    /// <summary>
    /// Macro editor control
    /// </summary>
    public class MacroEditor : CMSPlaceHolder, ICallbackEventHandler
    {
        #region "Hint container"

        /// <summary>
        /// Hint container
        /// </summary>
        protected class Hint : IComparable<Hint>
        {
            #region "Variables"

            /// <summary>
            /// Hint / Member name
            /// </summary>
            public readonly string Name;

            /// <summary>
            /// Member icon
            /// </summary>
            public readonly string Icon;

            /// <summary>
            /// Code snippet that the member represents
            /// </summary>
            public string Snippet;

            /// <summary>
            /// Member comment
            /// </summary>
            public string Comment;

            #endregion


            #region "Methods"

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="name">Name</param>
            /// <param name="icon">Icon</param>
            public Hint(string name, string icon = "")
            {
                Name = name;
                Icon = icon;
            }


            /// <summary>
            /// Compares the object to another
            /// </summary>
            /// <param name="other">Other object</param>
            public int CompareTo(Hint other)
            {
                return String.Compare(Name, other.Name, StringComparison.Ordinal);
            }


            /// <summary>
            /// Returns true if the object equals to another
            /// </summary>
            /// <param name="obj">Other object</param>
            public override bool Equals(object obj)
            {
                var other = obj as Hint;
                if (other == null)
                {
                    return false;
                }

                return Name.EqualsCSafe(other.Name);
            }


            /// <summary>
            /// Gets the hash code of the object
            /// </summary>
            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }

            #endregion
        }

        #endregion


        #region "Constants"

        // Property icon name
        private const string ICON_NAME_PROPERTY = "icon-me-property";

        // Method icon name
        private const string ICON_NAME_METHOD = "icon-me-method";

        // Snippet icon name
        private const string ICON_NAME_SNIPPET = "icon-me-snippet";

        // Default icon name
        private const string ICON_NAME_DEFAULT = "icon-me-abstractobjectcollection";

        #endregion


        #region "Variables"

        private MacroResolver mResolver;
        private MacroResolver mASCXResolver;

        private static List<Hint> mSnippets;

        private bool mUseAutoComplete = true;
        private bool mMixedMode = true;

        private static readonly SafeDictionary<Type, string> mTypeIcons = new SafeDictionary<Type, string>();

        // List of icon names
        private static readonly HashSet<string> iconCssClasses = new HashSet<string>
        {
            ICON_NAME_DEFAULT,
            "icon-me-binding",
            "icon-me-boolean",
            "icon-me-datetime",
            "icon-me-double",
            "icon-me-decimal",
            "icon-me-false",
            "icon-me-children",
            "icon-me-icontext",
            "icon-me-ilist",
            "icon-me-imacronamespace",
            "icon-me-info",
            "icon-me-int32",
            ICON_NAME_METHOD,
            "icon-me-null",
            "icon-me-number",
            "icon-me-parent",
            ICON_NAME_PROPERTY,
            "icon-me-referring",
            "icon-me-sitebinding",
            ICON_NAME_SNIPPET,
            "icon-me-string",
            "icon-me-true",
            "icon-me-value"
        };

        private string wholeText;
        private string text = "";
        private int position;
        private bool isShowContext;

        private ExtendedTextArea mEditor;
        private LiteralControl mContextHelp;

        private readonly Hint separatorHint = new Hint("----");

        #endregion


        #region "Properties"

        /// <summary>
        /// Determines whether the editor is in single line mode (i.e. only one line of text can be edited).
        /// </summary>
        public bool SingleLineMode
        {
            get
            {
                return Editor.SingleLineMode;
            }
            set
            {
                Editor.SingleLineMode = value;
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether the Web server control is enabled.
        /// </summary>
        public virtual bool Enabled
        {
            get
            {
                return Editor.Enabled;
            }
            set
            {
                Editor.Enabled = value;
            }
        }


        /// <summary>
        /// Gets or sets the root object the fields of which will be used as root help in ASCX mode.
        /// </summary>
        public object ASCXRootObject
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the editor is in ASCX transformation mode (i.e. AutoCompletion fires only when requested with CTRL+SPACE and only fields are shown)
        /// </summary>
        private bool ASCXMode
        {
            get
            {
                return (ASCXRootObject != null);
            }
        }


        /// <summary>
        /// Gets the list of predefined snippets.
        /// </summary>
        protected static IEnumerable<Hint> PredefinedSnippets
        {
            get
            {
                if (mSnippets == null)
                {
                    var list = GetSnippets();

                    mSnippets = list;
                }

                return mSnippets;
            }
        }


        /// <summary>
        /// Gets or sets the list of currentText namespaces for which the methods are permanently available (visible in AutoCompletion without any context).
        /// The namespaces are used only for root hints
        /// </summary>
        public List<string> NamespaceUsings
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the name of the resolver to use. This property is used if Resolver property is not explicitly set.
        /// </summary>
        public string ResolverName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ResolverName"], "");
            }
            set
            {
                ViewState["ResolverName"] = value;
                Editor.ResolverName = value;
            }
        }


        /// <summary>
        /// Gets or sets context resolver used for getting autocomplete options.
        /// </summary>
        public MacroResolver Resolver
        {
            get
            {
                return mResolver ?? (mResolver = CreateResolver());
            }
            set
            {
                mResolver = value;
            }
        }


        /// <summary>
        /// Gets or sets context resolver used for getting autocomplete options in ASCX Mode.
        /// </summary>
        protected MacroResolver ASCXResolver
        {
            get
            {
                if (ASCXRootObject == null)
                {
                    return Resolver;
                }

                return mASCXResolver ?? (mASCXResolver = CreateASCXResolver());
            }
        }


        /// <summary>
        /// Indicates whether the autocompletion feature is enabled or not.
        /// </summary>
        public bool UseAutoComplete
        {
            get
            {
                return mUseAutoComplete && Editor.SyntaxHighlightingEnabled;
            }
            set
            {
                mUseAutoComplete = value;
            }
        }


        /// <summary>
        /// Gets or sets the code which is executed when editor lost focus.
        /// </summary>
        public string EditorFocusLostScript
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the editor is in pure currentText editing mode (whole currentText is considered as currentText) or mixed mode, where auto completion is fired only inside {%%} environment.
        /// </summary>
        public bool MixedMode
        {
            get
            {
                return mMixedMode;
            }
            set
            {
                mMixedMode = value;
                Editor.IsMacroMixedMode = value;
            }
        }


        /// <summary>
        /// Gets or sets the left offset of the autocomplete control (to pos it correctly).
        /// </summary>
        public int LeftOffset
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the top offset of the autocomplete control (to pos it correctly).
        /// </summary>
        public int TopOffset
        {
            get;
            set;
        }


        /// <summary>
        /// If true, tree is shown above the editor, otherwise it is below (default pos is below).
        /// </summary>
        public bool ShowAutoCompletionAbove
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the name of java script object of the auto completion extender.
        /// </summary>
        public string AutoCompletionObject
        {
            get
            {
                return "autoCompleteObj_" + ClientID;
            }
        }


        /// <summary>
        /// If true, no global objects are shown, only inner sources (named data sources, etc.) are shown as hints.
        /// </summary>
        public bool ShowOnlyInnerSources
        {
            get;
            set;
        }

        #endregion


        #region "Mirrored editor properties"

        /// <summary>
        /// Gets the editor control.
        /// </summary>
        public ExtendedTextArea Editor
        {
            get
            {
                EnsureChildControls();

                return mEditor;
            }
            set
            {
                mEditor = value;
            }
        }


        /// <summary>
        /// Gets if syntax highlighting is enabled for this control.
        /// </summary>
        public bool SyntaxHighlightingEnabled
        {
            get
            {
                return Editor.SyntaxHighlightingEnabled;
            }
        }


        /// <summary>
        /// Gets or sets whether the control is read only
        /// </summary>
        public bool ReadOnly
        {
            get
            {
                return Editor.ReadOnly;
            }
            set
            {
                Editor.ReadOnly = value;
            }
        }


        /// <summary>
        /// Gets or sets the width of the editor
        /// </summary>
        public virtual Unit Width
        {
            get
            {
                return Editor.Width;
            }
            set
            {
                Editor.Width = value;
            }
        }


        /// <summary>
        /// Gets or sets the height of the editor
        /// </summary>
        public virtual Unit Height
        {
            get
            {
                return Editor.Height;
            }
            set
            {
                Editor.Height = value;
            }
        }


        /// <summary>
        /// Gets or sets the editor currentText.
        /// </summary>
        public string Text
        {
            get
            {
                return Editor.Text;
            }
            set
            {
                Editor.Text = value;
            }
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// Creates inner controls
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            mContextHelp = new LiteralControl();

            Controls.Add(mContextHelp);

            mEditor = new ExtendedTextArea
            {
                ID = "txtCode",
                EditorMode = EditorModeEnum.Advanced,
                Width = 500,
                Height = 200,
                Language = LanguageEnum.CSharp,
                ShowInsertMacro = true,
                TextMode = TextBoxMode.SingleLine
            };

            Controls.Add(mEditor);
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (UseAutoComplete && Editor.Enabled && !ReadOnly)
            {
                string editorObjName = "autoCompleteObj_" + ClientID;

                // Register main currentText editor scripts
                ScriptHelper.RegisterJQuery(Page);

                ScriptHelper.RegisterScriptFile(Page, "Macros/MacroEditor.js");
                ScriptHelper.RegisterClientScriptBlock(Page, typeof(string), "MacroEditorCallbackScript_" + ClientID, String.Format(
                    @"function handleCallback_{0}(value, context) {{
    if (context == 'context') {{
        {1}.fillContext(value);
    }} else {{
        {1}.fillHints(value);
    }}
}}"
                    , ClientID
                    , editorObjName)
                    , true);

                mContextHelp.Text = String.Format(
@"
<div id=""{0}_ctx"" {1}></div>
<div id=""{0}_quickCtx"" {1}></div>
<div id=""{0}_hints"" {1}>
    <ul>
    </ul>
</div>
",
                   ClientID,
                   "style=\"direction: ltr; display: none;\""
               );
            }
        }


        /// <summary>
        /// Render event handler
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            if (UseAutoComplete && Editor.Enabled && !ReadOnly)
            {
                // We need to generate this script on Render since extended area does that that late as well
                // and editor object is not available before
                string cbRefShowHint = Page.ClientScript.GetCallbackEventReference(this, "this.callbackArgument", "handleCallback_" + ClientID, "'hint'", true);
                string cbRefShowContext = Page.ClientScript.GetCallbackEventReference(this, "this.callbackArgument", "handleCallback_" + ClientID, "'context'", true);

                string script =
                    "var ac = " + AutoCompletionObject + " = new AutoCompleteExtender(" + Editor.EditorID + ", document.getElementById('" + ClientID + "_hints'), document.getElementById('" + ClientID + "_ctx'), document.getElementById('" + ClientID + "_quickCtx'), " + (MixedMode ? "true" : "false") + ", " + TopOffset + ", " + LeftOffset + ", " + (ASCXMode ? "true" : "false") + "); \n" +
                    "ac.callbackContext = function() { " + cbRefShowContext + "; }\n" +
                    "ac.callbackHint = function() { " + cbRefShowHint + "; }\n" +
                    "ac.editorFocusLost = function editorFocusLost_" + ClientID + "() {" + EditorFocusLostScript + "}\n";

                if (ShowAutoCompletionAbove)
                {
                    script += AutoCompletionObject + ".forceAbove = true; \n";
                }

                ScriptHelper.RegisterStartupScript(Page, typeof(string), "MacroEditorScript_" + ClientID, script, true);
            }
        }

        #endregion


        #region "Methods"

        private MacroResolver CreateASCXResolver()
        {
            var resolver = Resolver.CreateChild();

            resolver.Settings.VirtualMode = true;
            resolver.Settings.CheckSecurity = false;
            resolver.Settings.EncapsulateMacroObjects = false;
            resolver.SetNamedSourceData("ASCXRootObject", ASCXRootObject);
            return resolver;
        }


        private MacroResolver CreateResolver()
        {
            MacroResolver resolver;
            var resolverName = ResolverName;

            if (!string.IsNullOrEmpty(resolverName))
            {
                resolver = MacroResolverStorage.GetRegisteredResolver(resolverName);
            }
            else
            {
                // Setup the default resolver
                resolver = MacroContext.CurrentResolver.CreateChild();
            }

            resolver.Settings.VirtualMode = true;
            resolver.Settings.EncapsulateMacroObjects = false;
            return resolver;
        }


        /// <summary>
        /// Handles autocomplete action.
        /// </summary>
        /// <param name="currentText">Macro expression</param>
        /// <param name="pos">Caret pos</param>
        /// <param name="showContext">If true, method context should be returned, otherwise hints are returned</param>
        /// <param name="previousLines">Text of previous lines</param>
        private string AutoComplete(string currentText, int pos, bool showContext, string previousLines)
        {
            // Do lexical analysis, suppress errors
            List<MacroElement> lexems = MacroElement.ParseExpression(currentText, true);

            // Variables declared on actual line;
            List<string> inlineVariables = new List<string>();

            // Locate current element, find all created variables
            int currentElementIndex = -1;
            for (int i = 0; i < lexems.Count; i++)
            {
                MacroElement el = lexems[i];
                MacroElement elNext = (i < lexems.Count - 1 ? lexems[i + 1] : null);
                if (elNext == null || ((el.StartIndex <= pos) && (elNext.StartIndex > pos)))
                {
                    currentElementIndex = i;
                    break;
                }
                AddLocalVariable(inlineVariables, lexems, i);
            }

            if (showContext)
            {
                return GetMethodContextHelp(lexems, currentElementIndex);
            }

            // If we are in the middle of the comment or a constant, do not show any help
            MacroElement currentElement = (currentElementIndex != -1 ? lexems[currentElementIndex] : null);
            if ((currentElement != null) && ((currentElement.Type == ElementType.Boolean) || (currentElement.Type == ElementType.Double) ||
                                             (currentElement.Type == ElementType.Integer) || (currentElement.Type == ElementType.String)))
            {
                return "";
            }

            return GetHints(lexems, currentElementIndex, previousLines, inlineVariables);
        }


        /// <summary>
        /// Returns list of possible hints.
        /// </summary>
        /// <param name="currentText">Text passed from JS</param>
        /// <param name="pos">Position within the currentText</param>
        private string AutoCompleteASCX(string currentText, int pos)
        {
            var members = new List<Hint>();

            // Only part up to the cursor is interesting
            currentText = currentText.Substring(0, pos);

            // Parse parent expression
            string parentExpr = currentText;
            for (int i = currentText.Length - 1; i >= 0; i--)
            {
                char c = currentText[i];
                if (!(Char.IsLetterOrDigit(c) || (c == '.')))
                {
                    parentExpr = currentText.Substring(i + 1, currentText.Length - i - 1);
                    break;
                }
            }

            bool innerHints = false;

            if (!string.IsNullOrEmpty(parentExpr))
            {
                int index = parentExpr.LastIndexOfCSafe('.');
                if (index > 0)
                {
                    parentExpr = parentExpr.Substring(0, index);

                    var result = ASCXResolver.ResolveMacroExpression("ASCXRootObject." + parentExpr, true);
                    if ((result != null) && (result.Result != null))
                    {
                        GetObjectFields(members, null, result.Result);
                    }

                    innerHints = true;
                }
                else
                {
                    GetObjectFields(members, null, ASCXRootObject);
                }
            }
            else
            {
                GetObjectFields(members, null, ASCXRootObject);
            }

            // Sort everything
            members.Sort();

            // Add prioritized items at the beginning
            HashSet<string> prioritiesSet = Resolver.GetPrioritizedDataNames();
            if (!innerHints && (prioritiesSet.Count > 0))
            {
                var priorities = prioritiesSet.Select(x => new Hint(x)).ToList();
                priorities.Sort();

                members.Insert(0, separatorHint);
                members.InsertRange(0, priorities);
            }

            return GetHintsOutput(members);
        }


        /// <summary>
        /// Returns list of possible hints.
        /// </summary>
        /// <param name="lexems">Lexems from lexical analysis</param>
        /// <param name="currentElementIndex">Index of the current lexem</param>
        /// <param name="previousLines">Previous lines to get previous variable declarations</param>
        /// <param name="inlineVariables">List of variables declared within the macro</param>
        private string GetHints(List<MacroElement> lexems, int currentElementIndex, string previousLines, IEnumerable<string> inlineVariables)
        {
            // Lists with methods and properties which are to be showed as a help
            var members = new List<Hint>();

            var dataProperties = new List<string>();

            // Get the datamember context for which to receive help
            string dataMemberContext = GetDataMemberContext(lexems, currentElementIndex);

            // Analyze code before current line to find variable declarations (makes sense only for root hints - i.e. datamember context is empty)
            if (string.IsNullOrEmpty(dataMemberContext))
            {
                AddLocalVariables(members, previousLines, inlineVariables);
            }

            // Add properties of given context, store the object to get the type for which the methods should be added
            object obj = AddPropertiesForDataMember(members, dataProperties, dataMemberContext);

            // Do not process further if only prioritized properties should be handled
            if (string.IsNullOrEmpty(dataMemberContext) && Resolver.ShowOnlyPrioritized)
            {
                members.AddRange(Resolver.GetPrioritizedDataNames().Select(x => new Hint(x, GetDataMemberIconName(x))));
                members.Sort();

                return GetHintsOutput(members);
            }

            if (string.IsNullOrEmpty(dataMemberContext))
            {
                // Add methods which belongs to permanently registered namespaces
                AddUsingsMethods(members);

                // ROOT HINTS

                // Add commands only for empty contexts
                members.AddRange(PredefinedSnippets);

                // Add list of methods which are registered for any type
                FillFromMethodList(members, MacroMethods.GetMethodsForObject(null));
            }
            else if (obj != null)
            {
                // SPECIFIC OBJECT HINTS

                AddObjectMethods(members, obj);
            }

            // Sort everything
            members.Sort();

            var resultList = members.Distinct();

            // Remove items which should not be displayed
            if (Resolver.Settings.DisablePageContextMacros)
            {
                // Remove page context macros
                resultList = resultList.Where(hint => (hint.Name != "CurrentDocument") && (hint.Name != "CurrentPageInfo"));
            }

            // Remove context objects if not needed
            if (Resolver.Settings.DisableContextObjectMacros)
            {
                // Remove page context macros
                resultList = resultList.Where(hint => !hint.Name.EndsWithCSafe("context", true));
            }

            // Create final list of hints
            var finalList = new List<Hint>();
            finalList.AddRange(resultList);

            // Insert prioritized items at the beginning
            HashSet<string> prioritiesSet = Resolver.GetPrioritizedDataNames();
            if (string.IsNullOrEmpty(dataMemberContext) && (prioritiesSet.Count > 0))
            {
                List<string> priorities = new List<string>(prioritiesSet);
                priorities.Sort();

                finalList.Insert(0, separatorHint);
                finalList.InsertRange(0, priorities.Distinct().Select(x => new Hint(x, GetDataMemberIconName(x))));
            }

            // Add data properties at the end
            if (dataProperties.Count > 0)
            {
                finalList.Add(separatorHint);
                finalList.AddRange(dataProperties.Distinct().Select(x => new Hint(x, ICON_NAME_PROPERTY)));
            }

            // Return the result
            return GetHintsOutput(finalList);
        }


        private string GetHintsOutput(List<Hint> members)
        {
            return ScriptHelper.JsonSerializeObject(members);
        }


        /// <summary>
        /// Analyzes given lexems and determines current data member.
        /// </summary>
        /// <param name="lexems">Lexems of the currently typed expression</param>
        /// <param name="currentElementIndex">Index of the lexem where the cursor is</param>
        private static string GetDataMemberContext(List<MacroElement> lexems, int currentElementIndex)
        {
            if ((currentElementIndex < 0) || (currentElementIndex >= lexems.Count))
            {
                return "";
            }

            MacroElement currentElement = lexems[currentElementIndex];

            // Properties autocomplete
            bool showAllProperties = (currentElement.Type == ElementType.LeftBracket) || (currentElement.Type == ElementType.LeftIndexer) ||
                                     (currentElement.Type == ElementType.BlockStart) || (currentElement.Type == ElementType.BlockEnd) ||
                                     (currentElement.Type == ElementType.Comma) || MacroElement.IsValidOperator(currentElement.Expression);

            string dataMemeberContext = "";
            if (!showAllProperties)
            {
                // We need to find current datamember context
                int brackets = 0;
                for (int i = currentElementIndex - 1; i >= 0; i--)
                {
                    ElementType type = lexems[i].Type;

                    if (brackets == 0)
                    {
                        // We need to take context to the left side of current lexem
                        // It needs to preserve the structure (deepnes withing the brackets)
                        if ((type == ElementType.Comma) || (type == ElementType.Operator) ||
                            (type == ElementType.LeftBracket) || (type == ElementType.LeftIndexer) ||
                            (type == ElementType.BlockStart) || (type == ElementType.BlockEnd))
                        {
                            break;
                        }
                    }

                    // Append this part
                    dataMemeberContext = lexems[i].Expression + dataMemeberContext;

                    // Ensure correct deepnes within the brackets
                    if ((type == ElementType.RightBracket) || (type == ElementType.RightIndexer) || (type == ElementType.BlockEnd))
                    {
                        brackets++;
                    }
                    else if ((type == ElementType.LeftBracket) || (type == ElementType.LeftIndexer) || (type == ElementType.BlockStart))
                    {
                        brackets--;
                    }
                }
                dataMemeberContext = dataMemeberContext.TrimEnd('.');
            }
            return dataMemeberContext;
        }


        /// <summary>
        /// Resolves the data member context and adds correct properties options to a given list.
        /// </summary>
        /// <param name="propertyList">List of properties where to add the hints</param>
        /// <param name="dataProperties">List of data properties where to add data hints</param>
        /// <param name="dataMemberContext">Data member context (currentText part)</param>
        private object AddPropertiesForDataMember(List<Hint> propertyList, List<string> dataProperties, string dataMemberContext)
        {
            if (string.IsNullOrEmpty(dataMemberContext))
            {
                if (Resolver.ShowOnlyPrioritized)
                {
                    return null;
                }

                var propToAdd = new List<string>();

                if (!ShowOnlyInnerSources)
                {
                    // Add CMSDataContext.Current
                    if (!Resolver.Settings.DisableContextMacros)
                    {
                        propToAdd.AddRange(CMSDataContext.Current.Properties);
                    }
                }

                // Add all members contained in given resolver
                GetRootFields(propToAdd);

                propertyList.AddRange(propToAdd.Select(x => new Hint(x, GetDataMemberIconName(x))));

                return null;
            }

            // Disable security check and resolve the currentText
            Resolver.Settings.IdentityOption = MacroIdentityOption.FromUserInfo(MembershipContext.AuthenticatedUser);
            Resolver.Settings.CheckIntegrity = false;
            Resolver.Settings.EncapsulateMacroObjects = false;

            var result = Resolver.ResolveMacroExpression(dataMemberContext, true);
            if (result != null)
            {
                if (result.SecurityPassed)
                {
                    if (result.Match)
                    {
                        // Fill at most MaxMacroNodes properties to AutoCompletion data
                        GetObjectFields(propertyList, dataProperties, result.Result);

                        if (dataProperties.Count > MacroStaticSettings.MaxMacroNodes)
                        {
                            dataProperties.RemoveRange(MacroStaticSettings.MaxMacroNodes, dataProperties.Count - MacroStaticSettings.MaxMacroNodes);
                        }
                    }
                }
                else
                {
                    propertyList.Add(new Hint(ResHelper.GetString("macrodesigner.accessdenied")));
                }

                return result.Result;
            }

            return null;
        }


        /// <summary>
        /// Adds to given list all the data contained in resolver (sourcetables, namedsourcedata, etc.)
        /// </summary>
        /// <param name="propertyList">Properties to add</param>
        private void GetRootFields(List<string> propertyList)
        {
            // Named source data
            propertyList.AddRange(Resolver.GetRegisteredDataNames());

            // Hidden fields in development mode
            if (SystemContext.DevelopmentMode)
            {
                // Add % suffix to pass the information about hidden members to MacroEditor.js so it can have different style
                propertyList.AddRange(Resolver.GetHiddenRegisteredDataNames().Select(x => x + "%"));
            }

            // Root extensions
            var rootProperties = typeof(IMacroRoot).GetStaticProperties<object>();
            if (rootProperties != null)
            {
                foreach (var prop in rootProperties.TypedValues)
                {
                    // Do not show invisible fields
                    if (!typeof(IMacroInvisible).IsAssignableFrom(prop.Type))
                    {
                        propertyList.Add(prop.Name);
                    }
                }
            }
        }


        /// <summary>
        /// Fills given list with properties of given object.
        /// </summary>
        /// <param name="propertyList">List where to add normal hints</param>
        /// <param name="dataProperties">List where to add data hints (data of the InfoCollections, etc.)</param>
        /// <param name="obj">Object the properties (data respectively) of which should be added to the list(s)</param>
        private void GetObjectFields(List<Hint> propertyList, List<string> dataProperties, object obj)
        {
            if (obj != null)
            {
                // Disable license error when macro evaluation tries to load objects that are not included in current license
                using (new CMSActionContext
                {
                    EmptyDataForInvalidLicense = true
                })
                {
                    if (obj is BaseInfo)
                    {
                        // Data container source
                        var info = (BaseInfo)obj;

                        List<string> props = info.TypeInfo.SensitiveColumns != null ? info.Properties.Except(info.TypeInfo.SensitiveColumns).ToList() : info.Properties;

                        propertyList.AddRange(props.Select(x => new Hint(x, GetObjectIconName(info.GetProperty(x)))));
                    }
                    else if (obj is DataRow)
                    {
                        // DataRow source, bind column names
                        DataRow dr = (DataRow)obj;
                        foreach (DataColumn col in dr.Table.Columns)
                        {
                            propertyList.Add(new Hint(col.ColumnName, GetObjectTypeIconName(col.DataType)));
                        }
                    }
                    else if (obj is DataRowView)
                    {
                        // DataRowView source, bind column names
                        DataRowView dr = (DataRowView)obj;
                        foreach (DataColumn col in dr.DataView.Table.Columns)
                        {
                            propertyList.Add(new Hint(col.ColumnName, GetObjectTypeIconName(col.DataType)));
                        }
                    }
                    else if (obj is IHierarchicalObject)
                    {
                        // Data container source
                        IHierarchicalObject hc = (IHierarchicalObject)obj;
                        propertyList.AddRange(hc.Properties.Select(x => new Hint(x, GetObjectIconName(hc.GetProperty(x)))));
                    }
                    else if (obj is IDataContainer)
                    {
                        // Data container source
                        IDataContainer dc = (IDataContainer)obj;
                        propertyList.AddRange(dc.ColumnNames.Select(x => new Hint(x, GetObjectIconName(dc.GetValue(x)))));
                    }

                    // Named enumerable objects
                    if (obj is INamedEnumerable)
                    {
                        if (dataProperties != null)
                        {
                            var namedCol = (INamedEnumerable)obj;
                            if (namedCol.ItemsHaveNames)
                            {
                                var enumerator = namedCol.GetNamedEnumerator<object>();
                                while (enumerator.MoveNext())
                                {
                                    // Named item
                                    string name = namedCol.GetObjectName(enumerator.Current);
                                    if (ValidationHelper.IsIdentifier(name))
                                    {
                                        dataProperties.Add(name);
                                    }
                                    else
                                    {
                                        dataProperties.Add("[\"" + name + "\"]");
                                    }
                                }
                            }
                        }
                    }

                    // Special case for TreeNode and PageInfo - append editable parts
                    if ((obj is TreeNode) || (obj is PageInfo))
                    {
                        EditableItems eItems;

                        if (obj is TreeNode)
                        {
                            // TreeNode editable fields
                            TreeNode node = (TreeNode)obj;
                            eItems = node.DocumentContent;
                        }
                        else
                        {
                            PageInfo pi = (PageInfo)obj;
                            eItems = pi.EditableItems;
                        }

                        // Editable regions
                        foreach (string item in eItems.EditableRegions.Keys)
                        {
                            propertyList.Add(new Hint(item, ICON_NAME_PROPERTY));
                        }

                        // Editable webparts
                        foreach (string item in eItems.EditableWebParts.Keys)
                        {
                            propertyList.Add(new Hint(item, ICON_NAME_PROPERTY));
                        }
                    }

                    // Extensions
                    var context = new EvaluationContext(Resolver, "");

                    var fields = MacroFieldContainer.GetFieldsForObject(obj);
                    if (fields != null)
                    {
                        propertyList.AddRange(fields.Select(x => new Hint(x.Name, GetObjectIconName(x.GetValue(context)))));
                    }
                }
            }
        }


        #region "Methods processing"

        /// <summary>
        /// Appends methods registered for given object.
        /// </summary>
        /// <param name="members">List where to add hints</param>
        /// <param name="obj">Object the methods of which should be added</param>
        private static void AddObjectMethods(List<Hint> members, object obj)
        {
            // Get the list of suitable methods for resulting type
            FillFromMethodList(members, MacroMethods.GetMethodsForObject(obj));

            // Add extension methods
            FillFromMethodList(members, MacroMethodContainer.GetMethodsForObject(obj));

            // Add simple methods
            if (obj != null)
            {
                var simpleMethods = obj.GetType().GetStaticProperties<IMacroMethod>();
                if ((simpleMethods != null) && (simpleMethods.TypedValues != null))
                {
                    FillFromMethodList(members, simpleMethods.TypedValues.Select(x => x.Value));
                }
            }
        }


        /// <summary>
        /// Appends methods registered for object specified in the default usings.
        /// </summary>
        /// <param name="members">List where to add hints</param>
        private void AddUsingsMethods(List<Hint> members)
        {
            if (NamespaceUsings != null)
            {
                foreach (string name in NamespaceUsings)
                {
                    var result = Resolver.ResolveMacroExpression(name, true, true);
                    if ((result != null) && (result.Result != null))
                    {
                        AddObjectMethods(members, result.Result);
                    }
                }
            }
        }


        /// <summary>
        /// Fills the members with strings formatted for AutoCompletion.
        /// </summary>
        /// <param name="members">List where to add hints</param>
        /// <param name="methodList">List of macro methods to add</param>
        private static void FillFromMethodList(List<Hint> members, IEnumerable<IMacroMethod> methodList)
        {
            foreach (IMacroMethod method in methodList)
            {
                if (!method.IsHidden)
                {
                    string m = method.Name.Trim().ToLowerCSafe();
                    if (!MacroElement.IsValidOperator(m) && !MacroElement.IsWordOperator(m))
                    {
                        if ((m != "logtodebug") || MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                        {
                            Hint hint;

                            if (!string.IsNullOrEmpty(method.Snippet))
                            {
                                // Snippet hint
                                hint = new Hint(method.Name, ICON_NAME_SNIPPET)
                                {
                                    Snippet = method.Snippet
                                };
                            }
                            else
                            {
                                // Method hint
                                hint = new Hint(method.Name, ICON_NAME_METHOD)
                                {
                                    Comment = GetMethodString(method, -1, false)
                                };
                            }

                            members.Add(hint);
                        }
                    }
                }
            }
        }

        #endregion


        #region "Local variables processing"

        /// <summary>
        /// Adds localy defined variables declared within the macro (in case of "myParam = 10; myPar", myPar whould be completed to myParam).
        /// </summary>
        /// <param name="members">List where to add the hints</param>
        /// <param name="previousLines">Previous lines where the declarations could be</param>
        /// <param name="currentLineVariables">Inline variables declared on the same line as the cursor is</param>
        private void AddLocalVariables(List<Hint> members, string previousLines, IEnumerable<string> currentLineVariables)
        {
            List<MacroElement> lexems = MacroElement.ParseExpression(previousLines, true);
            List<string> variables = new List<string>(currentLineVariables);

            for (int i = 0; i < lexems.Count; i++)
            {
                AddLocalVariable(variables, lexems, i);
            }

            // Add variables
            members.AddRange(variables.Select(x => new Hint(x, GetDataMemberIconName(x))));
        }


        /// <summary>
        /// Adds local variable if given MacroElement is a variable declaration.
        /// </summary>
        /// <param name="members">List where to add hints</param>
        /// <param name="lexems">List of lexems</param>
        /// <param name="i">Index of current lexem</param>
        private void AddLocalVariable(List<string> members, List<MacroElement> lexems, int i)
        {
            MacroElement el = lexems[i];
            if ((el.Type == ElementType.Operator) && (el.Expression == "=") && (i > 0))
            {
                // Add variable declaration to special fields
                MacroElement elPrev = lexems[i - 1];
                if (elPrev.Type == ElementType.Identifier)
                {
                    members.Add(elPrev.Expression);
                }
            }
        }

        #endregion


        #region "Get method context help"

        /// <summary>
        /// Returns context help (current method parameters).
        /// </summary>
        /// <param name="lexems">Lexems from lexical analysis</param>
        /// <param name="currentElementIndex">Index of the current lexem (according to the cursor position)</param>
        private string GetMethodContextHelp(List<MacroElement> lexems, int currentElementIndex)
        {
            string exprToCheck = "";
            string methodName = "";
            int paramNumber = 0;
            int brackets = 0;
            bool withoutFirstParam = false;
            for (int i = currentElementIndex; i >= 0; i--)
            {
                ElementType type = lexems[i].Type;

                if (brackets == 0)
                {
                    // Count number of commas before current element (it's the current number of parameter)
                    if (type == ElementType.Comma)
                    {
                        paramNumber++;
                    }

                    // We need to take context to the left side of current lexem
                    // It needs to preserve the structure (deepnes withing the brackets)
                    if (type == ElementType.LeftBracket)
                    {
                        if ((i > 0) && (lexems[i - 1].Type == ElementType.Identifier))
                        {
                            methodName = lexems[i - 1].Expression;
                            if (i > 2)
                            {
                                exprToCheck = lexems[i - 3].Expression;
                                for (int j = i - 4; j >= 0; j--)
                                {
                                    if ((lexems[j].Type == ElementType.Dot) || (lexems[j].Type == ElementType.Identifier))
                                    {
                                        exprToCheck = lexems[j].Expression + exprToCheck;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                // Disable license error when macro evaluation tries to load objects that are not included in current license
                                using (new CMSActionContext
                                {
                                    EmptyDataForInvalidLicense = true
                                })
                                {
                                    bool isNamespaceCall = false;
                                    if (lexems[i - 3].Type == ElementType.Identifier)
                                    {
                                        isNamespaceCall = Resolver.CheckDataSources(exprToCheck, new EvaluationContext(Resolver, exprToCheck)).Result is IMacroNamespace;
                                    }

                                    withoutFirstParam = (lexems[i - 2].Type == ElementType.Dot) && !isNamespaceCall;
                                }
                            }
                        }
                        break;
                    }
                    else if (type == ElementType.LeftIndexer)
                    {
                        return "Indexer: Type number or name of the element.";
                    }
                }

                // Ensure correct deepnes within the brackets
                if ((type == ElementType.RightBracket) || (type == ElementType.RightIndexer))
                {
                    brackets++;
                }
                else if ((type == ElementType.LeftBracket) || (type == ElementType.LeftIndexer))
                {
                    brackets--;
                }
            }

            IMacroMethod method = null;
            if (!string.IsNullOrEmpty(exprToCheck))
            {
                // Disable security check and resolve the currentText
                Resolver.Settings.IdentityOption = MacroIdentityOption.FromUserInfo(MembershipContext.AuthenticatedUser);
                Resolver.Settings.CheckIntegrity = false;
                Resolver.Settings.EncapsulateMacroObjects = false;

                var settings = new ResolveExpressionSettings(exprToCheck)
                {
                    KeepObjectsAsResult = true,
                    SupressParsingError = true
                };

                EvaluationResult result = Resolver.ResolveMacroExpression(settings);
                if (result != null)
                {
                    if (result.Result != null)
                    {
                        method = MacroMethodContainer.GetMethodForObject(result.Result, methodName);
                        if (method == null)
                        {
                            // Try to get simple method if not found within a container
                            var prop = result.Result.GetType().StaticProperty<IMacroMethod>(methodName);
                            if (prop != null)
                            {
                                method = prop.Value;
                            }
                        }
                    }
                }
            }

            if (method == null)
            {
                // Try to find in the backward compatibility storage
                method = MacroMethods.GetMethod(methodName);
            }

            if (method != null)
            {
                return GetMethodString(method, paramNumber, withoutFirstParam);
            }

            // Method not known or there is syntax error in the expression
            return "";
        }


        /// <summary>
        /// Returns method help (return type, name and parameters), highlites current parameter.
        /// </summary>
        /// <param name="method">Method object</param>
        /// <param name="paramNumber">Number of the parameter which should be highlighted</param>
        /// <param name="withoutFirstParam">If true, first parameter is not rendered</param>
        private static string GetMethodString(IMacroMethod method, int paramNumber, bool withoutFirstParam)
        {
            string currentParamComment = "";
            string currentParamName = "";
            string parameters = "";

            int startParam = (withoutFirstParam ? 1 : 0);
            if (method.Parameters != null)
            {
                for (int i = startParam; i < method.Parameters.Count; i++)
                {
                    IMacroMethodParam p = method.Parameters[i];
                    string paramtype;
                    if (p.IsParams)
                    {
                        paramtype = "params " + p.Type.Name + "[]";
                    }
                    else
                    {
                        paramtype = p.Type.Name;
                    }
                    string param = paramtype + " " + p.Name;
                    if (i >= method.MinimumParameters)
                    {
                        param = "<i>" + param + "</i>";
                    }
                    int index = i - startParam;
                    if ((index == paramNumber) || (p.IsParams && (index <= paramNumber)))
                    {
                        currentParamName = p.Name;
                        currentParamComment = p.Comment;
                        param = "<strong>" + param + "</strong>";
                    }
                    parameters += ", " + param;
                }
            }
            if (parameters != "")
            {
                parameters = parameters.Substring(2);
            }

            string type = (method.Type == null ? "object" : method.Type.Name);
            string methodComment = (string.IsNullOrEmpty(method.Comment) ? "" : "<strong>" + method.Comment + "</strong><br/><br />");
            string paramComment = (string.IsNullOrEmpty(currentParamComment) ? "" : "<br/><br/><strong>" + currentParamName + ":&nbsp;</strong>" + currentParamComment);

            return methodComment + type + " " + method.Name + "(" + parameters + ")" + paramComment;
        }

        #endregion


        #region "Get type icon methods"

        /// <summary>
        /// Returns icon of given data member according to its type.
        /// </summary>
        /// <param name="name">Name of the datamember</param>
        private string GetDataMemberIconName(string name)
        {
            Type type = typeof(object);
            if (!string.IsNullOrEmpty(name))
            {
                // Disable license error when macro evaluation tries to load objects that are not included in current license
                using (new CMSActionContext
                {
                    EmptyDataForInvalidLicense = true
                })
                {
                    EvaluationResult result = Resolver.CheckDataSources(name, new EvaluationContext(Resolver, name));
                    if ((result != null) && (result.Result != null))
                    {
                        if ((result.Result is TreeNode) || (result.Result is IInfo) || (result.Result is IInfoObjectCollection))
                        {
                            return ICON_NAME_DEFAULT;
                        }
                        type = result.Result.GetType();
                    }
                }
            }
            return GetObjectTypeIconName(type);
        }


        /// <summary>
        /// Tries to find the path of the given type (traverses the whole type hierarchy). Return path if it exists, otherewise returns null.
        /// </summary>
        /// <param name="type">Type to check</param>
        private static string GetObjectTypeIconName(Type type)
        {
            if (mTypeIcons.ContainsKey(type))
            {
                return mTypeIcons[type];
            }

            Type currentType = type;
            while ((currentType != null) && (currentType != typeof(object)))
            {
                string name = GetIconName(currentType);
                if (name != null)
                {
                    return name;
                }

                // Check the interfaces
                var interfaces = currentType.GetInterfaces();
                foreach (var i in interfaces)
                {
                    name = GetIconName(i);
                    if (name != null)
                    {
                        mTypeIcons[type] = name;
                        return name;
                    }
                }

                // Check the base type
                currentType = currentType.BaseType;
            }

            mTypeIcons[type] = ICON_NAME_PROPERTY;
            return ICON_NAME_PROPERTY;
        }


        /// <summary>
        /// Returns the name of the type of the object which can be transformed to icon path.
        /// </summary>
        /// <param name="obj">Object the type of which to get the name of</param>
        private static string GetObjectIconName(object obj)
        {
            if (obj == null)
            {
                return ICON_NAME_PROPERTY;
            }

            return GetObjectTypeIconName(obj.GetType());
        }


        /// <summary>
        /// Returns the name of the type which can be transformed to icon path.
        /// </summary>
        /// <param name="type">Type to get the name of</param>
        private static string GetTypeName(Type type)
        {
            string name = type.Name;

            // Remove generic type
            int index = name.IndexOf('<');
            if (index >= 0)
            {
                name = name.Substring(0, index);
            }

            return name.ToLowerCSafe();
        }


        /// <summary>
        /// Tries to find icon name in predefined icon name collection.
        /// Returns the icon name if found. Null otherwise.
        /// </summary>
        /// <param name="type">Type.</param>
        private static string GetIconName(Type type)
        {
            var iconName = "icon-me-" + GetTypeName(type);
            if (iconCssClasses.Contains(iconName))
            {
                return iconName;
            }

            return null;
        }


        private static List<Hint> GetSnippets()
        {
            var list = new List<Hint> {
                new Hint("for", ICON_NAME_SNIPPET) { Snippet = "for (i = 0; i < 10; i++) { | }" },
                new Hint("foreach", ICON_NAME_SNIPPET) { Snippet = "foreach (x in collection) { | }" },
                new Hint("if", ICON_NAME_SNIPPET) { Snippet = "if (|) {  }" },
                new Hint("while", ICON_NAME_SNIPPET) { Snippet = "while( | ) {  }" },
                new Hint("break", ICON_NAME_SNIPPET) { Snippet = "break;" },
                new Hint("continue", ICON_NAME_SNIPPET) { Snippet = "continue;" },
                new Hint("return", ICON_NAME_SNIPPET) { Snippet = "return;" },
                new Hint("print", ICON_NAME_SNIPPET) { Snippet = "print(|," },
                new Hint("println", ICON_NAME_SNIPPET) { Snippet = "println(|," },
                new Hint("null", ICON_NAME_SNIPPET) { Snippet = "null" },
            };

            return list;
        }

        #endregion


        #endregion


        #region ICallbackEventHandler Members

        /// <summary>
        /// Gets the callback result
        /// </summary>
        public string GetCallbackResult()
        {
            if (ASCXMode)
            {
                if (isShowContext)
                {
                    return "";
                }

                return AutoCompleteASCX(text, position);
            }

            if (position == 0)
            {
                return AutoComplete(text, text.Length, isShowContext, wholeText);
            }

            return AutoComplete(text, position, isShowContext, wholeText);
        }


        /// <summary>
        /// Processes the callback
        /// </summary>
        /// <param name="eventArgument">Event argument</param>
        public void RaiseCallbackEvent(string eventArgument)
        {
            string[] args = eventArgument.Split(new[]
            {
                '\n'
            });
            if (args.Length == 5)
            {
                isShowContext = (args[3].ToLowerCSafe() == "context");
                wholeText = args[4];
                position = ValidationHelper.GetInteger(args[2], 0);
                if (args[1] == "")
                {
                    text = args[0];
                }
                else
                {
                    // Normal character - needs to be placed to correct pos
                    text = args[0].Substring(0, position) + args[1] + args[0].Substring(position);
                    position++;
                }
            }
        }

        #endregion
    }
}
