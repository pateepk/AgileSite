export interface SectionHeaderExtraButtonsContext {
  readonly sectionIdentifier: string;
}

export interface SectionHeaderExtraButtonsState {
  readonly showPropertiesButton: boolean;
}

export interface SectionHeaderExtraButtonsComponentProperties extends SectionHeaderExtraButtonsContext, SectionHeaderExtraButtonsState { }
