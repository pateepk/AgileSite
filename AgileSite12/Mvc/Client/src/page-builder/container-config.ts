import "reflect-metadata";

import { Container } from "inversify";

import { IPopUpElementsService, LocalizationService, NormalizerService } from "@/builder/declarations";
import { MessagingService } from "@/builder/services/MessagingService";
import { PageConfigurationService } from "@/builder/services/page-configuration/PageConfigurationService";
import { PopUpElementsService } from "@/builder/services/PopUpElementsService";
import { SERVICE_TYPES } from "@/builder/types";

import { PageBuilderLocalizationService } from "@/page-builder/services/PageBuilderLocalizationService";
import { PageBuilderNormalizerService } from "@/page-builder/services/PageBuilderNormalizerService";
import { PageBuilderMessagingService } from "./services/PageBuilderMessagingService";

const pageBuilderContainer = new Container();
pageBuilderContainer.bind<LocalizationService>(SERVICE_TYPES.LocalizationService).to(PageBuilderLocalizationService);
pageBuilderContainer.bind<NormalizerService>(SERVICE_TYPES.NormalizerService).to(PageBuilderNormalizerService);
pageBuilderContainer.bind<PageConfigurationService>(SERVICE_TYPES.PageConfigurationService).to(PageConfigurationService);
pageBuilderContainer.bind<IPopUpElementsService>(SERVICE_TYPES.PopUpElementsService).to(PopUpElementsService);
pageBuilderContainer.bind<MessagingService>(SERVICE_TYPES.MessagingService).to(PageBuilderMessagingService);

export { pageBuilderContainer };
