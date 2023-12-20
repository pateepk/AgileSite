<template>
  <BorderContainer
    :highlighted="highlightBorder"
    @mouseover.native="onSectionMouseOver"
    @mouseleave.native="onSectionMouseLeave"
    @click.native="onSectionClick"
  >
    <SectionHeader
      class="ktc-section-header-container"
      ref="sectionHeader"
      :sectionTypes="sectionTypes"
      :areaIdentifier="areaIdentifier"
      :sectionIdentifier="sectionIdentifier"
      :showSectionTypeList="showSectionTypeList"
      :sectionType="sectionType"
      :highlighted="highlightBorder"
      v-on-clickaway="onHeaderClickAway"
      @mouseenter.native.stop="onHeaderMouseEnter"
      @mouseleave.native.stop="onHeaderMouseLeave"
      @closePopup="onCloseSectionTypeList"
      @changeSection="$emit('changeSection', $event, sectionIdentifier)"
      @removeSection="$emit($event.type, sectionIdentifier)"
    >
      <template slot="sectionHeaderExtraButtons" slot-scope="extraButtons">
        <slot name="sectionHeaderExtraButtons" />
      </template>
    </SectionHeader>
    <div class="ktc-admin-ui">
      <DropZone v-if="showDropZone" :containerIdentifier="areaIdentifier" :position="position" :isBottom="false" class="ktc-dropzone--top" />
      <DropZone v-if="showDropZone" :containerIdentifier="areaIdentifier" :position="position" :isBottom="true" class="ktc-dropzone--bottom" />
    </div>
    <slot name="sectionMarkup" :sectionMarkup="sectionMarkup" />
    <div class="ktc-admin-ui">
      <kentico-drop-marker v-if="showSectionDropMarker" />
      <SectionDivider class="ktc-section-divider"
        v-show="showSectionDivider"
        :sectionIdentifier="sectionIdentifier"
        :areaIdentifier="areaIdentifier"
      />
    </div>
  </BorderContainer>
</template>

<script lang="ts">
import { mixin as clickaway } from "vue-clickaway";
import { Component, Prop, Vue, Watch } from "vue-property-decorator";

import { HIDE_ELEMENT_TIMEOUT } from "@/builder/constants";
import * as d from "@/builder/declarations";
import { EntityType } from "@/builder/EntityType";
import { dragAndDropHelper } from "@/builder/helpers";
import { getPosition } from "@/builder/helpers/position";
import { ComponentPosition, PopupType } from "@/builder/store/types";

import { BorderContainer, DropZone, SectionDivider, SectionHeader } from "@/builder/components";

@Component({
  components: {
    BorderContainer,
    DropZone,
    SectionDivider,
    SectionHeader,
  },
  mixins: [clickaway],
})
export default class Section extends Vue implements d.SectionComponentProperties {

  cancellableHideHeaderToken = null;
  highlightedBorder = false;

  @Prop({ required: true }) areaIdentifier: string;
  @Prop({ required: true }) sectionIdentifier: string;
  @Prop({ required: true }) position: number;

  // state props
  @Prop() sectionType: string;
  @Prop() sectionTypes: d.PopupListingElement[];
  @Prop() sectionMarkup: string;
  @Prop() popup: d.Popup;
  @Prop() highlightedSection: d.HighlightedSection;
  @Prop() dragAndDrop: d.DragAndDrop;

  // actions
  @Prop() showSectionHeader;
  @Prop() hideSectionHeader;
  @Prop() highlightSectionBorder;
  @Prop() dehighlightSectionBorder;
  @Prop() closePopup: () => void;
  @Prop() changeSection;

  @Watch("sectionMarkup")
  sectionMarkupChanged() {
    this.$emit("sectionMarkupChanged", this.sectionIdentifier);
  }

  @Watch("highlightedSection.freezeHighlight")
  freezeHighlightChanged() {
    if (this.highlightedSection.freezeHighlight && this.cancellableHideHeaderToken) {
      clearTimeout(this.cancellableHideHeaderToken);
    }
  }

  get showSectionTypeList() {
    const { componentIdentifier, popupType } = this.popup;
    return componentIdentifier === this.sectionIdentifier && popupType === PopupType.ChangeSection;
  }

  get renderSectionHeader() {
    return this.highlightedSection.sectionIdentifier === this.sectionIdentifier;
  }

  get highlightBorder() {
    return this.renderSectionHeader && this.highlightedSection.highlightBorder;
  }

  get showDropZone() {
    return this.dragAndDrop.entity === EntityType.Section &&
      this.dragAndDrop.itemIdentifier !== this.sectionIdentifier;
  }

