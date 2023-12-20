import { combineReducers } from "redux";

import { baseReducers } from "@/builder/store/reducers";

import { PageBuilderConfiguration } from "../declarations";
import { highlightedSection } from "./highlighted-section/reducer";
import { modalDialogs } from "./modal-dialogs/reducer";
import { pageTemplate } from "./page-template/reducer";
import { popup } from "./pop-up/reducer";
import { widgets } from "./widgets/reducer";

const pageBuilderReducers = combineReducers({
  ...baseReducers,
  config: (state: PageBuilderConfiguration = null) => state,
  pageTemplate,
  popup,
  modalDialogs,
  widgets,
  highlightedSection,
});

export {
  pageBuilderReducers,
};
