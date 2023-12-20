import _omit from "lodash.omit";
import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { Entities, WidgetZone } from "@/builder/declarations";
import { arrayHelper } from "@/builder/helpers";
import { mapZoneWidgets } from "@/builder/store/zones/helpers";

import * as actions from "../actions";
import { BuilderAction } from "../types";

const INIT_STATE: Entities<WidgetZone> = {};

export const zones: Reducer<Entities<WidgetZone>, BuilderAction> = (state = INIT_STATE, action) => {
  switch (action.type) {
    case getType(actions.addSection):
      return {
        ...state,
        ...createZones(action.payload.section.zones),
      };

    case getType(actions.removeSection):
      return _omit(state, action.payload.section.zones);

    case getType(actions.changeSection):
      const newZones = action.payload.widgetsToRemove.length ?
        createZones(action.payload.newSection.zones) :
        mapZoneWidgets(action.payload.oldSection.zones, action.payload.newSection.zones, state);

      return {
        ..._omit(state, action.payload.oldSection.zones),
        ...newZones,
      };

    case getType(actions.addWidget):
    case getType(actions.removeWidget):
      const { zoneIdentifier } = action.payload;
      return {
        ...state,
        [zoneIdentifier]: zone(state[zoneIdentifier], action),
      };

    case getType(actions.moveWidget):
      return moveWidgets(state, action.payload);

    default:
      return state;
  }
};

const zone = (state: WidgetZone, action: BuilderAction): WidgetZone => {
  switch (action.type) {
    case getType(actions.addWidget):
      return {
        ...state,
        widgets: arrayHelper.insertItemIntoArray(state.widgets, action.payload.widget.identifier, action.payload.position),
      };

    case getType(actions.removeWidget):
      return {
        ...state,
        widgets: state.widgets.filter((widgetId) => widgetId !== action.payload.widget.identifier),
      };

    default:
      return state;
  }
};

const createZones = (zoneIdentifiers: string[]): Entities<WidgetZone> => {
  return zoneIdentifiers.reduce((widgetZones: Entities<WidgetZone>, identifier: string) => {
    widgetZones[identifier] = {
      identifier,
      widgets: [],
    };
    return widgetZones;
  }, {});
};

const moveWidgets = (state: Entities<WidgetZone>, action): Entities<WidgetZone> => {
  const { originalZoneIdentifier, position, targetZoneIdentifier, widgetIdentifier } = action;
  let arraysAfterMove;
  const newState = { ...state };

  if (originalZoneIdentifier === targetZoneIdentifier) {
    arraysAfterMove = arrayHelper.moveItemBetweenArrays(state[originalZoneIdentifier].widgets, widgetIdentifier, position);
  } else {
    arraysAfterMove = arrayHelper.moveItemBetweenArrays(state[originalZoneIdentifier].widgets, widgetIdentifier, position, state[targetZoneIdentifier].widgets);
    newState[targetZoneIdentifier] = {
      ...state[targetZoneIdentifier],
      widgets: arraysAfterMove.destinationArray,
    };
  }

  newState[originalZoneIdentifier] = {
    ...state[originalZoneIdentifier],
    widgets: arraysAfterMove.sourceArray,
  };

  return newState;
};