  get showSectionDropMarker() {
    return (
      this.dragAndDrop.targetContainerIdentifier === this.areaIdentifier &&
      this.dragAndDrop.dropMarkerPosition === this.position + 1
    );
  }

  get showSectionDivider() {
    return !this.dragAndDrop.itemIdentifier;
  }

  mounted() {
    dragAndDropHelper.mountSectionToDragAndDrop(this.sectionIdentifier);
  }

  onSectionMouseOver() {
    if (!this.renderSectionHeader && !this.popup.componentIdentifier && !this.dragAndDrop.itemIdentifier) {
      this.showSectionHeader(this.sectionIdentifier, this.computeHeaderPosition());
    }
    if (this.dragAndDrop.entity === EntityType.Section) {
      this.highlightSectionBorder(this.sectionIdentifier);
    }
    if (this.cancellableHideHeaderToken) {
      clearTimeout(this.cancellableHideHeaderToken);
    }
  }

  onSectionMouseLeave() {
    if (this.highlightedSection.freezeHighlight) {
      clearTimeout(this.cancellableHideHeaderToken);
      return;
    }
    if (!this.showSectionTypeList) {
      this.cancellableHideHeaderToken = setTimeout(() => {
        if (this.renderSectionHeader) {
          this.hideSectionHeader();
        }}, HIDE_ELEMENT_TIMEOUT);
    }
    if (this.dragAndDrop.entity === EntityType.Section) {
      this.dehighlightSectionBorder();
    }
  }

  onSectionClick() {
    if (!this.renderSectionHeader) {
      this.showSectionHeader(this.sectionIdentifier, this.computeHeaderPosition());
    }
  }

  onHeaderMouseEnter() {
    if (!this.highlightBorder && !this.dragAndDrop.itemIdentifier) {
      this.highlightSectionBorder();
    }
  }

  onHeaderMouseLeave() {
    if (!this.showSectionTypeList && !this.dragAndDrop.itemIdentifier && !this.highlightedSection.freezeHighlight) {
      // Sometimes when mousing out of section header and opening the section properties at the same time, open section properties gets dispatched
      // after onHeaderMouseLeave gets fired which dispatches dehighlightSectionBorder making the section and its' header dehighlighted,
      // therefore the dehighlight has to be in nextTick and the condition for freezeHighlight has to be checked twice
      this.$nextTick(() => {
          if (!this.highlightedSection.freezeHighlight) {
            this.dehighlightSectionBorder();
          }
        });
    }
  }

  onHeaderClickAway(event: MouseEvent) {
    if (this.showSectionTypeList) {
      this.closePopup();
    }
    if (this.renderSectionHeader) {
      if (this.isClickOutsideSection(event)) {
        this.hideSectionHeader();
      } else if (this.highlightBorder) {
        this.dehighlightSectionBorder();
      }
    }
  }

  onCloseSectionTypeList(event: CustomEvent<MouseEvent>) {
    if (this.isClickOutsideSection(event.detail)) {
      this.hideSectionHeader();
    } else {
      this.dehighlightSectionBorder();
    }
    this.closePopup();
  }

  computeHeaderPosition() {
    const sectionHeaderElement = this.$refs.sectionHeader as Vue;
    const sectionHeader = sectionHeaderElement.$el.querySelector("kentico-section-header");
    if (!sectionHeader) {
      return null;
    }

    const position = getPosition(sectionHeader.getBoundingClientRect(), sectionHeader.offsetWidth);
    return position === ComponentPosition.Left ? ComponentPosition.Left : ComponentPosition.Right;
  }

  isClickOutsideSection(event: MouseEvent) {
    const { clientX, clientY } = event;
    const { left, right, top, bottom } = this.$el.getBoundingClientRect();

    return clientX < left || clientX > right || clientY < top || clientY > bottom;
  }
}
</script>

<style lang="less" scoped>
.ktc-section {
  position: relative;

  // Hide the outline added by draggable
  &:focus {
    outline: 0;
  }

  &.ktc-section--dragged {
    opacity: 0.4;

    > .ktc-border-root {
      &, * {
        pointer-events: none !important;
      }
    }

    &:hover {
      opacity: 1;
    }

    .ktc-section-divider,
    .ktc-section-header-container,
    kentico-widget-header {
      display: none !important;
    }
  }

  &.ktc-draggable-mirror {
    z-index: @dragged-widget-z-index;

    kentico-widget-header {
      z-index: @dragged-widget-z-index;
    }

    .ktc-section-header-wrapper,
    .ktc-section-divider {
      display: none !important;
    }
  }

  kentico-drop-marker {
    position: absolute;
    width: 100%;
  }
}
</style>
