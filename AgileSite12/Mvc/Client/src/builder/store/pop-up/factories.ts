import { ThunkAction } from "@/builder/declarations";

import { AddSectionThunk, AddWidgetThunk, PopupType } from "../types";

export class SelectItemThunkFactory {
  /**
   * Factory encapsulating the select item thunk action to ensure correct type of the thunk in components.
   * @param addWidget Add widget thunk action.
   * @param addSection Add section thunk action.
   */
  constructor(private readonly addWidget: AddWidgetThunk, private readonly addSection: AddSectionThunk) { }

  /**
   * Decides which action should be dispatched after an item is selected.
   * @param componentName Name of the selected component.
   */
  public selectItem = (componentName: string): ThunkAction<Promise<void>> =>
    async (dispatch, getState, services) => {
      const { popup, zones, editableAreas } = getState();
      let position: number;

      switch (popup.popupType) {
        case PopupType.AddWidget: {
          const zoneIdentifier = popup.zoneIdentifier;
          position = zones[zoneIdentifier].widgets.indexOf(popup.componentIdentifier) + 1;
          await this.addWidget(componentName, zoneIdentifier, position)(dispatch, getState, services);
          break;
        }
        case PopupType.AddSection: {
          const areaIdentifier = popup.areaIdentifier;
          position = editableAreas[areaIdentifier].sections.indexOf(popup.componentIdentifier) + 1;
          await this.addSection(componentName, areaIdentifier, position)(dispatch, getState, services);
          break;
        }
        default:
          return;
      }
    }
}
