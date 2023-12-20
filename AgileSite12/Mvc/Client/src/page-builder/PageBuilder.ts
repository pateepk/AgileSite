import { inject, injectable } from "inversify";

import { Builder } from "@/builder/BaseBuilder";
import { LocalizationService, State } from "@/builder/declarations";
import { MessagingService } from "@/builder/services/MessagingService";
import { registerSelectorsApi } from "@/builder/services/modal-dialog/selectors";
import { PageConfigurationService } from "@/builder/services/page-configuration/PageConfigurationService";
import { SERVICE_TYPES } from "@/builder/types";

import { PageBuilderState } from "@/page-builder/declarations";
import { PageBuilderConfig } from "@/page-builder/PageBuilderConfig";
import { configureStore } from "@/page-builder/store";
import { PageBuilderOptions } from "@/page-builder/types";

import EditableAreaPageBuilder from "./components/EditableArea.vue";
import GlobalComponentWrapper from "./components/PageBuilderGlobalComponentWrapperProvider";
import { moveSection, moveWidget } from "./store/thunks";

@injectable()
export class PageBuilder extends Builder {

  public constructor(
    @inject(SERVICE_TYPES.LocalizationService) localizationService: LocalizationService,
    @inject(SERVICE_TYPES.PageConfigurationService) pageConfigurationService: PageConfigurationService,
    @inject(SERVICE_TYPES.MessagingService) messagingService: MessagingService,
  ) {
    super(localizationService, pageConfigurationService, messagingService);
  }

  public async initializePageBuilder(options: PageBuilderOptions) {
    const config = new PageBuilderConfig(options.pageIdentifier, options.applicationPath, options.configurationEndpoints,
      options.metadataEndpoint, options.allowedOrigins, options.constants, options.featureSet,
      options.pageTemplate, options.selectors);

    const store = await super.initialize(
      config,
      (state: State) => configureStore(state as PageBuilderState, options.developmentMode),
      GlobalComponentWrapper,
      EditableAreaPageBuilder,
      moveWidget,
      moveSection,
    );

    registerSelectorsApi(window.kentico.modalDialog, config, store.dispatch, this.localizationService);
  }
}
