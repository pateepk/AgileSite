import { BuilderConfig } from "@/builder/builderConfig";
import { LocalizationService, ThunkDispatch } from "@/builder/declarations";

import { registerMediaFilesSelectorDialogApi } from "./media-files-selector";
import { registerPageSelectorApi } from "./page-selector";

export const registerSelectorsApi = (namespace, config: BuilderConfig, dispatch: ThunkDispatch, localizationService: LocalizationService) => {
  registerMediaFilesSelectorDialogApi(namespace, config, dispatch, localizationService);
  registerPageSelectorApi(namespace, config, dispatch, localizationService);
};
