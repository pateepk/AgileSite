<template>
  <div
    :class="sectionHeaderClasses"
  >
    <kentico-section-header
      :display="sectionHeaderPosition"
      :section-type-list-position="sectionTypeListPosition"
      :localizationService.prop="$_localizationService"
      :sectionTypeListTitleText.prop="getSectionTypeListTitleText"
      :sectionTypes.prop="sectionTypes"
      :activeSectionTypeIdentifier.prop="sectionType"
      @removeSection="$emit($event.type, $event)"
      @toggleSectionTypeList="onToggleSectionTypeList"
      @closePopup="onClosePopup"
      @selectItem="onChangeSection">
      <div slot="sectionHeaderExtraButtons">
        <slot name="sectionHeaderExtraButtons" />
      </div>
    </kentico-section-header>
  </div>
</template>

<script lang="ts">
import Stickyfill from "stickyfilljs";
import { Component, Prop, Vue, Watch } from "vue-property-decorator";

import { POP_UP_CONTAINER_WIDTH } from "@/builder/constants";
import { PopupListingElement } from "@/builder/declarations";
import { ComponentPosition, PopupType } from "@/builder/store/types";

import { ClosePopup, OpenPopup, SectionHeaderComponentProperties } from "./section-header-types";

@Component
export default class SectionHeaderContainer extends Vue implements SectionHeaderComponentProperties {
  FULL_HEADER_OPACITY = "1";
  LOWERED_HEADER_OPACITY = "0.7";
  setOpacityTimeoutCancellationToken = false;
  isMouseOver = false;

  @Prop({ required: true }) areaIdentifier: string;
  @Prop({ required: true }) sectionIdentifier: string;
  @Prop({ required: true }) showSectionTypeList: boolean;
  @Prop({ required: true }) sectionTypes: PopupListingElement[];
  @Prop({ required: true }) sectionType: string;
  @Prop({ required: true }) highlighted: boolean;

  // state props
  @Prop() sectionTypeListPosition: ComponentPosition;
  @Prop() sectionHeaderPosition: ComponentPosition;

  // actions
  @Prop() openPopup: OpenPopup;
  @Prop() closePopup: ClosePopup;

  // If the cursor is not over the header and section type list position is falsy,
  // therefore section type list is closed, change the opacity of the header back to 0.7.
  @Watch("sectionTypeListPosition")
  onSectionTypeListPositionChange(newValue: ComponentPosition) {
    if (!this.isMouseOver && !newValue) {
      this.$el.style.opacity = this.LOWERED_HEADER_OPACITY;
    }
  }

  @Watch("highlighted")
  onBorderHighlightedChange(newValue: boolean) {
    newValue ? this.highlightHeader() : this.unhighlightHeader();
  }

  get sectionHeaderClasses() {
    return {
      "ktc-admin-ui": true,
      "ktc-section-header-wrapper": true,
      "ktc-section-header--hidden": !this.sectionHeaderPosition,
    };
  }

  get getSectionTypeListTitleText() {
    return this.$_localizationService.getLocalization("section.typelist.title");
  }

  onToggleSectionTypeList() {
    if (!this.sectionTypeListPosition) {
      this.openPopup(this.sectionIdentifier, this.calculatePopupPosition(), PopupType.ChangeSection);
    } else {
      this.closePopup();
    }
  }

  onClosePopup(event: Event) {
    // Ensure that opacity of the section header is lowered after clicking on the X button od the section type list
    this.isMouseOver = false;
    this.setOpacityTimeoutCancellationToken = true;

    this.$emit(event.type, event);
  }

  onChangeSection(event: Event) {
    // Lower section header opacity after selecting a new section type
    this.$el.style.opacity = this.LOWERED_HEADER_OPACITY;

    this.$emit("changeSection", event);
  }

  highlightHeader() {
    // Remember that the cursor is over the header so that opacity stays at full
    // after toggling section type list by clicking on its' icon in the header
    this.isMouseOver = true;

    if (this.$el.style.opacity !== this.FULL_HEADER_OPACITY) {
      this.setOpacityTimeoutCancellationToken = false;

      // Change to full opacity after having the cursor over the header for 300ms
      setTimeout(() => {
        if (!this.setOpacityTimeoutCancellationToken) {
          this.$el.style.opacity = this.FULL_HEADER_OPACITY;
        }
      }, 300);
    }
  }

  unhighlightHeader() {
    // Remember that the cursor is not over the header so that opacity is lowered
    // after section type list is closed
    this.isMouseOver = false;

    // Ensure that the opacity of the header is not set to full
    // if the cursor was moved away from the header before the timeout ran out
    this.setOpacityTimeoutCancellationToken = true;

    // Change to 0.7 opacity when the cursor is not over the header and section type list is not opened
    if (this.$el.style.opacity === this.FULL_HEADER_OPACITY && !this.sectionTypeListPosition) {
      this.$el.style.opacity = this.LOWERED_HEADER_OPACITY;
    }
  }

  calculatePopupPosition(): ComponentPosition {
    // Add connecting triangle width to the popup width
    const popupWidth = POP_UP_CONTAINER_WIDTH + 10;
    return this.$el.getBoundingClientRect().left - popupWidth >= 0
            ? ComponentPosition.Left
            : ComponentPosition.Right;
  }

  mounted() {
    // Ensure stickiness of the header
    Stickyfill.addOne(this.$el);
  }

  beforeDestroy() {
    Stickyfill.removeOne(this.$el);
  }
}
</script>

<style lang="less">
.ktc-admin-ui {
  &.ktc-section-header-wrapper {
    position: -webkit-sticky;
    position: sticky;
    top: 0;
    z-index: @active-element-addon-z-index;
    margin-left: 100%;
    opacity: 0.7;

    // Bring section header forward after hovering for 300ms so it doesn't collide with other elements like widget header
    &:hover {
      @keyframes highlightWrapper {
        99% {
          z-index: @active-element-addon-z-index;
        }
        100% {
          z-index: @active-element-addon-z-index + 10;
        }
      }

      animation: highlightWrapper 0.3s forwards;
    }

    kentico-section-header {
      position: absolute;
      visibility: visible;

      top: -1 * @border-width;
      right: -1 * @section-header-width;

      &[display="left"] {
        right: -1 * @border-width;
      }
    }
  }

  &.ktc-section-header--hidden {
    &, * {
      visibility: hidden;
      pointer-events: none;
    }
  }
}
</style>
