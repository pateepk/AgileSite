import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { Entities, Section } from "@/builder/declarations";

import * as actions from "../actions";
import { BuilderAction } from "../types";

const INIT_STATE: Entities<Section> = {};

export const sections: Reducer<Entities<Section>, BuilderAction> = (state = INIT_STATE, action) => {
  switch (action.type) {
    case getType(actions.addSection):
      return {
        ...state,
        [action.payload.section.identifier]: action.payload.section,
      };

    case getType(actions.removeSection):
      return removeSection(state, action.payload.section.identifier);

    case getType(actions.changeSection):
      return {
        ...state,
        [action.payload.oldSection.identifier]: {
          ...action.payload.newSection,
          identifier: action.payload.oldSection.identifier,
        },
      };

    default:
      return state;
  }
};

const removeSection = (state: Entities<Section>, sectionIdentifier: string) => {
  const { [sectionIdentifier]: removedSection, ...newState } = state;

  return newState;
};
