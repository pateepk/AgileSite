interface FormComponent {
  readonly identifier: string;
  readonly type: string;
  readonly conditionType: string;
  properties?: object;
}

interface DenormalizedFormZone {
  readonly identifier: string;
  readonly formComponents: FormComponent[];
}

interface DenormalizedSection {
  readonly identifier: string;
  readonly type: string;
  readonly zones: DenormalizedFormZone[];
}

export interface DenormalizedEditableArea {
  readonly identifier: string;
  readonly sections: DenormalizedSection[];
}

export interface DenormalizedFormConfiguration {
  readonly editableAreas: DenormalizedEditableArea[];
}
