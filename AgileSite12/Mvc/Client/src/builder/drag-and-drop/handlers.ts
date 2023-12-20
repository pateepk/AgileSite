/**
 * Provides drag and drop event handlers.
 * @module drag-and-drop/handlers
 */

import { ThunkAction, ThunkDispatch } from "redux-thunk";

import { HIDE_ELEMENT_TIMEOUT } from "@/builder/constants";
import { State } from "@/builder/declarations";
import { invokeInlineEditorsDragStart, invokeInlineEditorsDrop } from "@/builder/services/inline-editors";
import { postMessage } from "@/builder/services/post-message";
import { endDragging, hideDropMarker } from "@/builder/store/actions";
import { BuilderAction } from "@/builder/store/types";
import { MessageTypes } from "@/builder/types";

const ESC_KEY_CODE = 27;
let cancellableToken;

/**
 * Drag and drop event handlers factory.
 */
const makeDragAndDropHandlers = (dispatch: ThunkDispatch<State, any, BuilderAction>, getState: () => State) => ({
  makeOnDragStart: (startDraggingAction: (itemIdentifier: string, containerIdentifier: string) => ThunkAction<void, State, any, BuilderAction>) => (evt) => {
    dispatch(startDraggingAction(evt.source.id, evt.sourceContainer.id));

    setBrowserSpecificGrabbingCursor();

    // add DnD cancellation event listeners
    document.addEventListener("keyup", makeTriggerMouseUpOnESC(getState, dispatch));
    window.addEventListener("message", endDnDFromOtherFrame(getState, dispatch));

    const draggedElement = document.getElementById(evt.source.id);
    invokeInlineEditorsDragStart(draggedElement);

    postMessage(MessageTypes.MESSAGING_DRAG_START, null, "*");
  },

  makeOnDragStop: (moveItemAction) => () => {
    document.body.style.cursor = "auto";
    if (cancellableToken) {
      clearTimeout(cancellableToken);
    }

    // remove DnD cancellation event listeners
    document.removeEventListener("keyup", makeTriggerMouseUpOnESC(getState, dispatch));
    window.removeEventListener("message", endDnDFromOtherFrame(getState, dispatch));
    postMessage(MessageTypes.MESSAGING_DRAG_STOP, null, "*");

    const state = getState().dragAndDrop;

    // Check if dragging wasn't canceled by ESC key
    if (state.itemIdentifier) {
      const draggedElement = document.getElementById(state.itemIdentifier);
      invokeInlineEditorsDrop(draggedElement);

      if (state.dropMarkerPosition !== null && state.targetContainerIdentifier) {
        const targetPosition = (state.sourceContainerIdentifier === state.targetContainerIdentifier && state.originalPosition <= state.dropMarkerPosition)
          ? state.dropMarkerPosition - 1
          : state.dropMarkerPosition;

        dispatch(moveItemAction(state.itemIdentifier, state.sourceContainerIdentifier, state.targetContainerIdentifier, targetPosition));
      }

      dispatch(endDragging());
    }
  },

  onDragOver: (evt) => {
    const state = getState();

    if (evt.source.id === evt.over.id && state.dragAndDrop.dropMarkerPosition !== null) {
      const { top, bottom } = evt.over.getBoundingClientRect();
      const mouseY = evt.sensorEvent.clientY;

      // As widget header is inside the widget component the drag over is sometimes registered just by starting the drag on the header
      // therefore it is needed to check the dimensions of the mouse event and the widget's top/bottom offset
      if (mouseY >= top && mouseY <= bottom) {
        dispatch(hideDropMarker());
      } else if (cancellableToken) {
        // If the mouse event is outside of the widget the timeout to hide the drop marker is sometimes started
        // therefore it has to be cancelled
        clearTimeout(cancellableToken);
      }
    }

    if (evt.over.id !== state.dragAndDrop.itemIdentifier && cancellableToken) {
      clearTimeout(cancellableToken);
    }
  },

  onDragContainerOver: (evt) => {
    const state = getState();
    const containerId = evt.overContainer.id;
    const originalZoneId = state.dragAndDrop.sourceContainerIdentifier;

    if (cancellableToken && containerId !== originalZoneId) {
      clearTimeout(cancellableToken);
    }
  },

  onDragContainerOut: (evt) => {
    const overContainerId = evt.overContainer.id;
    const zoneId = getState().dragAndDrop.targetContainerIdentifier;

    if (overContainerId === zoneId) {
      cancellableToken = setTimeout(() => dispatch(hideDropMarker()), HIDE_ELEMENT_TIMEOUT);
    }
  },
});

/**
 * Sets the cursor style to 'grabbing hand'.
 */
const setBrowserSpecificGrabbingCursor = () => {
  document.body.style.cursor = "move";
  document.body.style.cursor = "grabbing";
  document.body.style.cursor = "-moz-grabbing";
  document.body.style.cursor = "-webkit-grabbing";
};

/**
 * ESC key up event handler factory.
 * @param getState Store's getState function.
 * @param dispatch Store's dispatch function.
 */
const makeTriggerMouseUpOnESC = (getState: () => State, dispatch: ThunkDispatch<State, any, BuilderAction>) => (evt) => {
  if (evt.which === ESC_KEY_CODE && getState().dragAndDrop.itemIdentifier) {
    endDnDWithFakeMouseUp(dispatch);
  }
};

/**
 * End DnD from parent frame event handler factory.
 * @param getState Store's getState function.
 * @param dispatch Store's dispatch function.
 */
const endDnDFromOtherFrame = (getState: () => State, dispatch: ThunkDispatch<State, any, BuilderAction>) => (evt) => {
  if (evt.data && evt.data.msg === MessageTypes.MESSAGING_DRAG_STOP && getState().dragAndDrop.itemIdentifier) {
    endDnDWithFakeMouseUp(dispatch);
  }
};

/**
 * End dnd with action dispatch and fake mouse up event dispatch.
 * @param dispatch Store's dispatch function.
 */
const endDnDWithFakeMouseUp = (dispatch: ThunkDispatch<State, any, BuilderAction>) => {
  dispatch(endDragging());
  document.dispatchEvent(new MouseEvent("mouseup", { bubbles: true, cancelable: true }));
};

export {
  makeDragAndDropHandlers,
};
