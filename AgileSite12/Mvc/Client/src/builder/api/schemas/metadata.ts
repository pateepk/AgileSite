import { schema } from "normalizr";

const widget = new schema.Entity("widgets", {}, {
  idAttribute: "typeIdentifier",
});

const section = new schema.Entity("sections", {}, {
  idAttribute: "typeIdentifier",
});

const pageTemplate = new schema.Entity("pageTemplates", {}, {
  idAttribute: "typeIdentifier",
});

const personalizationConditionType = new schema.Entity("personalizationConditionTypes", {}, {
  idAttribute: "typeIdentifier",
});

const metadataSchema = {
  pageTemplates: [pageTemplate],
  personalizationConditionTypes: [personalizationConditionType],
  sections: [section],
  widgets: [widget],
};

export {
  metadataSchema,
};
