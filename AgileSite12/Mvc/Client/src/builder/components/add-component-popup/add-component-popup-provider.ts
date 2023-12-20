import { getService } from "@/builder/container";
import {
  IPopUpElementsService,
  PopupListingElement,
  State,
} from "@/builder/declarations";
import { connect, MapDispatchToProps, MapStateToProps } from "@/builder/helpers/connector";
import { filterWidgets } from "@/builder/helpers/widget-restrictions";
import { closePopup } from "@/builder/store/actions";
import { PopupType } from "@/builder/store/types";
import { SERVICE_TYPES } from "@/builder/types";

import { AddComponentPopupComponentActions, AddComponentPopupComponentState } from "./add-component-popup-types";
import AddComponentPopup from "./AddComponentPopup.vue";

type AddComponentPopupStateToProps = MapStateToProps<State, object, AddComponentPopupComponentState>;
type AddComponentPopupDispatchToProps = MapDispatchToProps<AddComponentPopupComponentActions>;

const mapStateToProps: AddComponentPopupStateToProps = ({ popup, dragAndDrop, editableAreasConfiguration, metadata }) => {
  const popUpElementsService = getService<IPopUpElementsService>(SERVICE_TYPES.PopUpElementsService);

  let items: PopupListingElement[] = [];

  if (popup.popupType === PopupType.AddWidget) {
    const editableAreaConfiguration = editableAreasConfiguration[popup.areaIdentifier];
    items = editableAreaConfiguration &&
              popUpElementsService.getWidgetElements((filterWidgets(editableAreaConfiguration.widgetRestrictions, Object.values(metadata.widgets))));
  } else if (popup.popupType === PopupType.AddSection) {
    items = popUpElementsService.getSectionElements(Object.values(metadata.sections));
  }

  return {
    popup,
    isDnDActive: !!dragAndDrop.itemIdentifier,
    items,
  };
};

const mapDispatchToProps: AddComponentPopupDispatchToProps = () => ({
  closePopup,
});

export default connect(mapStateToProps, mapDispatchToProps)(AddComponentPopup);
