import queryString from "query-string";

import { BuilderMode } from "@/builder/BuilderModeEnum";

const queryStrings = queryString.parse(location.search);

/**
 * Defines the context in which the builder is run.
 * Builder can be in page builder mode or form builder mode.
 * If script is not executed in the browser then general builder mode is used.
 */
const builderMode: BuilderMode = queryStrings.builder === "formBuilder" ? BuilderMode.FormBuilder : BuilderMode.PageBuilder;

export const builderContext = {
  builderMode,
};
