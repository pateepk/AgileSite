<template>
  <BorderContainer
    :id="zoneIdentifier"
    :class="widgetZoneClasses">
    <div class="ktc-admin-ui">
      <DropZone v-if="isEmpty" v-show="isDraggedItemWidget && !isBanned"
      class="ktc-dropzone--full"
      :class="{ 'ktc-dropzone--active': dragAndDrop.targetContainerIdentifier === zoneIdentifier }"
      :position="0"
      :containerIdentifier="zoneIdentifier" />
      <kentico-drop-marker v-if="showZoneDropMarker" />
    </div>
    <Widget v-for="(widget, index) in widgetsInZone"
      :id="widget.identifier"
      :key="widget.identifier"
      :widgetIdentifier="widget.identifier"
      :zoneIdentifier="zoneIdentifier"
      :areaIdentifier="areaIdentifier"
      :position="index"
      :isDropBanned="isBanned"
      :friendlyElementIds="friendlyElementIds"
      :availableWidgetTypes="availableWidgetTypes"
      @removeWidget="onRemoveWidget"
    >
      <template slot="widgetHeader" slot-scope="{ widgetTitle, widgetIdentifier, widgetTopOffset }">
        <slot name="widgetHeader"
          :widgetTitle="widgetTitle"
          :widgetIdentifier="widgetIdentifier"
          :widgetTopOffset="widgetTopOffset" />
      </template>
    </Widget>
    <div class="ktc-admin-ui">
      <kentico-add-component-button ref="addWidget"
        :is-active="isWidgetListOpen"
        :tooltip.prop="addTooltip"
        :primary="true"
        v-on-clickaway="clickedAway"
        v-if="isEmpty"
        v-show="!dragAndDrop.itemIdentifier"
        @openComponentList="onOpenComponentList"
      />
    </div>
  </BorderContainer>
</template>

<script lang="ts">
import { mixin as clickaway } from "vue-clickaway";
import { Component, Prop } from "vue-property-decorator";

import { builderConfig } from "@/builder/api/client";
import { BorderContainer, DropZone } from "@/builder/components";
import * as d from "@/builder/declarations";
import { EntityType } from "@/builder/EntityType";
import { getPosition } from "@/builder/helpers/position";
import { isAreaBanned } from "@/builder/helpers/widget-restrictions";
import { PopupType } from "@/builder/store/types";

import Widget from "./WidgetProvider";
import { WidgetZoneBase } from "./WidgetZoneBase";

@Component({
  components: {
    BorderContainer,
    Widget,
    DropZone,
  },
  mixins: [clickaway],
})
export default class WidgetZone extends WidgetZoneBase implements d.WidgetZoneComponentProperties {

  // state props
  @Prop() zoneIdentifier: string;
  @Prop() dragAndDrop: d.DragAndDrop;
  @Prop() zone: d.WidgetZone;
  @Prop() popup: d.Popup;
  @Prop() widgetsInZone: d.Widget[];
  @Prop() availableWidgetTypes: d.PopupListingElement[];

  // actions
  @Prop() removeWidget;
  @Prop() closePopup: () => void;
  @Prop() openPopup;

  get addTooltip() {
    return this.$_localizationService.getLocalization("widget.addTooltip");
  }

  get widgetZoneClasses() {
    return {
      "ktc-widget-zone": true,
      "ktc-widget-zone--empty": this.isEmpty,
      "ktc-widget-zone--highlighted": this.dragAndDrop.itemIdentifier !== null,
    };
  }

  get isEmpty() {
    return this.widgetsInZone.length === 0;
  }

  get isWidgetListOpen() {
    return this.popup.componentIdentifier === this.zoneIdentifier;
  }

  get showZoneDropMarker() {
    return (
      !this.isEmpty &&
      this.dragAndDrop.targetContainerIdentifier === this.zoneIdentifier &&
      this.dragAndDrop.dropMarkerPosition === 0
    );
  }

  get isBanned() {
    return isAreaBanned(this.dragAndDrop.bannedContainers, this.areaIdentifier);
  }

  get isDraggedItemWidget() {
    return this.dragAndDrop.entity === EntityType.Widget;
  }

  get friendlyElementIds() {
    return [(builderConfig as any).propertiesEditorClientId];
  }

  get headerTitle() {
    return this.$_localizationService.getLocalization("widget.headerTitle");
  }

  onRemoveWidget(event) {
    const message = this.$_localizationService.getLocalization("widget.remove.confirmation");

    if (confirm(message)) {
      this.removeWidget(event, this.zoneIdentifier);
    }
  }

  onOpenComponentList({ detail }: CustomEvent): void {
    // Close widget list after clicking on the same addButton 2x
    if (this.isWidgetListOpen) {
      this.closePopup();
      return;
    }
    this.openPopup(
      this.zoneIdentifier,
      getPosition(detail),
      PopupType.AddWidget,
      detail,
      this.areaIdentifier,
      this.zoneIdentifier,
    );
  }

  clickedAway() {
    if (this.isWidgetListOpen) {
      this.closePopup();
    }
  }
}
</script>

<style lang="less" scoped>
.ktc-section {
  &.ktc-section--dragged,
  &.ktc-draggable-mirror {
    kentico-widget-header,
    kentico-add-component-button {
      display: none !important;
    }
  }
}

.ktc-widget-zone {
  position: relative;
  min-width: 100px;

  // To disable default draggable border
  &:focus {
    outline: 0;
  }

  &.ktc-widget-zone--empty {
    height: 100px;

    kentico-add-component-button {
      &[active="true"] {
        z-index: @widget-zone-add-component;
      }

      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
    }
  }

  kentico-drop-marker {
    position: absolute;
    width: 100%;
  }
}
</style>
