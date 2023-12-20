import { injectable, unmanaged } from "inversify";

import { LocalizationService } from "@/builder/declarations";
import { stringHelper } from "@/builder/helpers";

@injectable()
export abstract class LocalizationServiceBase implements LocalizationService {
  /**
   * Default resource string prefix that is used for general resource strings.
   */
  private readonly defaultPrefix = "kentico.builder";
  private readonly specializedPrefix;

  public constructor(@unmanaged() prefix: string) {
    this.specializedPrefix = prefix;
  }

  /**
   * Gets localized text of resource string for current culture.
   * When string in current culture is not provided returns string in default culture.
   * When localization is not found with prefixed resource string, provided resource string is probed to find the localization.
   * When localization not found at all, resource string is returned.
   * @param resourceString Resource string.
   * @param parameters Parameters for the string formatting after localizing.
   * @returns Localized text.
   */
  public getLocalization(resourceString: string, ...parameters: any[]): string {
    let localization = this.getRawLocalization(this.getResourceStringKey(this.specializedPrefix, resourceString));

    if (localization === undefined) {
      localization = this.getRawLocalization(this.getResourceStringKey(this.defaultPrefix, resourceString));
    }

    if (localization === undefined) {
      return resourceString;
    }

    if (parameters.length > 0) {
      localization = stringHelper.format(localization, ...parameters);
    }

    return localization;
  }

  public getLocalizationWithoutPrefix = (resourceString: string, ...parameters: any[]): string => {
    let localization = this.getRawLocalization(resourceString);

    if (localization === undefined) {
      return resourceString;
    }

    if (parameters.length > 0) {
      localization = stringHelper.format(localization, ...parameters);
    }

    return localization;
  }

  public getCultureCode(): string {
    return window.kentico.localization.culture;
  }

  private getRawLocalization(resourceString: string): string {
    return window.kentico.localization.strings[resourceString];
  }

  private getResourceStringKey(prefix: string, resourceString: string) {
    return prefix + "." + resourceString;
  }
}
