import { connect } from "@/builder/helpers/connector";
import _cloneDeep from "lodash.clonedeep";
import _pick from "lodash.pick";

import { getService } from "@/builder/container";
import { IPopUpElementsService, LocalizationService, PopupListingElement, WidgetVariant } from "@/builder/declarations";
import { arrayHelper } from "@/builder/helpers";
import { closePopup, openPopup } from "@/builder/store/actions";
import { selectWidgetVariant } from "@/builder/store/thunks";
import { PopupType } from "@/builder/store/types";
import { updateWidgetConditionTypeParameters } from "@/builder/store/widget-variants/actions";
import { SERVICE_TYPES } from "@/builder/types";
import { PageBuilderState, VariantListingElement } from "@/page-builder/declarations";
import {
  WidgetPersonalizationComponentActions,
  WidgetPersonalizationComponentContext,
  WidgetPersonalizationComponentState,
} from "@/page-builder/declarations";
import { addWidgetVariant, changeVariantsPriority, removeWidgetVariant } from "@/page-builder/store/thunks";

import WidgetPersonalization from "./WidgetPersonalization.vue";

const getVariants = (widgetVariants: WidgetVariant[]): VariantListingElement[] => {
  const originalVariantName = getService<LocalizationService>(SERVICE_TYPES.LocalizationService).getLocalization("variant.original");
  let originalVariantIndex: number;

  const resultVariants: VariantListingElement[] = _cloneDeep(widgetVariants).reverse().map((variant: WidgetVariant, index: number): VariantListingElement => {
    if (!variant.name) {
      originalVariantIndex = index;
    }

    return {
      name: variant.name || originalVariantName,
      iconClass: null,
      key: variant.identifier,
      description: variant.name || originalVariantName,
      renderActionButtons: !!variant.name,
      parameters: variant.conditionTypeParameters,
    };
  });

  return arrayHelper.move(resultVariants, originalVariantIndex, resultVariants.length - 1);
};

const getPopupItems = (
  hasConditionType: boolean,
  widgetIdentifier: string,
  { widgets, metadata, widgetVariants }: PageBuilderState,
): PopupListingElement[] | VariantListingElement[] => {
  const variants = Object.values(_pick(widgetVariants, widgets[widgetIdentifier].variants));

  return hasConditionType ? getVariants(variants) :
    getService<IPopUpElementsService>(SERVICE_TYPES.PopUpElementsService).getWidgetElements(Object.values(metadata.personalizationConditionTypes));
};

const mapStateToProps = (state: PageBuilderState, { widgetIdentifier }: WidgetPersonalizationComponentContext): WidgetPersonalizationComponentState => {
  const { widgets, widgetVariants, displayedWidgetVariants, popup, metadata } = state;
  const conditionType = widgets[widgetIdentifier] ? widgets[widgetIdentifier].conditionType : null;
  const isPersonalizationOpen = widgetIdentifier === popup.componentIdentifier && popup.popupType === PopupType.Personalization;
  const popupItems = widgets[widgetIdentifier] ? getPopupItems(!!conditionType, widgetIdentifier, state) : [];
  const activeItemIdentifier = displayedWidgetVariants[widgetIdentifier] ? widgetVariants[displayedWidgetVariants[widgetIdentifier]].identifier : null;
  const personalizationConditionTypes = metadata.personalizationConditionTypes;

  return {
    isPersonalizationOpen,
    popupItems,
    personalizationDialogPosition: popup.position,
    activeItemIdentifier,
    conditionType,
    personalizationConditionTypes,
  };
};

const mapDispatchToProps = (): WidgetPersonalizationComponentActions => ({
  addWidgetVariant,
  updateWidgetConditionTypeParameters,
  removeWidgetVariant,
  openPopup,
  closePopup,
  selectWidgetVariant,
  changeVariantsPriority,
});

export default connect(mapStateToProps, mapDispatchToProps)(WidgetPersonalization);
