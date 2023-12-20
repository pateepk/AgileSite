import { IPopUpElementsService, WidgetZoneComponentActions, WidgetZoneComponentContext, WidgetZoneComponentState  } from "@/builder/declarations";
import { connect } from "@/builder/helpers/connector";
import { filterWidgets } from "@/builder/helpers/widget-restrictions";
import { closePopup } from "@/builder/store/actions";
import { openPopup } from "@/builder/store/thunks";
import { FormBuilderState } from "@/form-builder/declarations";
import { removeWidget } from "@/form-builder/store/thunks";

import WidgetZone from "@/builder/components/WidgetZone.vue";
import { getService } from "@/builder/container";
import { SERVICE_TYPES } from "@/builder/types";

const mapStateToProps = (state: FormBuilderState, propsData: WidgetZoneComponentContext): WidgetZoneComponentState => {
  const { dragAndDrop, widgets, popup, zones, sections, editableAreasConfiguration, metadata } = state;
  const { sectionIdentifier, zoneIndex, areaIdentifier } = propsData;

  const zoneIdentifier = sections[sectionIdentifier] ? sections[sectionIdentifier].zones[zoneIndex] : "";
  const editableAreaConfiguration = editableAreasConfiguration[areaIdentifier];
  const popUpElementsService = getService<IPopUpElementsService>(SERVICE_TYPES.PopUpElementsService);

  return {
    zoneIdentifier,
    dragAndDrop,
    zone: zones[zoneIdentifier],
    popup,
    widgetsInZone: zones[zoneIdentifier] ? zones[zoneIdentifier].widgets.map((widgetId) => widgets[widgetId]) : [],
    availableWidgetTypes: popUpElementsService.getWidgetElements((filterWidgets(editableAreaConfiguration.widgetRestrictions, Object.values(metadata.widgets)))),
  };
};

const mapDispatchToProps = (): WidgetZoneComponentActions => ({
  removeWidget,
  closePopup,
  openPopup,
});

export default connect(mapStateToProps, mapDispatchToProps)(WidgetZone);
