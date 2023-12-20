import { EditableArea, Entities, PageTemplate, Section, Widget, WidgetVariant, WidgetZone } from "@/builder/declarations";

interface DenormalizedWidgetVariant {
  readonly identifier: string;
  readonly name: string;
  readonly properties?: object;
}

interface DenormalizedWidget {
  readonly identifier: string;
  readonly type: string;
  readonly conditionType: string;
  readonly variants: DenormalizedWidgetVariant[];
}

interface DenormalizedWidgetZone {
  readonly identifier: string;
  readonly widgets: DenormalizedWidget[];
}

interface DenormalizedSection {
  readonly identifier: string;
  readonly type: string;
  readonly zones: DenormalizedWidgetZone[];
}

interface DenormalizedEditableArea {
  readonly identifier: string;
  readonly sections: DenormalizedSection[];
}

interface DenormalizedTemplate {
  readonly identifier: string;
}

export interface DenormalizedEditableAreasConfiguration {
  readonly editableAreas: DenormalizedEditableArea[];
}

export interface DenormalizedPageBuilderConfiguration {
  readonly page: DenormalizedEditableAreasConfiguration;
  readonly pageTemplate?: DenormalizedTemplate;
}

export interface NormalizedConfiguration {
  readonly editableAreas: Entities<EditableArea>;
  readonly sections: Entities<Section>;
  readonly pageTemplate?: PageTemplate;
  readonly zones: Entities<WidgetZone>;
  readonly widgets: Entities<Widget>;
  readonly widgetVariants: Entities<WidgetVariant>;
}

/**
 * Provides interface for configuration endpoints.
 */
export interface ConfigurationEndpoints {
  readonly load: string;
  readonly store: string;
}

export interface PropertiesValidationResponseData {
  status: string;
  data: any;
}
