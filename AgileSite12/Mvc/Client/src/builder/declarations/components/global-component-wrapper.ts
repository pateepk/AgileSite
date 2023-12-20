import { PopupType } from "@/builder/store/types";
import { ModalDialog } from "../store";

export interface GlobalComponentWrapperComponentState {
  readonly popupType: PopupType;
  readonly modalDialogs: ModalDialog[];
}

export interface GlobalComponentWrapperComponentActions {
  selectItem: (componentName: string) => void;
}

export interface GlobalComponentWrapperComponentProperties extends GlobalComponentWrapperComponentActions, GlobalComponentWrapperComponentState {
}
