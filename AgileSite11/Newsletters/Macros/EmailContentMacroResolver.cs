using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Newsletters.Issues.Widgets.Configuration;

namespace CMS.Newsletters
{
    /// <summary>
    /// Resolves macros for email content.
    /// </summary>
    public sealed class EmailContentMacroResolver : IEmailContentMacroResolver
    {
        private readonly ISubscriberEmailRetriever subscriberEmailRetriever;
        private readonly EmailContentMacroResolverSettings settings;
        private MacroResolver macroResolver;


        /// <summary>
        /// Creates an instance of <see cref="EmailContentMacroResolver"/> class.
        /// </summary>
        /// <param name="settings">Settings for the resolver.</param>
        /// <param name="subscriberEmailRetriever">Service that retrieves email for all types of subscribers. When not set registered implementation is used.</param>
        public EmailContentMacroResolver(EmailContentMacroResolverSettings settings, ISubscriberEmailRetriever subscriberEmailRetriever = null)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this.settings = settings;
            this.subscriberEmailRetriever = subscriberEmailRetriever ?? Service.Resolve<ISubscriberEmailRetriever>();
            macroResolver = GetResolver();
        }


        /// <summary>
        /// Resolves the dynamic field macros, replaces the {%dynamicfieldname%} macros with the dynamic field values.
        /// </summary>
        /// <param name="text">Text containing dynamic fields to resolve.</param>
        public string Resolve(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            using (var h = NewsletterEvents.ResolveMacros.StartEvent(macroResolver, text, settings.Newsletter, settings.Issue))
            {
                var e = h.EventArguments;
                if (h.CanContinue())
                {
                    // Resolve macros from text edited in event
                    e.TextToResolve = macroResolver.ResolveMacros(e.TextToResolve);
                }

                h.FinishEvent();

                return e.TextToResolve;
            }
        }


        /// <summary>
        /// Creates child instance of current resolver with data context of given <paramref name="widget"/> configuration properties.
        /// </summary>
        /// <param name="widget">Widget configuration.</param>
        public IEmailContentMacroResolver GetWidgetContentMacroResolver(Widget widget)
        {
            var child = new EmailContentMacroResolver(settings, subscriberEmailRetriever);
            child.macroResolver = macroResolver.CreateChild();

            if (widget == null)
            {
                return child;
            }

            RegisterProperties(child, widget);

            return child;
        }


        private void RegisterProperties(EmailContentMacroResolver child, Widget widget)
        {
            var formInfo = GetFormInfo(widget);

            foreach (var field in formInfo.GetFields(true, true))
            {
                var property = widget.Properties.FirstOrDefault(prop => prop.Name.Equals(field.Name, StringComparison.InvariantCultureIgnoreCase));
                var value = GetTypedValue(property?.Value, field.DataType);
                child.macroResolver.SetNamedSourceData(field.Name, value);
            }
        }


        private static object GetTypedValue(string value, string dataType)
        {
            var type = DataTypeManager.GetDataType(TypeEnum.Field, dataType);
            return type.Convert(value, CultureHelper.EnglishCulture, type.ObjectDefaultValue);
        }


        private FormInfo GetFormInfo(Widget widget)
        {
            FormInfo formInfo = null;

            // Try to get data from cache
            using (var cs = new CachedSection<FormInfo>(ref formInfo, 60, true, null, "emailwidgetforminfo", widget.TypeIdentifier, settings.Site.ObjectCodeName))
            {
                if (cs.LoadData)
                {
                    var widgetDef = EmailWidgetInfoProvider.GetEmailWidgetInfo(widget.TypeIdentifier, settings.Site);
                    formInfo = new FormInfo(widgetDef.EmailWidgetProperties);

                    // Save to the cache
                    if (cs.Cached)
                    {
                        // Get dependencies
                        var dependencies = new List<string>
                        {
                            $"{EmailWidgetInfo.OBJECT_TYPE}|byguid|{widget.TypeIdentifier}"
                        };

                        // Set dependencies
                        cs.CacheDependency = CacheHelper.GetCacheDependency(dependencies);
                    }

                    cs.Data = formInfo;
                }
            }

            return formInfo;
        }


        /// <summary>
        /// Gets initialized macro resolver.
        /// </summary>
        internal MacroResolver GetResolver()
        {
            var resolver = MacroResolver.GetInstance();

            SetSettings(resolver);
            SetName(resolver);
            SetCulture(resolver);
            var subscriber = GetSubscriber();
            SetNamedData(resolver, subscriber);

            return resolver;
        }


        private void SetSettings(MacroResolver resolver)
        {
            resolver.Settings.KeepUnresolvedMacros = settings.KeepUnresolvedMacros;
            resolver.Settings.DisableContextMacros = settings.DisableContextMacros;
        }


        private void SetName(MacroResolver resolver)
        {
            if (settings.Name != null)
            {
                resolver.ResolverName = settings.Name;
            }
        }


        private void SetNamedData(MacroResolver resolver, SubscriberInfo subscriber)
        {
            var urlService = Service.Resolve<IIssueUrlService>();

            if (subscriber != null)
            {

                resolver.SetNamedSourceData(NewsletterMacroConstants.Recipient, new Recipient(subscriber));

                resolver.SetNamedSourceData(NewsletterMacroConstants.EmailFeed, new EmailFeedwithSubscriber(urlService, settings.Newsletter, settings.Issue, settings.Subscription, subscriber));
                resolver.SetNamedSourceData(NewsletterMacroConstants.Email, new EmailWithSubscriber(urlService, settings.Newsletter, settings.Issue, subscriber));

                resolver.SetHiddenNamedSourceData(NewsletterMacroConstants.Advanced, new AdvancedWithSubscriber(settings.Newsletter, settings.Issue, subscriber));
            }
            else
            {
                resolver.SetNamedSourceData(NewsletterMacroConstants.EmailFeed, new EmailFeed(settings.Newsletter));
                resolver.SetNamedSourceData(NewsletterMacroConstants.Email, new Email(settings.Newsletter, settings.Issue));

                resolver.SetHiddenNamedSourceData(NewsletterMacroConstants.Advanced, new Advanced(settings.Newsletter, settings.Issue));
            }
        }


        private SubscriberInfo GetSubscriber()
        {
            var subscriber = settings.Subscriber;
            if (subscriber == null && (settings.IsPreview || settings.UseFakeData))
            {
                return Service.Resolve<IFakeSubscriberService>().GetFakeSubscriber();
            }

            if (subscriber != null && string.IsNullOrEmpty(subscriber.SubscriberEmail))
            {
                subscriber.SubscriberEmail = subscriberEmailRetriever.GetSubscriberEmail(subscriber.SubscriberID);
            }

            return subscriber;
        }

        private void SetCulture(MacroResolver resolver)
        {
            resolver.Culture = CultureHelper.GetDefaultCultureCode(settings.Site);
        }
    }
}