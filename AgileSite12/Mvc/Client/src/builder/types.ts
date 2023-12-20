/**
 * Defines types of services for IoC.
 */
export const SERVICE_TYPES = {
  LocalizationService: Symbol.for("LocalizationService"),
  NormalizerService: Symbol.for("NormalizerService"),
  PageConfigurationService: Symbol.for("PageConfigurationService"),
  PopUpElementsService: Symbol.for("PopUpElementsService"),
  MessagingService: Symbol.for("MessagingService"),
};

export enum MessageTypes {
  SAVE_CONFIGURATION = "Kentico.SaveConfiguration",
  LOAD_DISPLAYED_WIDGET_VARIANTS = "Kentico.LoadDisplayedWidgetVariants",
  GET_DISPLAYED_WIDGET_VARIANTS = "Kentico.GetDisplayedWidgetVariants",
  CONFIGURATION_STORED = "Kentico.ConfigurationStored",
  CONFIGURATION_CHANGED = "Kentico.ConfigurationChanged",
  MESSAGING_ERROR = "Kentico.Messaging.Error",
  MESSAGING_EXCEPTION = "Kentico.Messaging.Exception",
  MESSAGING_WARNING = "Kentico.Messaging.Warning",
  MESSAGING_DRAG_START = "Kentico.Messaging.DragStart",
  MESSAGING_DRAG_STOP = "Kentico.Messaging.DragStop",
  OPEN_MODAL_DIALOG = "Kentico.OpenModalDialog",
  CLOSE_MODAL_DIALOG = "Kentico.CloseModalDialog",
  SAVE_TEMP_CONFIGURATION = "Kentico.SaveTempConfiguration",
  TEMP_CONFIGURATION_STORED = "Kentico.TempConfigurationStored",
  CHANGE_TEMPLATE = "Kentico.ChangeTemplate",
  CANCEL_SCREENLOCK = "Kentico.CancelScreenLock",
}

/**
 * Defines color themes used in the builder.
 */
export enum Theme {
  Widget = "widget",
  Section = "section",
  Template = "template",
}

export enum ButtonType {
  Properties = "properties",
  Change = "change",
  Personalization = "personalisation",
  Delete = "delete",
  Move = "move",
}
