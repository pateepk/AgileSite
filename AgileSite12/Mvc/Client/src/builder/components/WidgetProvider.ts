import { State, WidgetComponentActions, WidgetComponentContext, WidgetComponentState } from "@/builder/declarations";
import { connect } from "@/builder/helpers/connector";
import { closePopup, dehighlightWidget, enableWidgetClickAway, highlightWidget, selectWidget, unselectWidget } from "@/builder/store/actions";
import { openPopup, setWidgetProperty } from "@/builder/store/thunks";

import Widget from "./Widget.vue";

const mapStateToProps = (state: State, { widgetIdentifier }: WidgetComponentContext): WidgetComponentState => {
  const {
    dragAndDrop, widgetSelection, highlightedWidget,
    popup, metadata, widgetVariants, markups,
    displayedWidgetVariants, widgets,
  } = state;

  const widget = widgets[widgetIdentifier];
  const displayedVariantIdentifier = displayedWidgetVariants[widgetIdentifier];

  return {
    dragAndDrop,
    selectedWidgetIdentifier: widgetSelection.identifier,
    highlightedWidgetIdentifier: highlightedWidget,
    popup,
    widget,
    displayedVariant: widgetVariants[displayedVariantIdentifier],
    displayedVariantMarkup: displayedVariantIdentifier ? markups.variants[displayedVariantIdentifier].markup : "",
    widgetTitle: widget ? metadata.widgets[widget.type].name : "",
    isFrozen: widgetSelection.freezeSelection,
    isClickAwayDisabled: widgetSelection.preventClickAway,
  };
};

const mapDispatchToProps = (): WidgetComponentActions => ({
  highlightWidget,
  dehighlightWidget,
  selectWidget,
  unselectWidget,
  enableWidgetClickAway,
  closePopup,
  openPopup,
  setWidgetProperty,
});

export default connect(mapStateToProps, mapDispatchToProps)(Widget);
