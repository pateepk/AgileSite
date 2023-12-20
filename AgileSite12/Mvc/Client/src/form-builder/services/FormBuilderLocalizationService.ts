import { injectable } from "inversify";

import { LocalizationServiceBase } from "@/builder/services/LocalizationServiceBase";

/**
 * Service responsible for localization in Forms builder.
 */
@injectable()
export class FormBuilderLocalizationService extends LocalizationServiceBase {
  constructor() {
    super("kentico.formbuilder");
  }
}
