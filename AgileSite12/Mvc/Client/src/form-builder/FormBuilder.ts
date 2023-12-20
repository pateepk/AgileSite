import { inject, injectable } from "inversify";
import Vue, { ComponentOptions } from "vue";

import { Builder } from "@/builder/BaseBuilder";
import { LocalizationService, State } from "@/builder/declarations";
import { createStoreProviderMixin } from "@/builder/helpers/connector";
import { MessagingService } from "@/builder/services/MessagingService";
import { PageConfigurationService } from "@/builder/services/page-configuration/PageConfigurationService";
import { SERVICE_TYPES } from "@/builder/types";

import PropertiesEditor from "@/form-builder/components/PropertiesEditorProvider";
import SaveMessage from "@/form-builder/components/SaveMessageProvider";
import { FormBuilderState } from "@/form-builder/declarations";
import { FormBuilderConfig } from "@/form-builder/FormBuilderConfig";
import { configureStore } from "@/form-builder/store";
import { moveSection, moveWidget } from "@/form-builder/store/thunks";
import { FormBuilderOptions } from "@/form-builder/types";

import EditableAreaFormBuilder from "@/form-builder/components/EditableArea.vue";
import GlobalComponentWrapper from "./components/GlobalComponentWrapperProvider";

@injectable()
export class FormBuilder extends Builder {

  public constructor(
    @inject(SERVICE_TYPES.LocalizationService) localizationService: LocalizationService,
    @inject(SERVICE_TYPES.PageConfigurationService) pageConfigurationService: PageConfigurationService,
    @inject(SERVICE_TYPES.MessagingService) messagingService: MessagingService,
  ) {
    super(localizationService, pageConfigurationService, messagingService);
  }

  /**
   * Initializes the form builder.
   * @param options Form builder configuration object.
   */
  public async initializeFormBuilder(options: FormBuilderOptions) {
    const config = new FormBuilderConfig(options.formIdentifier, options.propertiesEditorClientId, options.propertiesEditorEndpoint,
      options.validationRuleMetadataEndpoint, options.validationRuleMarkupEndpoint, options.visibilityConditionMarkupEndpoint,
      options.saveMessageClientId, options.applicationPath, options.configurationEndpoints,
      options.metadataEndpoint, options.allowedOrigins, options.constants);

    const store = await super.initialize(
      config,
      (state: State) => configureStore(state as FormBuilderState, options.developmentMode),
      GlobalComponentWrapper,
      EditableAreaFormBuilder,
      moveWidget,
      moveSection,
    );

    const storeProviderMixin = createStoreProviderMixin(store);
    this.initializePropertiesEditor(storeProviderMixin, config);
    this.initializeSaveMessage(storeProviderMixin, config);
  }

  private initializePropertiesEditor(storeProviderMixin: ComponentOptions<Vue>, config: FormBuilderConfig) {
    // tslint:disable-next-line:no-unused-expression
    new Vue({
      components: { PropertiesEditor },
      el: `#${config.propertiesEditorClientId}`,
      mixins: [storeProviderMixin],
    });
  }

  private initializeSaveMessage(storeProviderMixin: ComponentOptions<Vue>, config: FormBuilderConfig) {
    // tslint:disable-next-line:no-unused-expression
    new Vue({
      components: { SaveMessage },
      el: `#${config.saveMessageClientId}`,
      mixins: [storeProviderMixin],
    });
  }
}
