import { createAction } from "typesafe-actions";

import { EntityType } from "@/builder/EntityType";

/**
 * 'Start dragging' action creator.
 * @param entity Dragged item's entity type.
 * @param typeIdentifier Dragged item's type identifier.
 * @param itemIdentifier Dragged item's identifier.
 * @param sourceContainerIdentifier Dragged item's source container identifier.
 * @param originalPosition Dragged item's original position.
 * @param bannedContainers List of containers which are banned for dropping the item.
 */
export const startDragging = createAction("dragAndDrop/START_DRAGGING", (resolve) => (
  entity: EntityType, typeIdentifier: string, itemIdentifier: string, sourceContainerIdentifier: string,
  originalPosition: number, bannedContainers: string[]) => resolve({
    entity,
    typeIdentifier,
    itemIdentifier,
    sourceContainerIdentifier,
    originalPosition,
    bannedContainers,
  }),
);

/**
 * 'End dragging' action creator.
 */
export const endDragging = createAction("dragAndDrop/END_DRAGGING");

/**
 * 'Show drop marker' action creator.
 * @param containerIdentifier Target container identifier
 * @param dropMarkerPosition Drag and drop marker position
 */
export const showDropMarker = createAction("dragAndDrop/SHOW_DROP_MARKER", (resolve) =>
  (containerIdentifier: string, dropMarkerPosition: number) => resolve({
    containerIdentifier,
    dropMarkerPosition,
  }),
);

/**
 * 'Hide drop marker' action creator.
 */
export const hideDropMarker = createAction("dragAndDrop/HIDE_DROP_MARKER");
