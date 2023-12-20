using System;
using System.Collections.Generic;
using System.Xml;

using CMS.Membership;


namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Class represents clipboard item for web part/widgets
    /// </summary>
    [Serializable]
    internal class WebPartClipBoardItem
    {
        #region "Properties"

        /// <summary>
        /// User ID
        /// </summary>
        private int UserID
        {
            get;
            set;
        }


        /// <summary>
        /// String xml definition
        /// </summary>
        private string XMLDefinition
        {
            get;
            set;
        }


        /// <summary>
        /// Zone type
        /// </summary>
        public WidgetZoneTypeEnum ZoneType
        {
            get;
            private set;
        }


        /// <summary>
        /// Current template scope
        /// </summary>
        public PageTemplateTypeEnum TemplateScope
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates whether clipboard item is valid for current user
        /// </summary>
        public bool IsValid
        {
            get
            {
                return (MembershipContext.AuthenticatedUser.UserID == UserID);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the Web part instance objects
        /// </summary>
        public IEnumerable<WebPartInstance> GetItems()
        {
            // Create XML document
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(XMLDefinition);

            // loop thru saved zones
            foreach (XmlNode node in doc.FirstChild.ChildNodes)
            {
                if (node.Name == "webpart")
                {
                    yield return new WebPartInstance(node);
                }
            }
            yield break;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates clipboard item
        /// </summary>
        /// <param name="zoneType">Zone type</param>
        /// <param name="templateScope">Template scope</param>
        private WebPartClipBoardItem(WidgetZoneTypeEnum zoneType, PageTemplateTypeEnum templateScope)
        {
            UserID = MembershipContext.AuthenticatedUser.UserID;
            ZoneType = zoneType;
            TemplateScope = WebPartClipBoardManager.GetTemplateScopeType(templateScope);
        }


        /// <summary>
        /// Creates clipboard item for single web part
        /// </summary>
        /// <param name="wpi">Web part instance</param>
        /// <param name="templateInstance">Template instance</param>
        public WebPartClipBoardItem(WebPartInstance wpi, PageTemplateInstance templateInstance)
            : this(wpi.ParentZone.WidgetZoneType, templateInstance.ParentPageTemplate != null ? templateInstance.ParentPageTemplate.PageTemplateType : PageTemplateTypeEnum.Unknown)
        {
            XMLDefinition = "<c>" + wpi.GetXmlNode().OuterXml + "</c>";
        }



        /// <summary>
        /// Creates clipboard item for zone web parts
        /// </summary>
        /// <param name="wpzi">Web part zone instance</param>
        /// <param name="templateInstance">Template instance</param>
        public WebPartClipBoardItem(WebPartZoneInstance wpzi, PageTemplateInstance templateInstance)
            : this(wpzi.WidgetZoneType, templateInstance.ParentPageTemplate != null ? templateInstance.ParentPageTemplate.PageTemplateType : PageTemplateTypeEnum.Unknown)
        {
            XMLDefinition = "<c>" + wpzi.GetXmlNode().InnerXml + "</c>";
        }

        #endregion
    }
}
