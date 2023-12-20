import * as api from "@/builder/api";
import { ThunkAction } from "@/builder/declarations";
import { selectWidgetVariant as selectWidgetVariantAction } from "@/builder/store/displayed-widget-variants/actions";
import { updateWidgetVariantMarkups } from "@/builder/store/markups/actions";

const selectWidgetVariant = (widgetIdentifier: string, oldVariantIdentifier: string, newVariantIdentifier: string): ThunkAction<Promise<void>> => async (dispatch, getState) => {
  const { markups, widgets, metadata, widgetVariants } = getState();

  if (markups.variants[oldVariantIdentifier].isDirty) {
    const type = widgets[widgetIdentifier].type;
    const freshMarkup = await api.getComponentMarkup(metadata.widgets[type].markupUrl, widgetVariants[oldVariantIdentifier].properties);

    dispatch(updateWidgetVariantMarkups({[oldVariantIdentifier]: freshMarkup }));
  }

  dispatch(selectWidgetVariantAction(newVariantIdentifier, widgetIdentifier));
};

export {
  selectWidgetVariant,
};
