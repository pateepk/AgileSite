<template>
  <BorderContainer v-on-clickaway="onWidgetClickAway"
    :class="widgetClasses"
    :highlighted="isHighlighted"
    :selected="isSelected"
    :hideVerticalBorder="true"
    :primary="true"
    @click.native="onWidgetClick"
    @mouseover.native="onMouseOver"
    @mouseleave.native="onMouseLeave"
    @updateProperty.native.capture="onUpdateProperty"
  >
    <div class="ktc-admin-ui">
      <div class="ktc-widget-header-wrapper" @removeWidget="$emit($event.type, widgetIdentifier)">
        <slot name="widgetHeader"
          :widgetTitle="widgetTitle"
          :widgetIdentifier="widgetIdentifier"
          :widgetTopOffset="topOffset"
        />
      </div>
      <DropZone v-if="showDropZone" :containerIdentifier="zoneIdentifier" :position="position" :isBottom="false" class="ktc-dropzone--top" />
      <DropZone v-if="showDropZone" :containerIdentifier="zoneIdentifier" :position="position" :isBottom="true" class="ktc-dropzone--bottom" />
    </div>
    <WidgetMarkup
      :markup="displayedVariantMarkup"
      :variantIdentifier="displayedVariant.identifier"
      @markupUpdated="onMarkupUpdated" />
    <div class="ktc-admin-ui">
      <kentico-add-component-button is-small="true" ref="addWidget"
        :is-active="isWidgetListOpen"
        :tooltip.prop="addTooltip"
        :primary="true"
        v-show="!dragAndDrop.itemIdentifier"
        v-on-clickaway="clickedAway"
        @openComponentList="onOpenComponentList"
      />
      <kentico-drop-marker v-if="showWidgetDropMarker" />
    </div>
  </BorderContainer>
</template>

<script lang="ts">
import { mixin as clickaway } from "vue-clickaway";
import { Component, Prop, Vue } from "vue-property-decorator";

import { BorderContainer, DropZone } from "@/builder/components";
import * as d from "@/builder/declarations";
import { EntityType } from "@/builder/EntityType";
import { getPosition } from "@/builder/helpers/position";
import { initializeInlineEditors } from "@/builder/services/inline-editors";
import { PopupType } from "@/builder/store/types";

import WidgetMarkup from "./WidgetMarkup.vue";

@Component({
  components: {
    BorderContainer,
    WidgetMarkup,
    DropZone,
  },
  mixins: [clickaway],
})
export default class Widget extends Vue implements d.WidgetComponentProperties {
  topOffset = 0;

  @Prop({ required: true })
  widgetIdentifier: string;

  @Prop({ required: true })
  position: number;

  @Prop({ required: true })
  zoneIdentifier: string;

  @Prop({ default: false })
  isDropBanned: boolean;

  @Prop({ required: true })
  areaIdentifier: string;

  @Prop({ default: [] })
  friendlyElementIds: string[];

  @Prop({ default: [] })
  availableWidgetTypes: d.PopupListingElement[];

  // state props
  @Prop() dragAndDrop: d.DragAndDrop;
  @Prop() selectedWidgetIdentifier: string;
  @Prop() highlightedWidgetIdentifier: string;
  @Prop() popup: d.Popup;
  @Prop() widget: d.Widget;
  @Prop() displayedVariantMarkup: string;
  @Prop() displayedVariant: d.WidgetVariant;
  @Prop() widgetTitle: string;
  @Prop() isFrozen: boolean;
  @Prop() isClickAwayDisabled: boolean;

  // actions
  @Prop() highlightWidget;
  @Prop() dehighlightWidget;
  @Prop() selectWidget;
  @Prop() unselectWidget;
  @Prop() enableWidgetClickAway;
  @Prop() closePopup: () => void;
  @Prop() openPopup;
  @Prop() setWidgetProperty;

  get addTooltip(): string {
    return this.$_localizationService.getLocalization("widget.addTooltip");
  }

  get widgetClasses(): object {
    return {
      "ktc-widget": true,
      "ktc-widget--highlighted": this.isHighlighted,
      "ktc-widget--selected": this.isSelected,
      "ktc-widget--dragged": this.isDragged,
    };
  }

  get isDragged(): boolean {
    return this.dragAndDrop.itemIdentifier === this.widgetIdentifier;
  }

  get isSelected(): boolean {
    return this.selectedWidgetIdentifier === this.widgetIdentifier;
  }

  get isHighlighted(): boolean {
    return (
      this.highlightedWidgetIdentifier === this.widgetIdentifier &&
      !this.popup.componentIdentifier &&
      (!this.dragAndDrop.itemIdentifier &&
      !this.selectedWidgetIdentifier ||
      this.dragAndDrop.entity === EntityType.Widget)
    );
  }

  get isWidgetListOpen(): boolean {
    return this.widgetIdentifier === this.popup.componentIdentifier && this.popup.popupType === PopupType.AddWidget;
  }

  get showDropZone(): boolean {
    return (
      this.dragAndDrop.entity === EntityType.Widget &&
      this.dragAndDrop.itemIdentifier !== this.widgetIdentifier &&
      !this.isDropBanned
    );
  }

  get showWidgetDropMarker(): boolean {
    return (
      this.dragAndDrop.targetContainerIdentifier === this.zoneIdentifier &&
      this.dragAndDrop.dropMarkerPosition === this.position + 1
    );
  }

