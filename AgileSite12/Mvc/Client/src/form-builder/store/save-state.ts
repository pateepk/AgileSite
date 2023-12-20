import { ThunkDispatch } from "redux-thunk";

import { saveState } from "@/builder/api/index";
import { State, ThunkServices } from "@/builder/declarations";
import { postMessage } from "@/builder/services/post-message";
import { MessageTypes } from "@/builder/types";
import { showSavedMessage, showSavingMessage } from "@/form-builder/store/actions";

import { performApiCall } from "../api/index";
import { FormBuilderAction } from "./types";

const queue: Array<Promise<any>> = [];

/**
 * Executes a given function with respect to whether a previous invocation has completed. If the previous invocation is pending
 * then the function is planned to be executed once the previous is finished.
 * @param userFunction Function to be executed.
 */
const runWithSemaphore = async (userFunction: () => Promise<any>): Promise<void> => {
  let res;
  let rej;

  const lastPromise = queue[queue.length - 1];
  const promise = new Promise((resolve, reject) => { res = resolve; rej = reject; });
  queue.push(promise);

  if (lastPromise) {
    try {
      await lastPromise;
    // tslint:disable-next-line:no-empty
    } catch (v) { }
  }

  try {
    const userFunctionResult = await userFunction();
    res();
    return userFunctionResult;
  } catch (value) {
    rej();
    return Promise.reject(value);
  } finally {
    queue.splice(0, 1);
  }
};

/**
 * Dispatches show saving and show saved messages before and after builder saveState execution, respectively.
 * @param dispatch Used to dispatch events.
 * @param state State of the page.
 * @param formId Form ID.
 */
const formBuilderSaveState = async (dispatch: ThunkDispatch<State, ThunkServices, FormBuilderAction>, state: State, formId: string): Promise<any> => {
  dispatch(showSavingMessage());
  await runWithSemaphore(() => performApiCall(async () => saveState(state, formId)));
  postMessage(MessageTypes.CONFIGURATION_CHANGED, null, "*");
  dispatch(showSavedMessage());
};

export {
  formBuilderSaveState,
  runWithSemaphore,
};
