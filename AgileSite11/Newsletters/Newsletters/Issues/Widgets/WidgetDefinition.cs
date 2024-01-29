using System;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.FormEngine;

namespace CMS.Newsletters.Issues.Widgets
{
    /// <summary>
    /// Provides access to <see cref="EmailWidgetInfo"/> properties.
    /// </summary>
    internal sealed class WidgetDefinition
    {
        private readonly EmailWidgetInfo definition;
        private Lazy<FormInfo> mFormInfo;
        private FormInfo FormInfo => mFormInfo.Value;


        /// <summary>
        /// Gets widget definition code.
        /// </summary>
        /// <remarks>This property is <c>null</c> when email widget definition doesn't exists.</remarks>
        public string Code => definition?.EmailWidgetCode;


        /// <summary>
        /// Gets widget definition fields.
        /// </summary>
        public IEnumerable<FormFieldInfo> Fields => FormInfo.GetFields(true, true);


        /// <summary>
        /// Indicates that widget definition (<see cref="EmailWidgetInfo"/>) was not found.
        /// </summary>
        public bool WidgetDefinitionNotFound;


        /// <summary>
        /// Creates an instance of <see cref="WidgetDefinition"/> class.
        /// </summary>
        /// <param name="definition">Widget definition instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
        public WidgetDefinition(EmailWidgetInfo definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            this.definition = definition;
            InitializeFormInfo();
        }


        /// <summary>
        /// Creates an instance of <see cref="WidgetDefinition"/> class.
        /// </summary>
        /// <param name="widgetTypeIdentifier">Widget definition identifier.</param>
        /// <param name="siteIdentifier">Site identifier of the widget definition.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="siteIdentifier"/> is null.</exception>
        public WidgetDefinition(Guid widgetTypeIdentifier, SiteInfoIdentifier siteIdentifier)
        {
            if (siteIdentifier == null)
            {
                throw new ArgumentNullException(nameof(siteIdentifier));
            }

            definition = EmailWidgetInfoProvider.GetEmailWidgetInfo(widgetTypeIdentifier, siteIdentifier);
            if(definition == null)
            {
                WidgetDefinitionNotFound = true;
            }

            InitializeFormInfo();
        }


        private void InitializeFormInfo()
        {
            mFormInfo = new Lazy<FormInfo>(() => new FormInfo(definition?.EmailWidgetProperties));
        }
    }
}