  mounted() {
    this.calculateTopOffset();
  }

  updated() {
    this.calculateTopOffset();
  }

  calculateTopOffset() {
    this.topOffset = this.$el.getBoundingClientRect().top + window.pageYOffset;
  }

  onMouseOver(): void {
    if (
      !this.isHighlighted &&
      this.popup.componentIdentifier === null &&
      (!this.dragAndDrop.itemIdentifier &&
      this.selectedWidgetIdentifier === null ||
      this.dragAndDrop.entity === EntityType.Widget)
    ) {
      this.highlightWidget(this.widgetIdentifier);
    }
  }

  onMouseLeave(): void {
    if (
      this.popup.componentIdentifier === null &&
      !this.dragAndDrop.itemIdentifier &&
      this.selectedWidgetIdentifier === null
    ) {
      this.dehighlightWidget(this.widgetIdentifier);
    }
  }

  onWidgetClick(): void {
    if (!this.isFrozen && this.widgetIdentifier !== this.selectedWidgetIdentifier) {
      this.selectWidget(this.widgetIdentifier);
    }
  }

  onWidgetClickAway(e) {
    // Component was not updated after the store was updated so delegate unselection to the next tick
    this.$nextTick(() => {
      // In browsers that don't support path property on MouseEvent, manual target property traverse is needed
      let targetElementPath = [];
      let currentElem = e.target;
      if (!e.path) {
        while (currentElem) {
          targetElementPath.push(currentElem);
          currentElem = currentElem.parentElement;
        }
      } else {
        targetElementPath = e.path;
      }

      const isCurrentWidgetSelected: boolean = this.widgetIdentifier === this.selectedWidgetIdentifier;

      const selectedElementWithAncestors: Node[] = targetElementPath.filter((el) => el instanceof Node);

      if (!this.isFrozen && !this.isClickAwayDisabled &&
          isCurrentWidgetSelected &&
          !selectedElementWithAncestors.some((el) => this.isFriendlyElementSelected(el))) {
        this.unselectWidget();
      }

      if (this.isClickAwayDisabled && isCurrentWidgetSelected) {
        this.enableWidgetClickAway();
      }
    });
  }

  isFriendlyElementSelected(selectedElement: Node): boolean {
    return this.friendlyElementIds.some((friendlyElementId) => {
      const friendlyElement = document.getElementById(friendlyElementId);
      return friendlyElement && (friendlyElement === selectedElement || friendlyElement.contains(selectedElement));
    });
  }

  onOpenComponentList({ detail }: CustomEvent): void {
    // Close widget list after clicking on the same addButton 2x
    if (this.isWidgetListOpen) {
      this.closePopup();
      return;
    }
    this.openPopup(
      this.widgetIdentifier,
      getPosition(detail),
      PopupType.AddWidget,
      detail,
      this.areaIdentifier,
      this.zoneIdentifier,
    );
  }

  onUpdateProperty(event): void {
    this.setWidgetProperty(this.widgetIdentifier, event.detail);
  }

  onMarkupUpdated(widgetMarkupElement: HTMLElement): void {
    initializeInlineEditors(widgetMarkupElement, this.displayedVariant.properties);
  }

  clickedAway() {
    if (this.isWidgetListOpen) {
      this.closePopup();
    }
  }
}
</script>

<style lang="less">
/* global styles */

.ktc-widget.ktc-draggable-mirror {
  .ktc-admin-ui {
    // Hide second border rendered when the widget cloned (including custom element contents) and custom element is re-rendered
    .ktc-widget-header:nth-of-type(2),
    .ktc-header-button:nth-of-type(2) {
      display: none;
    }
  }
}
</style>

<style lang="less" scoped>
.ktc-section {
  &.ktc-section--dragged,
  &.ktc-draggable-mirror {
    kentico-add-component-button {
      display: none !important;
    }
  }
}

.ktc-widget {
  min-height: 1.875 * @base-unit;

  // Reset positioning for widget content which includes margins
  position: relative;

  // To disable default draggable border
  &:focus {
    outline: 0;
  }

  &.ktc-widget--highlighted,
  &.ktc-widget--selected {
    kentico-widget-header {
      display: block;
    }
  }

  &.ktc-widget--dragged {
    height: 100%;
    width: 100%;
    opacity: 0.4;
    z-index: @active-element-z-index;

    .ktc-widget-body-wrapper {
      &, * {
        // Prevent events propagation to the widget markup during dragging
        pointer-events: none;
      }
    }

    &:hover {
      opacity: 1;
    }

    .ktc-admin-ui {
      kentico-add-component-button {
        display: none !important;
      }
    }
  }

  &.ktc-draggable-mirror {
    z-index: @dragged-widget-z-index;

    .ktc-admin-ui {
      kentico-widget-header {
        z-index: @dragged-widget-z-index;
      }

      kentico-add-component-button {
        display: none !important;
      }
    }
  }

  .ktc-admin-ui {
    kentico-add-component-button {
      position: absolute;
      bottom: @border-width;
      left: 50%;
      transform: translateX(-50%);
      z-index: @active-element-z-index;

      &[is-active="true"] {
        z-index: @widget-zone-add-component;
      }
    }

    kentico-drop-marker {
      position: absolute;
      width: 100%;
      bottom: 0;
    }
  }
}
</style>
