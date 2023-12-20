import { StateType } from "typesafe-actions";

import { HighlightedSectionAction } from "@/builder/store/highlighted-section/types";
import { PopUpAction } from "@/builder/store/pop-up/types";

import { DisplayedWidgetVariantsAction } from "./displayed-widget-variants/types";
import { DragAndDropAction } from "./drag-and-drop/types";
import { HighlightedWidgetAction } from "./highlighted-widget/types";
import { MarkupsAction } from "./markups/types";
import { ModalDialogsAction } from "./modal-dialogs/types";
import { baseReducers } from "./reducers";
import { SectionsAction } from "./sections/types";
import { WidgetSelectionAction } from "./widget-selection/types";
import { WidgetVariantsAction } from "./widget-variants/types";
import { WidgetsAction } from "./widgets/types";

export { AddWidgetThunk } from "./widgets/types";
export { AddSectionThunk } from "./sections/types";
export { ModalDialogType, OpenModalDialogOptions } from "./modal-dialogs/types";

export enum ComponentPosition {
  Center = "center",
  Left = "left",
  Right = "right",
}

export enum PopupType {
  AddWidget = "addWidget",
  AddSection = "addSection",
  ChangeSection = "changeSection",
  Personalization = "personalization",
}

export interface ListingOffset {
  readonly top: number;
  readonly left: number;
}

export type BuilderAction =
  | DisplayedWidgetVariantsAction
  | DragAndDropAction
  | HighlightedSectionAction
  | HighlightedWidgetAction
  | MarkupsAction
  | PopUpAction
  | SectionsAction
  | WidgetSelectionAction
  | WidgetVariantsAction
  | WidgetsAction
  | ModalDialogsAction;

export type State = Readonly<StateType<typeof baseReducers>>;
