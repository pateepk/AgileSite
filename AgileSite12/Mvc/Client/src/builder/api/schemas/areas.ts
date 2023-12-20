import { schema } from "normalizr";

const widgetVariant = new schema.Entity("widgetVariants", {}, {
  idAttribute: "identifier",
});
const widget = new schema.Entity("widgets", { variants: [widgetVariant]}, {
  idAttribute: "identifier",
});
const zone = new schema.Entity("zones", { widgets: [widget] }, {
  idAttribute: "identifier",
});
const section = new schema.Entity("sections", { zones: [zone]}, {
  idAttribute: "identifier",
});
const editableArea = new schema.Entity("editableAreas", { sections: [section] }, {
   idAttribute: "identifier",
});

const areasSchema = {
  editableAreas: [editableArea],
};

export {
  areasSchema,
};
