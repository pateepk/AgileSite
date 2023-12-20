import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { PageBuilderAction } from "@/page-builder/store/types";

import { PageTemplate } from "@/builder/declarations";
import * as actions from "../actions";

const initialState: PageTemplate = {
  identifier: null,
  properties: {},
};

export const pageTemplate: Reducer<PageTemplate, PageBuilderAction> = (state = initialState, action) => {
  switch (action.type) {
    case getType(actions.updateTemplateProperties):
      return {
        ...state,
        properties: action.payload.properties,
      };

    default:
      return state;
  }
};
