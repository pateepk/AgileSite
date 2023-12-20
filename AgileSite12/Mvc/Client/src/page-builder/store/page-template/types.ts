import { ActionType } from "typesafe-actions";

import * as actions from "./actions";

export type UpdateTemplatePropertiesActionCreator = typeof actions.updateTemplateProperties;
export type UpdateTemplatePropertiesAction = ActionType<UpdateTemplatePropertiesActionCreator>;
export type PageTemplateAction = ActionType<typeof actions>;

import { setPageTemplateProperties } from "./thunks";

export type SetPageTemplatePropertiesThunk = typeof setPageTemplateProperties;
