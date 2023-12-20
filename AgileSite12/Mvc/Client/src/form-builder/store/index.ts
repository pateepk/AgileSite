/**
 * Module responsible for the Form builder store configuration.
 * @module store
 */

import { applyMiddleware, createStore, Store } from "redux";
import thunk from "redux-thunk";

import { ThunkServices } from "@/builder/declarations";
import { getThunkServices } from "@/builder/services/thunk-services";
import { composeEnhancers } from "@/builder/store";
import { detectConfigurationChange } from "@/builder/store/middleware/configuration-state";
import { logErrors } from "@/builder/store/middleware/error-logger";

import { FormBuilderState } from "@/form-builder/declarations";
import { formBuilderReducers } from "@/form-builder/store/reducers";
import { FormBuilderAction } from "@/form-builder/store/types";

/**
 * Returns an instance of a redux store for Form builder.
 * @param initialState Initial state of the store.
 */
export const configureStore = (initialState: FormBuilderState, developmentMode: boolean): Store<FormBuilderState> => {
  const enhancers = composeEnhancers(developmentMode);

  return createStore<FormBuilderState, FormBuilderAction, {}, {}>(formBuilderReducers, initialState, enhancers(
    applyMiddleware(thunk.withExtraArgument<ThunkServices>(getThunkServices()), logErrors, detectConfigurationChange),
  ));
};
