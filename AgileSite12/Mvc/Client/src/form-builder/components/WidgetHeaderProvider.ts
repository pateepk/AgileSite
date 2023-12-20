import { State } from "@/builder/declarations";
import { EntityType } from "@/builder/EntityType";
import { connect } from "@/builder/helpers/connector";
import { WidgetHeaderComponentContext, WidgetHeaderComponentState } from "@/form-builder/declarations";

import WidgetHeader from "./WidgetHeader.vue";

const mapStateToProps = (state: State, { widgetIdentifier }: WidgetHeaderComponentContext): WidgetHeaderComponentState => {
  const { widgetVariants, displayedWidgetVariants, widgetSelection, dragAndDrop, highlightedWidget } = state;
  const activeItem = widgetVariants[displayedWidgetVariants[widgetIdentifier]];
  const showWidgetHeader = (highlightedWidget === widgetIdentifier || widgetSelection.identifier === widgetIdentifier) && dragAndDrop.entity !== EntityType.Widget;

  return {
    showWidgetHeader,
    activeItemIdentifier: activeItem ? activeItem.identifier : null,
  };
};

export default connect(mapStateToProps)(WidgetHeader);
