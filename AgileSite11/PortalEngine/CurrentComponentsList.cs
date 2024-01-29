using System;
using System.Collections.Generic;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Current components used on the page
    /// </summary>
    public class CurrentComponentsList
    {
        #region "Properties"

        /// <summary>
        /// Web part containers used by current page. Container name is used as a key.
        /// </summary>
        public Dictionary<string, WebPartContainerInfo> Containers
        {
            get;
            protected set;
        }


        /// <summary>
        /// Transformations used by current page. Full name of the transformation is used as a key.
        /// </summary>
        public Dictionary<string, TransformationInfo> Transformations
        {
            get;
            protected set;
        }


        /// <summary>
        /// Web part layouts used by current page. Full name of the web part layout is used as a key.
        /// </summary>
        public Dictionary<string, WebPartLayoutInfo> WebPartLayouts
        {
            get;
            protected set;
        }


        /// <summary>
        /// Web parts used by current page. Web part name is used as a key.
        /// </summary>
        public Dictionary<string, WebPartInfo> WebParts
        {
            get;
            protected set;
        }


        /// <summary>
        /// Layouts used by current page. Layout name is used as a key.
        /// </summary>
        public Dictionary<string, LayoutInfo> Layouts
        {
            get;
            protected set;
        }


        /// <summary>
        /// Page templates used by current page. Page template name is used as a key.
        /// </summary>
        public Dictionary<string, PageTemplateInfo> Templates
        {
            get;
            protected set;
        }


        /// <summary>
        /// Device layouts used by current page. Full name of the device layout is used as a key.
        /// </summary>
        public Dictionary<string, PageTemplateDeviceLayoutInfo> DeviceLayouts
        {
            get;
            protected set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Registers the web part container within the collection of containers for current request.
        /// </summary>
        /// <param name="container">Container object to register within current request</param>
        public void RegisterWebPartContainer(WebPartContainerInfo container)
        {
            if (container == null)
            {
                return;
            }

            if (Containers == null)
            {
                Containers = new Dictionary<string, WebPartContainerInfo>(StringComparer.InvariantCultureIgnoreCase);
            }

            Containers[container.ContainerName] = container;
        }


        /// <summary>
        /// Registers the transformation within the collection of transformations for current request.
        /// </summary>
        /// <param name="transformation">Transformation object to register within current request</param>
        public void RegisterTransformation(TransformationInfo transformation)
        {
            if (transformation == null)
            {
                return;
            }

            if (Transformations == null)
            {
                Transformations = new Dictionary<string, TransformationInfo>(StringComparer.InvariantCultureIgnoreCase);
            }

            Transformations[transformation.TransformationFullName] = transformation;
        }


        /// <summary>
        /// Registers the web part layout within the collection of web part layouts for current request.
        /// </summary>
        /// <param name="webPartLayout">Web part layout object to register within current request</param>
        public void RegisterWebPartLayout(WebPartLayoutInfo webPartLayout)
        {
            if (webPartLayout == null)
            {
                return;
            }

            if (WebPartLayouts == null)
            {
                WebPartLayouts = new Dictionary<string, WebPartLayoutInfo>(StringComparer.InvariantCultureIgnoreCase);
            }

            WebPartLayouts[webPartLayout.WebPartLayoutFullName] = webPartLayout;
        }


        /// <summary>
        /// Registers the web part  within the collection of web part s for current request.
        /// </summary>
        /// <param name="webPart">Web part  object to register within current request</param>
        public void RegisterWebPart(WebPartInfo webPart)
        {
            if (webPart == null)
            {
                return;
            }

            if (WebParts == null)
            {
                WebParts = new Dictionary<string, WebPartInfo>(StringComparer.InvariantCultureIgnoreCase);
            }

            WebParts[webPart.WebPartName] = webPart;
        }


        /// <summary>
        /// Registers the layout within the collection of layouts for current request.
        /// </summary>
        /// <param name="layout">Layout object to register within current request</param>
        public void RegisterLayout(LayoutInfo layout)
        {
            if (layout == null)
            {
                return;
            }

            if (Layouts == null)
            {
                Layouts = new Dictionary<string, LayoutInfo>(StringComparer.InvariantCultureIgnoreCase);
            }

            Layouts[layout.LayoutCodeName] = layout;
        }


        /// <summary>
        /// Registers the page template within the collection of page templates for current request.
        /// </summary>
        /// <param name="pageTemplate">Page template object to register within current request</param>
        public void RegisterPageTemplate(PageTemplateInfo pageTemplate)
        {
            if (pageTemplate == null)
            {
                return;
            }

            if (Templates == null)
            {
                Templates = new Dictionary<string, PageTemplateInfo>(StringComparer.InvariantCultureIgnoreCase);
            }

            Templates[pageTemplate.CodeName] = pageTemplate;
        }


        /// <summary>
        /// Registers the device layout within the collection of device layouts for current request.
        /// </summary>
        /// <param name="layout">Device layout object to register within current request</param>
        public void RegisterDeviceLayout(PageTemplateDeviceLayoutInfo layout)
        {
            if (layout == null)
            {
                return;
            }

            if (DeviceLayouts == null)
            {
                DeviceLayouts = new Dictionary<string, PageTemplateDeviceLayoutInfo>(StringComparer.Ordinal);
            }

            DeviceLayouts[layout.LayoutFullName] = layout;
        }

        #endregion
    }
}