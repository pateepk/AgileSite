import { Action } from "redux";

import { BuilderConfiguration, ThunkAction } from "@/builder/declarations";
import { PageBuilderAction, PageBuilderState } from "@/page-builder/store/types";
import { FeatureSet, PageTemplateConfig } from "./config";

export interface PageBuilderConfiguration extends BuilderConfiguration {
  readonly pageIdentifier: number;
  readonly featureSet: FeatureSet;
  readonly pageTemplate: PageTemplateConfig;
}

/**
 * Defines an interface page builder thunk action.
 */
export interface ThunkAction<R = void, S = PageBuilderState, A extends Action = PageBuilderAction>
  extends ThunkAction<R, S, A> {
}

export {
  PageBuilderState,
};
