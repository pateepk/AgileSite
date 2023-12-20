/**
 * Provides drag and drop functionality of the Page Builder.
 * @module drag-and-drop
 */

import { Draggable } from "@shopify/draggable/lib/es5/draggable.bundle.legacy";
import { ThunkDispatch } from "redux-thunk";

import { SECTION_MOUNTED_EVENT, SECTION_REMOVE_EVENT } from "@/builder/constants";
import { State } from "@/builder/declarations/store";
import { startDraggingSection, startDraggingWidget } from "@/builder/store/drag-and-drop/thunks";

import { BuilderAction } from "../store/types";
import { makeDragAndDropHandlers } from "./handlers";
import { MirrorPlugin } from "./MirrorPlugin";

const MIRROR_CLASS_NAME = "ktc-draggable-mirror";
const DRAGGED_WIDGET_CLASS_NAME = "ktc-widget--dragged";
const DRAGGABLE_ITEM_SELECTOR = ".ktc-widget";
const DRAG_HANDLE_SELECTOR = ".ktc-widget-header-handle";
const DRAGGED_SECTION_CLASS_NAME = "ktc-section--dragged";
const DRAGGABLE_SECTION_SELECTOR = ".ktc-section";
const DRAG_SECTION_HANDLE_SELECTOR = ".ktc-section-header-drag-icon";

/**
 * Initializes the widget drag and drop feature of the builder.
 * @param dispatch Store's dispatch function.
 * @param getState Store's getState function.
 * @param moveWidget Function used for widget move handling.
 */
const initializeDragAndDrop = (dispatch: ThunkDispatch<State, {}, BuilderAction>, getState: () => State, moveWidget) => {
  const fakeElement = document.createElement("draggable");
  const draggable = new Draggable(fakeElement, {
    classes: {
      "mirror": MIRROR_CLASS_NAME,
      "source:dragging": DRAGGED_WIDGET_CLASS_NAME,
    },
    draggable: DRAGGABLE_ITEM_SELECTOR,
    handle: DRAG_HANDLE_SELECTOR,
    mirror: {
      constrainDimensions: true,
    },
    plugins: [MirrorPlugin],
  });

  draggable.removePlugin(Draggable.Plugins.Mirror);

  registerDraggableContainerEventHandlers(draggable);

  const {
    makeOnDragStart,
    makeOnDragStop,
    onDragOver,
    onDragContainerOver,
    onDragContainerOut,
  } = makeDragAndDropHandlers(dispatch, getState);

  draggable.on("drag:start", makeOnDragStart(startDraggingWidget))
    .on("drag:stop", makeOnDragStop(moveWidget))
    .on("drag:over", onDragOver)
    .on("drag:over:container", onDragContainerOver)
    .on("drag:out:container", onDragContainerOut);
};

/**
 * Registers event handlers for manipulating draggable containers.
 * @param draggable Draggable instance.
 */
const registerDraggableContainerEventHandlers = (draggable: Draggable) => {
  document.addEventListener(SECTION_REMOVE_EVENT, (evt: any) => {
    const containersToBeRemoved = getWidgetZonesElements(evt.detail.sectionIdentifier);

    Array.prototype.forEach.call(containersToBeRemoved, (container) => {
      draggable.removeContainer(container);
    });
  });

  document.addEventListener(SECTION_MOUNTED_EVENT, (evt: any) => {
    const containersToBeAdded = getWidgetZonesElements(evt.detail.sectionIdentifier);

    Array.prototype.forEach.call(containersToBeAdded, (container) => {
      draggable.addContainer(container);
    });
  });

  const getWidgetZonesElements = (sectionIdentifier: string) => {
    return document.querySelectorAll(`div[id="${sectionIdentifier}"] .ktc-widget-zone`);
  };
};

/**
 * Initializes the section drag and drop feature of the builder.
 * @param dispatch Store's dispatch function.
 * @param getState Store's getState function.
 * @param moveSection Function used for section move handling.
 */
const initializeSectionDragAndDrop = (dispatch: ThunkDispatch<State, {}, BuilderAction>, getState: () => State, moveSection) => {
  const sections = document.querySelectorAll(".ktc-editable-area");
  const draggable = new Draggable(sections, {
    classes: {
      "mirror": MIRROR_CLASS_NAME,
      "source:dragging": DRAGGED_SECTION_CLASS_NAME,
    },
    draggable: DRAGGABLE_SECTION_SELECTOR,
    handle: DRAG_SECTION_HANDLE_SELECTOR,
    mirror: {
      constrainDimensions: true,
    },
    plugins: [MirrorPlugin],
  });

  draggable.removePlugin(Draggable.Plugins.Mirror);
  const {
    makeOnDragStart,
    makeOnDragStop,
    onDragOver,
    onDragContainerOver,
    onDragContainerOut,
  } = makeDragAndDropHandlers(dispatch, getState);

  draggable.on("drag:start", makeOnDragStart(startDraggingSection))
    .on("drag:stop", makeOnDragStop(moveSection))
    .on("drag:over", onDragOver)
    .on("drag:over:container", onDragContainerOver)
    .on("drag:out:container", onDragContainerOut);
};

export {
  initializeDragAndDrop,
  initializeSectionDragAndDrop,
};
