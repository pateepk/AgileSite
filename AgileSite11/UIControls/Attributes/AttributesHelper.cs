using System;
using System.Collections.Generic;
using System.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Class attributes helper.
    /// </summary>
    public class AttributesHelper
    {
        /// <summary>
        /// Processes the page attributes.
        /// </summary>
        /// <param name="sender">Object on which the attributes should be processed</param>
        /// <param name="postProcessList">List to which the attributes to be postprocessed should be put</param>
        public static void ProcessAttributes(object sender, ref List<ICMSAttribute> postProcessList)
        {
            // Check if the page contains some attributes
            object[] attributes = sender.GetType().GetCustomAttributes(true);
            if (attributes.Length > 0)
            {
                if (postProcessList == null)
                {
                    postProcessList = new List<ICMSAttribute>();
                }

                // Process the attributes in the correct order
                ProcessAttributes(sender, typeof(EditedObjectAttribute), postProcessList);
                ProcessAttributes(sender, typeof(UIElementAttribute), postProcessList);
                ProcessAttributes(sender, typeof(CheckLicenceAttribute), postProcessList);
                ProcessAttributes(sender, typeof(SecurityAttribute), postProcessList);
                ProcessAttributes(sender, typeof(HashValidationAttribute), postProcessList);

                ProcessAttributes(sender, typeof(ParentObjectAttribute), postProcessList);
               
                if (!ProcessAttributes(sender, typeof(TitleAttribute), postProcessList))
                {
                    ProcessAttributes(sender, typeof(HelpAttribute), postProcessList);
                }

                // Process breadcrumbs
                ProcessAttributes(sender, typeof(BreadcrumbsAttribute), postProcessList);
                ProcessAttributes(sender, typeof(BreadcrumbAttribute), postProcessList);

                // Process tabs
                if (ProcessAttributes(sender, typeof(TabsAttribute), postProcessList))
                {
                    ProcessAttributes(sender, typeof(TabAttribute), postProcessList);
                }

                // Process header actions
                ProcessAttributes(sender, typeof(ActionAttribute), postProcessList);

                ProcessAttributes(sender, typeof(ScriptAttribute), postProcessList);
            }
        }


        /// <summary>
        /// Processes the attributes of the specific type.
        /// </summary>
        /// <param name="sender">Object on which the attributes should be processed</param>
        /// <param name="attributeType">Attribute type</param>
        /// <param name="postProcessList">List to which the attributes to be postprocessed should be put</param>
        private static bool ProcessAttributes(object sender, Type attributeType, IList<ICMSAttribute> postProcessList)
        {
            // Get the attributes
            Type type = sender.GetType();
            object[] attributes = type.GetCustomAttributes(attributeType, true);

            bool found = ((attributes != null) && (attributes.Length > 0));
            if (found)
            {
                // Transform sender to the page
                if (sender is Control)
                {
                    sender = ((Control)sender).Page;
                }

                // Process all attributes
                foreach (ICMSAttribute attribute in attributes)
                {
                    if (attribute is ICMSFunctionalAttribute)
                    {
                        // Apply functional ones
                        ((ICMSFunctionalAttribute)attribute).Apply(sender);
                    }
                    else if (attribute is ICMSSecurityAttribute)
                    {
                        var page = GetCMSPage(attribute, sender);
                        // Raise check on security ones
                        ((ICMSSecurityAttribute)attribute).Check(page);
                    }

                    // If the attribute contains macro, add to the post process list
                    if ((postProcessList != null) && attribute.ContainsMacro)
                    {
                        postProcessList.Add(attribute);
                    }
                }
            }

            return found;
        }


        /// <summary>
        /// Processes the attributes of the specific type.
        /// </summary>
        /// <param name="sender">Object on which the attributes should be processed</param>
        /// <param name="attributes">Collection of attributes to process</param>
        public static void PostProcessAttributes(object sender, IEnumerable<ICMSAttribute> attributes)
        {
            // Transform sender to the page
            if (sender is Control)
            {
                sender = ((Control)sender).Page;
            }

            if (attributes == null)
            {
                return;
            }

            // Process all attributes
            foreach (ICMSAttribute attribute in attributes)
            {
                if (attribute is ICMSFunctionalAttribute)
                {
                    // Apply functional ones
                    ((ICMSFunctionalAttribute)attribute).Apply(sender);
                }
                else if (attribute is ICMSSecurityAttribute)
                {
                    var page = GetCMSPage(attribute, sender);
                    // Raise check on security ones
                    ((ICMSSecurityAttribute)attribute).Check(page);
                }
            }
        }


        /// <summary>
        /// Tries to get <see cref="CMSPage"/> typed object from <paramref name="sender"/> object parameter.
        /// </summary>
        /// <exception cref="NotSupportedException">Throws when <see cref="CMSPage"/> object cannot be retrieved</exception>
        /// <param name="attribute">Security attribute</param>
        /// <param name="sender">Object from which was attribute initialized</param>
        private static CMSPage GetCMSPage(ICMSAttribute attribute, object sender)
        {
            CMSPage page = sender as CMSPage;
            if (page == null)
            {
                throw new NotSupportedException(String.Format("'{0}' works only with pages inherited from '{1}'", attribute.GetType().FullName, typeof(CMSPage).FullName));
            }

            return page;
        }

    }
}