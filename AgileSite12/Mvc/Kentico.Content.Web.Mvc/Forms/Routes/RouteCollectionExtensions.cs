using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

using CMS.Core;

using Kentico.Builder.Web.Mvc;
using Kentico.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Extends a <see cref="RouteCollection"/> object by Form builder route mapping functionality.
    /// </summary>
    internal static class RouteCollectionExtensions
    {
        internal static Func<IFormComponentDefinitionProvider> FormComponentDefinitionProviderResolver
        {
            get;
            set;
        } = () => Service.Resolve<IFormComponentDefinitionProvider>();


        internal static Func<ISectionDefinitionProvider> SectionDefinitionProviderResolver
        {
            get;
            set;
        } = () => Service.Resolve<ISectionDefinitionProvider>();


        /// <summary>
        /// Adds routes for Form builder system services.
        /// </summary>
        /// <param name="extensionPoint">Extension point providing <see cref="RouteCollection"/> as its target.</param>
        /// <exception cref="ArgumentNullException"><paramref name="extensionPoint"/> is null.</exception>
        public static void MapFormBuilderRoutes(this ExtensionPoint<RouteCollection> extensionPoint)
        {
            if (extensionPoint == null)
            {
                throw new ArgumentNullException(nameof(extensionPoint));
            }

            extensionPoint.MapLocalizationScriptRoute();

            var routes = extensionPoint.Target;

            routes.MapConfigurationStoreRoute();
            routes.MapConfigurationLoadRoute();

            var formComponentDefinitionProvider = FormComponentDefinitionProviderResolver();
            routes.MapMarkupRoute(formComponentDefinitionProvider);
            routes.MapDefaultPropertiesRoute(formComponentDefinitionProvider);
            routes.MapMetadataRoute();
            routes.MapFormBuilderRoute();
            routes.MapFormBuilderSectionRoute(SectionDefinitionProviderResolver());
            routes.MapFormBuilderPropertiesTabRoute();
            routes.MapFormBuilderPropertiesTabValidationRoute();
            routes.MapFormBuilderGetValidationRuleConfigurationMarkupRoute();
            routes.MapFormBuilderValidateValidationRuleConfigurationRoute();
            routes.MapFormBuilderGetVisibilityConditionConfigurationMarkupRoute();
            routes.MapFormBuilderValidateVisibilityConditionConfigurationRoute();
            routes.MapFormItemPreviewRoute();
            routes.MapFormItemEditRoute();
            routes.MapFormItemEditSubmitRoute();
            routes.MapValidationRuleMetadataRoute();
            routes.MapFileUploaderPostRoute();
            routes.MapFileUploaderDeleteRoute();
        }


        internal static Route MapConfigurationLoadRoute(this RouteCollection routes)
        {
            return routes.MapHttpRoute(
                name: FormBuilderRoutes.CONFIGURATION_LOAD_ROUTE_NAME,
                routeTemplate: FormBuilderRoutes.CONFIGURATION_LOAD_ROUTE_TEMPLATE,
                defaults: new
                {
                    controller = "KenticoFormBuilderConfiguration",
                    action = "Get"
                },
                constraints: new
                {
                    formId = new System.Web.Http.Routing.Constraints.IntRouteConstraint()
                }
            );
        }


        internal static Route MapConfigurationStoreRoute(this RouteCollection routes)
        {
            return routes.MapHttpRoute(
                name: FormBuilderRoutes.CONFIGURATION_STORE_ROUTE_NAME,
                routeTemplate: FormBuilderRoutes.CONFIGURATION_STORE_ROUTE_TEMPLATE,
                defaults: new
                {
                    controller = "KenticoFormBuilderConfiguration",
                    action = "Set"
                },
                constraints: new { }
            );
        }


        internal static Route MapMarkupRoute(this RouteCollection routes, IFormComponentDefinitionProvider formComponentDefinitionProvider)
        {
            return routes.MapRoute(
                name: FormBuilderRoutes.MARKUP_ROUTE_NAME,
                url: FormBuilderRoutes.MARKUP_ROUTE_TEMPLATE,
                defaults: new
                {
                    controller = "KenticoFormComponentMarkup",
                    action = "EditorRow"
                },
                constraints: new
                {
                    identifier = new FormComponentIdentifierConstraint(formComponentDefinitionProvider)
                }
            );
        }


        internal static Route MapDefaultPropertiesRoute(this RouteCollection routes, IFormComponentDefinitionProvider formComponentDefinitionProvider)
        {
            return routes.MapHttpRoute(
                name: FormBuilderRoutes.PROPERTIES_ROUTE_NAME,
                routeTemplate: FormBuilderRoutes.PROPERTIES_ROUTE_TEMPLATE,
                defaults: new
                {
                    controller = "KenticoFormComponentConfiguration",
                    action = "GetDefaultProperties"
                },
                constraints: new
                {
                    identifier = new FormComponentIdentifierConstraint(formComponentDefinitionProvider),
                    formId = new System.Web.Mvc.Routing.Constraints.IntRouteConstraint()
                }
            );
        }


        internal static Route MapMetadataRoute(this RouteCollection routes)
        {
            return routes.MapHttpRoute(
                name: FormBuilderRoutes.METADATA_ROUTE_NAME,
                routeTemplate: FormBuilderRoutes.METADATA_ROUTE_TEMPLATE,
                defaults: new
                {
                    controller = "KenticoFormComponentMetadata",
                    action = "GetAll"
                },
                constraints: new
                {
                    formId = new System.Web.Http.Routing.Constraints.IntRouteConstraint()
                }
            );
        }


        internal static Route MapFormBuilderRoute(this RouteCollection routes)
        {
            return routes.MapRoute(
                name: FormBuilderRoutes.FORMBUILDER_ROUTE_NAME,
                url: FormBuilderRoutes.FORMBUILDER_ROUTE_TEMPLATE,
                defaults: new
                {
                    controller = "KenticoFormBuilder",
                    action = "Index"
                }
            );
        }


        internal static Route MapFormBuilderSectionRoute(this RouteCollection routes, ISectionDefinitionProvider sectionDefinitionProvider)
        {
            return routes.MapRoute(
                name: FormBuilderRoutes.SECTION_MARKUP_ROUTE_NAME,
                url: FormBuilderRoutes.SECTION_MARKUP_ROUTE_TEMPLATE,
                defaults: new
                {
                    action = FormBuilderRoutes.DEFAULT_ACTION_NAME
                },
                constraints: new
                {
                    controller = new SectionConstraint(sectionDefinitionProvider)
                }
            );
        }


        internal static Route MapFormBuilderPropertiesTabRoute(this RouteCollection routes)
        {
            return routes.MapRoute(
                name: FormBuilderRoutes.FORMBUILDER_PROPERTIES_TAB_ROUTE_NAME,
                url: FormBuilderRoutes.FORMBUILDER_PROPERTIES_TAB_ROUTE_TEMPLATE,
                defaults: new
                {
                    controller = "KenticoFormBuilderPropertiesPanel",
                    action = "GetPropertiesMarkup"
                },
                constraints: new
                {
                    formId = new System.Web.Mvc.Routing.Constraints.IntRouteConstraint()
                }
            );
        }


        internal static Route MapFormBuilderPropertiesTabValidationRoute(this RouteCollection routes)
        {
            return routes.MapRoute(
                name: FormBuilderRoutes.VALIDATE_FORMCOMPONENT_PROPERTIES_ROUTE_NAME,
                url: FormBuilderRoutes.VALIDATE_FORMCOMPONENT_PROPERTIES_ROUTE_TEMPLATE,
                defaults: new
                {
                    controller = "KenticoFormBuilderPropertiesPanel",
                    action = "ValidateProperties"
                },
                constraints: new
                {
                    formId = new System.Web.Mvc.Routing.Constraints.IntRouteConstraint()
                }
            );
        }


        internal static Route MapFormBuilderGetValidationRuleConfigurationMarkupRoute(this RouteCollection routes)
        {
            return routes.MapRoute(
                name: FormBuilderRoutes.FORMBUILDER_GET_VALIDATION_RULE_CONFIGURATION_MARKUP_ROUTE_NAME,
                url: FormBuilderRoutes.FORMBUILDER_GET_VALIDATION_RULE_CONFIGURATION_MARKUP_ROUTE_TEMPLATE,
                defaults: new
                {
                    controller = "KenticoFormBuilderPropertiesPanel",
                    action = "GetValidationRuleConfigurationMarkup"
                },
                constraints: new { }
            );
        }


        internal static Route MapFormBuilderValidateValidationRuleConfigurationRoute(this RouteCollection routes)
        {
            return routes.MapRoute(
                name: FormBuilderRoutes.FORMBUILDER_VALIDATE_VALIDATION_RULE_CONFIGURATION_ROUTE_NAME,
                url: FormBuilderRoutes.FORMBUILDER_VALIDATE_VALIDATION_RULE_CONFIGURATION_ROUTE_TEMPLATE,
                defaults: new
                {
                    controller = "KenticoFormBuilderPropertiesPanel",
                    action = "ValidateValidationRuleConfiguration"
                },
                constraints: new { }
            );
        }


        internal static Route MapFormItemPreviewRoute(this RouteCollection routes)
        {
            return routes.MapRoute(
                name: FormBuilderRoutes.FORM_ITEM_PREVIEW_ROUTE_NAME,
                url: FormBuilderRoutes.FORM_ITEM_PREVIEW_ROUTE_TEMPLATE,
                defaults: new
                {
                    controller = "KenticoFormItem",
                    action = "PreviewItem"
                }
            );
        }


        internal static Route MapFormItemEditRoute(this RouteCollection routes)
        {
            return routes.MapRoute(
                name: FormBuilderRoutes.FORM_ITEM_EDIT_ROUTE_NAME,
                url: FormBuilderRoutes.FORM_ITEM_EDIT_ROUTE_TEMPLATE,
                defaults: new
                {
                    controller = "KenticoFormItem",
                    action = "EditItem",
                    itemId = UrlParameter.Optional
                }
            );
        }


        internal static Route MapFormItemEditSubmitRoute(this RouteCollection routes)
        {
            return routes.MapRoute(
                name: FormBuilderRoutes.FORM_ITEM_EDIT_SUBMIT_ROUTE_NAME,
                url: FormBuilderRoutes.FORM_ITEM_EDIT_SUBMIT_ROUTE_TEMPLATE,
                defaults: new
                {
                    controller = "KenticoFormItem",
                    action = "Submit",
                }
            );
        }


        internal static Route MapValidationRuleMetadataRoute(this RouteCollection routes)
        {
            return routes.MapHttpRoute(
                name: FormBuilderRoutes.VALIDATION_RULE_METADATA_ROUTE_NAME,
                routeTemplate: FormBuilderRoutes.VALIDATION_RULE_METADATA_ROUTE_TEMPLATE,
                defaults: new
                {
                    controller = "KenticoValidationRuleMetadata",
                    action = "GetAll"
                }
            );
        }


        internal static Route MapFormBuilderGetVisibilityConditionConfigurationMarkupRoute(this RouteCollection routes)
        {
            return routes.MapRoute(
                name: FormBuilderRoutes.FORMBUILDER_GET_VISIBILITY_CONDITION_CONFIGURATION_MARKUP_ROUTE_NAME,
                url: FormBuilderRoutes.FORMBUILDER_GET_VISIBILITY_CONDITION_CONFIGURATION_MARKUP_ROUTE_TEMPLATE,
                defaults: new
                {
                    controller = "KenticoFormBuilderPropertiesPanel",
                    action = "GetVisibilityConditionConfigurationMarkup"
                }
            );
        }


        internal static Route MapFormBuilderValidateVisibilityConditionConfigurationRoute(this RouteCollection routes)
        {
            return routes.MapRoute(
                name: FormBuilderRoutes.FORMBUILDER_VALIDATE_VISIBILITY_CONDITION_CONFIGURATION_ROUTE_NAME,
                url: FormBuilderRoutes.FORMBUILDER_VALIDATE_VISIBILITY_CONDITION_CONFIGURATION_ROUTE_TEMPLATE,
                defaults: new
                {
                    controller = "KenticoFormBuilderPropertiesPanel",
                    action = "ValidateVisibilityConditionConfiguration"
                },
                constraints: new { }
            );
        }


        internal static Route MapFileUploaderPostRoute(this RouteCollection routes)
        {
            return routes.MapRoute(
                name: FormBuilderRoutes.FILE_UPLOADER_POST_ROUTE_NAME,
                url: FormBuilderRoutes.FILE_UPLOADER_POST_ROUTE_TEMPLATE,
                defaults: new
                {
                    controller = "KenticoFormFileUploader",
                    action = "PostFile"
                },
                constraints: new { }
            );
        }


        internal static Route MapFileUploaderDeleteRoute(this RouteCollection routes)
        {
            return routes.MapRoute(
                name: FormBuilderRoutes.FILE_UPLOADER_DELETE_ROUTE_NAME,
                url: FormBuilderRoutes.FILE_UPLOADER_DELETE_ROUTE_TEMPLATE,
                defaults: new
                {
                    controller = "KenticoFormFileUploader",
                    action = "DeleteTempFile"
                },
                constraints: new { }
            );
        }
    }
}
