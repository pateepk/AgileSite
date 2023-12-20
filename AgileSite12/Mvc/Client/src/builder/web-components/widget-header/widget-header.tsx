import { Component, Event, EventEmitter, Prop } from "@stencil/core";

import { LocalizationService } from "@/builder/declarations/index";
import { ButtonType } from "@/builder/types";

@Component({
  tag: "kentico-widget-header",
  styleUrl: "widget-header.less",
  shadow: false,
})
export class WidgetHeader {

  @Prop() localizationService: LocalizationService;
  @Prop() widgetTitle: string;
  @Prop({ context: "getString" }) getString: any;

  @Event() removeWidget: EventEmitter;

  get dragTooltip() {
    return this.getString(this.localizationService, "widget.dragTooltip");
  }

  get deleteTooltip() {
    return this.getString(this.localizationService, "widget.deleteTooltip");
  }

  removeWidgetHandler = (event: UIEvent) => {
    event.stopPropagation();
    this.removeWidget.emit();
  }

  render() {
    // preventing default behavior on mousedown when dragging, fixes unwanted highlighting of elements when dragging widget in IE11
    return (
      <div class="ktc-widget-header">
        <div class="ktc-widget-header-panel-left ktc-widget-header-handle ktc-widget-header-icon" button-type={ButtonType.Move} onMouseDown={(e) => e.preventDefault()}>
          <i aria-hidden="true" title={this.dragTooltip} class="icon-dots-vertical"></i>
        </div>
        <div class="ktc-widget-header-panel-title ktc-widget-header-handle" onMouseDown={(e) => e.preventDefault()}>
          <div class="ktc-widget-header-title-text" title={this.widgetTitle}>
            {this.widgetTitle}
          </div>
        </div>
        <div class="ktc-widget-header-panel-right">
          <slot name="extraHeaderButtons" />
          <a class="ktc-widget-header-icon" button-type={ButtonType.Delete}>
            <i aria-hidden="true" title={this.deleteTooltip} class="icon-bin" onClick={this.removeWidgetHandler}></i>
          </a>
        </div>
      </div>
    );
  }
}
