import { Entities, PersonalizationConditionTypeMetadata, PopupListingElement } from "@/builder/declarations";
import { ComponentPosition } from "@/builder/store/types";

import { VariantListingElement } from "./variant-listing";

export interface WidgetPersonalizationComponentContext {
  widgetIdentifier: string;
}

export interface WidgetPersonalizationComponentState {
  isPersonalizationOpen: boolean;
  popupItems: PopupListingElement[] | VariantListingElement[];
  personalizationDialogPosition: ComponentPosition;
  activeItemIdentifier: string;
  conditionType?: string;
  personalizationConditionTypes: Entities<PersonalizationConditionTypeMetadata>;
}

export interface WidgetPersonalizationComponentActions {
  addWidgetVariant;
  updateWidgetConditionTypeParameters;
  removeWidgetVariant;
  openPopup;
  closePopup: () => void;
  selectWidgetVariant;
  changeVariantsPriority;
}

export interface WidgetPersonalizationComponentProperties
  extends WidgetPersonalizationComponentContext, WidgetPersonalizationComponentState, WidgetPersonalizationComponentActions {
}
