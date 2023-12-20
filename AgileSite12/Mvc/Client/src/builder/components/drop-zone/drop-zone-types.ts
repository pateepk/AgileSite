import { DragAndDrop } from "@/builder/declarations";

export type HideDropMarker = () => void;
export type ShowDropMarker = (containerIdentifier: string, dropMarkerPosition: number) => void;

export interface DropZoneComponentContext {
  containerIdentifier: string;
}

export interface DropZoneComponentState {
  dragAndDrop: DragAndDrop;
}

export interface DropZoneComponentActions {
  hideDropMarker: HideDropMarker;
  showDropMarker: ShowDropMarker;
}

export interface DropZoneComponentProperties extends DropZoneComponentContext, DropZoneComponentState, DropZoneComponentActions {
}
