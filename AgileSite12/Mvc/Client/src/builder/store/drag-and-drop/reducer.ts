
import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { DragAndDrop } from "@/builder/declarations/store";

import * as actions from "../actions";
import { BuilderAction } from "../types";

const INIT_STATE: DragAndDrop = {
  entity: null,
  typeIdentifier: null,
  itemIdentifier: null,
  originalPosition: null,
  sourceContainerIdentifier: null,
  targetContainerIdentifier: null,
  dropMarkerPosition: null,
  bannedContainers: [],
};

export const dragAndDrop: Reducer<DragAndDrop, BuilderAction> = (state = INIT_STATE, action) => {
  switch (action.type) {
    case getType(actions.startDragging):
      return {
        ...state,
        entity: action.payload.entity,
        typeIdentifier: action.payload.typeIdentifier,
        itemIdentifier: action.payload.itemIdentifier,
        originalPosition: action.payload.originalPosition,
        sourceContainerIdentifier: action.payload.sourceContainerIdentifier,
        bannedContainers: action.payload.bannedContainers,
      };

    case getType(actions.endDragging):
      return {
        ...INIT_STATE,
      };

    case getType(actions.showDropMarker):
      return {
        ...state,
        targetContainerIdentifier: action.payload.containerIdentifier,
        dropMarkerPosition: action.payload.dropMarkerPosition,
      };

    case getType(actions.hideDropMarker):
      return {
        ...state,
        targetContainerIdentifier: null,
        dropMarkerPosition: null,
      };

    default:
      return state;
  }
};
