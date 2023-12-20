import { Action, Dispatch, Middleware } from "redux";
import { ThunkAction as ReduxThunkAction, ThunkDispatch as ReduxThunkDispatch } from "redux-thunk";

import { BuilderConstants, ConfigurationEndpoints, LocalizationService, Logger, MessagingService, SelectorsConfig } from "@/builder/declarations";
import { EntityType } from "@/builder/EntityType";
import { BuilderAction, ComponentPosition, ListingOffset, ModalDialogType, PopupType } from "@/builder/store/types";
import { Theme } from "@/builder/types";

import { State } from "../store/types";

export interface Entities<T> {
  [index: string]: T;
}

export interface BuilderConfiguration {
  readonly applicationPath: string;
  readonly configurationEndpoints: ConfigurationEndpoints;
  readonly metadataEndpoint: string;
  readonly allowedOrigins: string[];
  readonly constants: BuilderConstants;
  readonly selectors: SelectorsConfig;
}

export interface Popup {
  readonly componentIdentifier: Nullable<string>;
  readonly position: Nullable<ComponentPosition>;
  readonly popupType: Nullable<PopupType>;
  readonly listingOffset: Nullable<ListingOffset>;
  readonly areaIdentifier: Nullable<string>;
  readonly zoneIdentifier: Nullable<string>;
}

export interface DragAndDrop {
  readonly entity: Nullable<EntityType>;
  readonly typeIdentifier: Nullable<string>;
  readonly itemIdentifier: Nullable<string>;
  readonly originalPosition: Nullable<number>;
  readonly sourceContainerIdentifier: Nullable<string>;
  readonly targetContainerIdentifier: Nullable<string>;
  readonly dropMarkerPosition: Nullable<number>;
  readonly bannedContainers: string[];
}

export interface Markups {
  readonly sections: Entities<SectionMarkup>;
  readonly variants: Entities<WidgetVariantMarkup>;
}

export interface AreaConfiguration {
  readonly defaultSection: string;
  readonly widgetRestrictions: string[];
}

export interface WidgetVariantMarkup {
  readonly markup: string;
  readonly isDirty: boolean;
}

export interface SectionMarkup {
  readonly markup: string;
}

export interface Metadata {
  readonly widgets: Entities<WidgetMetadata>;
  readonly sections: Entities<SectionMetadata>;
  readonly pageTemplates: Entities<PageTemplateMetadata>;
  readonly personalizationConditionTypes: Entities<PersonalizationConditionTypeMetadata>;
}

export interface WidgetMetadata extends ComponentMetadata {
  readonly defaultPropertiesUrl: string;
  readonly hasProperties: boolean;
  readonly propertiesFormMarkupUrl: string;
  readonly hasEditableProperties: boolean;
}

export interface SectionMetadata extends ComponentMetadata {
  readonly defaultPropertiesUrl: string | undefined;
  readonly hasProperties: boolean | undefined;
  readonly propertiesFormMarkupUrl: string | undefined;
}

export interface PageTemplateMetadata extends ComponentMetadataBase {
  readonly defaultPropertiesUrl: string | undefined;
  readonly hasProperties: boolean | undefined;
  readonly propertiesFormMarkupUrl: string | undefined;
}

export interface PersonalizationConditionTypeMetadata extends ComponentMetadata {
  readonly hint: Nullable<string>;
}

interface ComponentMetadataBase {
  readonly typeIdentifier: string;
  readonly name: string;
  readonly description: Nullable<string>;
  readonly iconClass: Nullable<string>;
}

export interface ComponentMetadata extends ComponentMetadataBase {
  readonly markupUrl: string;
}

export interface HighlightedSection {
  readonly sectionIdentifier: Nullable<string>;
  readonly headerPosition: Nullable<ComponentPosition>;
  readonly highlightBorder: boolean;
  readonly freezeHighlight: boolean;
}

/**
 * Defines an interface for modal dialog state.
 */
export interface ModalDialogsState {
  readonly dialogs: ModalDialog[];
  readonly theme: Theme;
}

/**
 * Defines an interface for modal dialog.
 */
export interface ModalDialog {
  readonly identifier: string;
  readonly index: number;
  readonly type: ModalDialogType;
  readonly markup: string;
  readonly isValid: boolean;
  readonly title: Nullable<string>;
  readonly showFooter: boolean;
  readonly applyButtonText: string;
  readonly cancelButtonText: string;
  readonly markupUrl?: string;
  readonly model?: object;
  readonly width?: string;
  readonly maximized?: boolean;
}

export interface Middleware extends Middleware<{}, State, Dispatch> {
}

export interface ThunkServices {
  readonly logger: Logger;
  readonly localizationService: LocalizationService;
  readonly messaging: MessagingService;
}

export interface ThunkAction<R = void, S = State, A extends Action = BuilderAction>
  extends ReduxThunkAction<R, S, ThunkServices, A> { }

export interface ThunkDispatch extends ReduxThunkDispatch<State, ThunkServices, BuilderAction> { }

export {
  State,
};
