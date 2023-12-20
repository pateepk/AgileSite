import { injectable } from "inversify";

import { getService } from "@/builder/container";
import { LocalizationService } from "@/builder/declarations";
import { logger } from "@/builder/logger";
import { PageConfigurationService } from "@/builder/services/page-configuration/PageConfigurationService";
import { logWarning } from "@/builder/services/page-configuration/provider";
import { SERVICE_TYPES } from "@/builder/types";

import { getValidationRulesMetadata } from "@/form-builder/api";
import { UNKNOWN_SECTION_TYPE_NAME } from "@/form-builder/constants";
import { FormBuilderConfiguration } from "@/form-builder/declarations";

@injectable()
class FormBuilderPageConfigurationService extends PageConfigurationService {
  async getConfiguration(config: FormBuilderConfiguration): Promise<any> {
    const pageBuilderState = await super.getConfiguration(config);
    const localizationService = getService<LocalizationService>(SERVICE_TYPES.LocalizationService);

    const anySectionReturnedEmptyMarkup = Object.values(pageBuilderState.markups.sections).some((section) => !section.markup);
    if (anySectionReturnedEmptyMarkup) {
      logger.logError(localizationService.getLocalization("error.sectionrender"));
    }

    const containsUnknownSections = Object.values(pageBuilderState.sections).some((section) => section.type === UNKNOWN_SECTION_TYPE_NAME);
    if (containsUnknownSections) {
      logWarning("error.sectionconfig", []);
    }

    const validationRulesMetadata = await getValidationRulesMetadata();
    return {
      ...pageBuilderState,
      validationRulesMetadata,
    };
  }
}

export {
  FormBuilderPageConfigurationService,
};
