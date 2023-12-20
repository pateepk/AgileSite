import { createAction } from "typesafe-actions";

/**
 * 'Update page template properties' action creator.
 */
export const updateTemplateProperties = createAction("pageTemplates/UPDATE_PROPERTIES", (resolve) =>
  (identifier: string, properties: object) => resolve({
    identifier,
    properties,
  }),
);
