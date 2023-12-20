import { StateType } from "typesafe-actions";

import { BuilderAction } from "@/builder/store/types";

import { ModalDialogsAction } from "./modal-dialogs/types";
import { PageTemplateAction } from "./page-template/types";
import { pageBuilderReducers } from "./reducers";
import { WidgetVariantAction } from "./widget-variants/types";

export { SetSectionPropertiesThunk } from "./sections/types";
export { SetWidgetPropertiesThunk } from "./widgets/types";
export { SetPageTemplatePropertiesThunk } from "./page-template/types";

export type PageBuilderAction =
  | BuilderAction
  | ModalDialogsAction
  | WidgetVariantAction
  | PageTemplateAction;

export type PageBuilderState = Readonly<StateType<typeof pageBuilderReducers>>;
