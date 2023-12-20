import "reflect-metadata";

import { Container } from "inversify";

import { IPopUpElementsService, LocalizationService, NormalizerService } from "@/builder/declarations";
import { MessagingService } from "@/builder/services/MessagingService";
import { PageConfigurationService } from "@/builder/services/page-configuration/PageConfigurationService";
import { SERVICE_TYPES } from "@/builder/types";

import { FormBuilderLocalizationService } from "@/form-builder/services/FormBuilderLocalizationService";
import { FormBuilderNormalizerService } from "@/form-builder/services/FormBuilderNormalizerService";
import { FormBuilderPopUpElementsService } from "@/form-builder/services/FormBuilderPopUpElementsService";
import { FormBuilderPageConfigurationService } from "@/form-builder/services/page-configuration/FormBuilderPageConfigurationService";

const formBuilderContainer = new Container();
formBuilderContainer.bind<LocalizationService>(SERVICE_TYPES.LocalizationService).to(FormBuilderLocalizationService);
formBuilderContainer.bind<NormalizerService>(SERVICE_TYPES.NormalizerService).to(FormBuilderNormalizerService);
formBuilderContainer.bind<PageConfigurationService>(SERVICE_TYPES.PageConfigurationService).to(FormBuilderPageConfigurationService);
formBuilderContainer.bind<IPopUpElementsService>(SERVICE_TYPES.PopUpElementsService).to(FormBuilderPopUpElementsService);
formBuilderContainer.bind<MessagingService>(SERVICE_TYPES.MessagingService).to(MessagingService);

export {
  formBuilderContainer,
};
