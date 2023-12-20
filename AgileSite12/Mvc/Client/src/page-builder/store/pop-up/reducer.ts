import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { Popup } from "@/builder/declarations";
import * as actions from "@/builder/store/actions";
import { INIT_STATE, popup as basePopupReducer } from "@/builder/store/pop-up/reducer";
import { BuilderAction } from "@/builder/store/types";

import { PageBuilderAction } from "../types";

const popup: Reducer<Popup, PageBuilderAction> = (state, action) => {
  state = basePopupReducer(state, action as BuilderAction);

  switch (action.type) {
    case getType(actions.openModalDialog):
      return {
        ...INIT_STATE,
      };

    default:
      return state;
  }
};

export {
  INIT_STATE,
  popup,
};
