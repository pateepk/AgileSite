import { connect } from "@/builder/helpers/connector";

import { PageBuilderState } from "@/page-builder/declarations";
import {
  WidgetHeaderComponentContext,
  WidgetHeaderComponentState,
} from "@/page-builder/declarations";

import WidgetHeader from "./WidgetHeader.vue";

const mapStateToProps = (state: PageBuilderState, { widgetIdentifier }: WidgetHeaderComponentContext): WidgetHeaderComponentState => {
  const { widgets, widgetSelection, dragAndDrop, highlightedWidget, metadata } = state;
  const showWidgetHeader = (highlightedWidget === widgetIdentifier || widgetSelection.identifier === widgetIdentifier) && !dragAndDrop.itemIdentifier;
  const personalizationConditionTypes = metadata.personalizationConditionTypes;
  const showPropertiesButton = widgets[widgetIdentifier] ? metadata.widgets[widgets[widgetIdentifier].type].hasEditableProperties : false;

  return {
    showWidgetHeader,
    personalizationConditionTypes,
    showPropertiesButton,
  };
};

export default connect(mapStateToProps)(WidgetHeader);
