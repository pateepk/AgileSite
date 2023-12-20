export interface WidgetHeaderComponentContext {
  widgetTitle: string;
  widgetIdentifier: string;
}

export interface WidgetHeaderComponentState {
  showWidgetHeader: boolean;
  activeItemIdentifier: string;
}

export interface WidgetHeaderComponentProperties extends WidgetHeaderComponentContext, WidgetHeaderComponentState {
}
