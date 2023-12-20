import * as d from "../";

export interface EditableAreaComponentContext {
  identifier: string;
}

export interface EditableAreaComponentState {
  sectionsInArea: string[];
  dragAndDrop: d.DragAndDrop;
}

export interface EditableAreaComponentActions {
  removeSection;
  changeSection;
}

export interface EditableAreaComponentProperties extends EditableAreaComponentContext, EditableAreaComponentState, EditableAreaComponentActions {
}
