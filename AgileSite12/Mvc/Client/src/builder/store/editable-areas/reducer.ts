import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { EditableArea, Entities } from "@/builder/declarations";
import { arrayHelper } from "@/builder/helpers";

import * as actions from "../actions";
import { BuilderAction } from "../types";

const INIT_STATE: Entities<EditableArea> = {};

export const editableAreas: Reducer<Entities<EditableArea>, BuilderAction> = (state = INIT_STATE, action) => {
  switch (action.type) {
    case getType(actions.addSection):
      return {
        ...state,
        [action.payload.areaIdentifier]: {
          ...state[action.payload.areaIdentifier],
          sections: arrayHelper.insertItemIntoArray(state[action.payload.areaIdentifier].sections, action.payload.section.identifier, action.payload.position),
        },
      };

    case getType(actions.removeSection):
      return {
        ...state,
        [action.payload.areaIdentifier]: {
          ...state[action.payload.areaIdentifier],
          sections: state[action.payload.areaIdentifier].sections.filter((sectionId) => sectionId !== action.payload.section.identifier),
        },
      };

    case getType(actions.moveSection):
      const { sectionIdentifier, originalAreaIdentifier, targetAreaIdentifier, position } = action.payload;
      let arraysAfterMove;
      const newState = { ...state };

      if (originalAreaIdentifier === targetAreaIdentifier) {
        arraysAfterMove = arrayHelper.moveItemBetweenArrays(state[originalAreaIdentifier].sections, sectionIdentifier, position);
      } else {
        arraysAfterMove = arrayHelper.moveItemBetweenArrays(state[originalAreaIdentifier].sections, sectionIdentifier, position, state[targetAreaIdentifier].sections);
        newState[targetAreaIdentifier] = {
          ...state[targetAreaIdentifier],
          sections: arraysAfterMove.destinationArray,
        };
      }

      newState[originalAreaIdentifier] = {
        ...state[originalAreaIdentifier],
        sections: arraysAfterMove.sourceArray,
      };

      return newState;

    default:
      return state;
  }
};
