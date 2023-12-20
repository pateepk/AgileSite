import { ComponentMetadata, Metadata, NormalizedConfiguration, PopupListingElement } from "@/builder/declarations";

import { MessageTypes } from "../types";

/**
 * Defines an interface for normalizing and denormalizing data.
 */
export interface NormalizerService {
  normalizeConfiguration(configuration: any): NormalizedConfiguration;
  denormalizeConfiguration(configuration: NormalizedConfiguration): any;
  normalizeMetadata(metadata: any): Metadata;
}

/**
 * Defines an interface for localization.
 */
export interface LocalizationService {
  getLocalization(resourceString: string, ...parameters: any[]): string;
  getCultureCode(): string;
  getLocalizationWithoutPrefix(resourceString: string, ...parameters: any[]): string;
}

/**
 * Defines an interface for messaging service.
 */
export interface MessagingService {
  postMessage: (messageType: MessageTypes, data: any, targetOrigin: string) => void;
}

export interface IPopUpElementsService {
  getWidgetElements(metadata: ComponentMetadata[]): PopupListingElement[];
  getSectionElements(metadata: ComponentMetadata[]): PopupListingElement[];
}

export interface InlineEditorHandlers {
  readonly init: (editorName, propertyName, propertyValue, localizationService) => void;
  readonly destroy?: (editorName) => void;
  readonly dragStart?: (editorName) => void;
  readonly drop?: (editorName) => void;
}
