import { injectable } from "inversify";

import { LocalizationServiceBase } from "@/builder/services/LocalizationServiceBase";

/**
 * Service responsible for localization in Page builder.
 */
@injectable()
export class PageBuilderLocalizationService extends LocalizationServiceBase {
  constructor() {
    super("kentico.pagebuilder");
  }
}
