using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

using CMS.IO;
using CMS.Base;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Extended listbox.
    /// </summary>
    [ToolboxData("<{0}:ExtendedListBox runat=server></{0}:ExtendedListBox>")]
    public class ExtendedListBox : CMSListBox
    {
        #region "Variables"

        private string txtControl = null;
        private string mSectionSeparator = "";
        private int mScrollPadding = 2;

        #endregion


        #region "Properties"

        /// <summary>
        /// TextBox control.
        /// </summary>
        [DefaultValue(""), TypeConverter(typeof(ControlIDConverter))]
        public string TextBoxControl
        {
            get
            {
                return txtControl;
            }
            set
            {
                txtControl = value;
            }
        }


        /// <summary>
        /// Indent separator in selected text.
        /// </summary>
        public string SectionSeparator
        {
            get
            {
                return mSectionSeparator;
            }
            set
            {
                mSectionSeparator = value;
            }
        }


        /// <summary>
        /// Scroll padding.
        /// </summary>
        public int ScrollPadding
        {
            get
            {
                return mScrollPadding;
            }
            set
            {
                mScrollPadding = value;
            }
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// Render.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                writer.Write("[CMSDocumentValue: " + ID + "]");
                return;
            }

            SectionProgress();

            base.Render(writer);
        }

        #endregion


        #region "Other methods"

        /// <summary>
        /// Fill this listbox with section from controlled Extended text area.
        /// </summary>
        protected void SectionProgress()
        {
            // Generate space for list box
            StringWriter writer = new StringWriter();
            string pad = "&nbsp;&nbsp;&nbsp;";
            HttpUtility.HtmlDecode(pad, writer);
            pad = writer.ToString();

            //Try find ExtendedTextBox
            Control ctrl = FindControl(TextBoxControl);
            if (ctrl != null)
            {
                // get control
                ExtendedTextArea txt = ctrl as ExtendedTextArea;
                if (txt != null)
                {
                    // Check if exists some macros
                    if (txt.Count > 0)
                    {
                        // Cerate new sorted list
                        SortedList sorted = new SortedList();
                        for (int i = 0; i < txt.Count; i++)
                        {
                            // Create list item
                            ListItem li = new ListItem();
                            li.Text = txt.Names[i].ToString();
                            li.Value = txt.Lines[i].ToString();

                            // Surted list must contains unique key
                            // so if key allready exist add some char to the end
                            char schar = 'Z';
                            if (sorted.ContainsKey(li.Text))
                            {
                                while (sorted.ContainsKey(li.Text + schar.ToString()))
                                {
                                    schar--;
                                }

                                // Add key and list item to the sorted list array
                                sorted.Add(li.Text + schar.ToString(), li);
                            }
                            else
                            {
                                // Add key and list item to the sorted list array
                                sorted.Add(li.Text, li);
                            }
                        }

                        // Get items from sorted list and add them to the ListBox
                        for (int i = 0; i < txt.Count; i++)
                        {
                            ListItem li = (ListItem)sorted.GetValueList()[i];

                            // Add padding by separator
                            if (li.Text.Contains(SectionSeparator))
                            {
                                string padding = "";
                                string[] occurrence = li.Text.Split(SectionSeparator[0]);

                                for (int j = 0; j < 3 * (occurrence.Length - 1); j++)
                                {
                                    padding += pad;
                                }

                                // Set list item text with padding
                                li.Text = padding + li.Text.Remove(0, li.Text.LastIndexOfCSafe(SectionSeparator) + 1);
                            }

                            // Add item to the list
                            Items.Add(li);
                        }

                        // Add javascript action "onclick" to the items
                        Attributes.Add("onclick", "document.getElementById('" + ctrl.ClientID + "').scrollTop = ((this.value - 1)*(14-" + mScrollPadding.ToString() + "))+((this.value-1)*" + mScrollPadding.ToString() + "); ");
                    }
                }
            }
        }

        #endregion
    }
}