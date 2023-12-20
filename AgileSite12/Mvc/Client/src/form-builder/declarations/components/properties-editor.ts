import * as d from "@/builder/declarations";

export interface PropertiesEditorComponentState {
  validationRulesMetadataPerType: any;
  widgetMetadata: d.Entities<d.WidgetMetadata>;
  widgetVariants: d.Entities<d.WidgetVariant>;
  widgets: d.Entities<d.Widget>;
  widgetSelection: d.WidgetSelection;
  savingInProgress: boolean;
}

export interface PropertiesEditorComponentActions {
  expandValidationRule;
  freezeWidgetSelection;
  newValidationRuleAdded;
  setWidgetProperties;
  thawWidgetSelection;
  disableWidgetClickAway;
}

export interface PropertiesEditorComponentProperties extends PropertiesEditorComponentState, PropertiesEditorComponentActions {
}
