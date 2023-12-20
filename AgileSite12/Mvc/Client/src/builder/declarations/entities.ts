export interface Widget {
  readonly identifier: string;
  readonly type: string;
  readonly conditionType?: string;
  readonly variants: string[];
}

export interface WidgetVariant {
  readonly identifier: string;
  readonly name?: string;
  readonly properties?: object;
  readonly conditionTypeParameters?: object;
}

export interface WidgetZone {
  readonly identifier: string;
  readonly widgets: string[];
}

export interface Section {
  readonly identifier: string;
  readonly type: string;
  readonly properties?: object;
  readonly zones: string[];
}

export interface PageTemplate {
  readonly identifier: string;
  readonly configurationIdentifier?: string;
  readonly properties?: object;
}

export interface EditableArea {
  readonly identifier: string;
  readonly sections: string[];
}

export interface WidgetSelection {
  readonly identifier?: string;
  readonly freezeSelection: boolean;
  readonly preventClickAway: boolean;
}
