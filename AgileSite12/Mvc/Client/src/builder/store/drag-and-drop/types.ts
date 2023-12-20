import { ActionType } from "typesafe-actions";

import * as actions from "./actions";

export type StartDraggingAction = ActionType<typeof actions.startDragging>;
export type EndDraggingAction = ActionType<typeof actions.endDragging>;
export type ShowDropMarkerAction = ActionType<typeof actions.showDropMarker>;
export type HideDropMarkerAction = ActionType<typeof actions.hideDropMarker>;
export type DragAndDropAction = ActionType<typeof actions>;
