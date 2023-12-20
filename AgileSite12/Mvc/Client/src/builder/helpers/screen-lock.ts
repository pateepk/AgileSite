import { postMessage } from "../services/post-message";
import { MessageTypes } from "../types";

const postCancelScreenLock = () => {
  setTimeout(() => postMessage(MessageTypes.CANCEL_SCREENLOCK, null, "*"), 1000);
};

export const addCancelScreenLockEventListeners = (element: HTMLElement = document.documentElement) => {
  element.addEventListener("mousedown", postCancelScreenLock, true);
  element.addEventListener("keydown", postCancelScreenLock, true);
};

export const removeCancelScreenLockEventListeners = (element: HTMLElement = document.documentElement) => {
  element.removeEventListener("mousedown", postCancelScreenLock, true);
  element.removeEventListener("keydown", postCancelScreenLock, true);
};
