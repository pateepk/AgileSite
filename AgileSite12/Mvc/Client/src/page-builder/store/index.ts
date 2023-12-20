/**
 * Module responsible for the Page builder store configuration.
 * @module store
 */

import { applyMiddleware, createStore, Store } from "redux";
import thunk from "redux-thunk";

import { ThunkServices } from "@/builder/declarations";
import { getThunkServices } from "@/builder/services/thunk-services";
import { composeEnhancers } from "@/builder/store";
import { detectConfigurationChange } from "@/builder/store/middleware/configuration-state";
import { logErrors } from "@/builder/store/middleware/error-logger";
import { BuilderAction } from "@/builder/store/types";

import { PageBuilderState } from "@/page-builder/declarations";
import { pageBuilderReducers } from "@/page-builder/store/reducers";

/**
 * Returns an instance of a redux store for Page builder.
 * @param initialState Initial state of the store.
 */
export const configureStore = (initialState: PageBuilderState, developmentMode: boolean): Store<PageBuilderState> => {
  const enhancers = composeEnhancers(developmentMode);

  return createStore<PageBuilderState, BuilderAction, {}, {}>(pageBuilderReducers, initialState, enhancers(
    applyMiddleware(thunk.withExtraArgument<ThunkServices>(getThunkServices()), logErrors, detectConfigurationChange),
  ));
};
