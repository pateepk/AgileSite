import { Entities, PersonalizationConditionTypeMetadata } from "@/builder/declarations";

export interface WidgetHeaderComponentContext {
  widgetTitle: string;
  widgetIdentifier: string;
  widgetTopOffset: number;
}

export interface WidgetHeaderComponentState {
  showWidgetHeader: boolean;
  personalizationConditionTypes: Entities<PersonalizationConditionTypeMetadata>;
  showPropertiesButton: boolean;
}

export interface WidgetHeaderComponentProperties extends WidgetHeaderComponentContext, WidgetHeaderComponentState {
}
