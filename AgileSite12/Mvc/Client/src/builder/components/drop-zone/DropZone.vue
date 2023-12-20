<template>
  <div class="ktc-dropzone" @mouseover="onMouseOver" />
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import { DragAndDrop } from "@/builder/declarations";

import { DropZoneComponentProperties, HideDropMarker, ShowDropMarker } from "./drop-zone-types";

@Component
export default class DropZone extends Vue implements DropZoneComponentProperties {

  @Prop({ required: true })
  readonly containerIdentifier: string;

  @Prop({ required: true })
  readonly position: number;

  @Prop({ required: false, default: false })
  readonly isBottom: boolean;

  // state props
  @Prop() readonly dragAndDrop: DragAndDrop;

  // actions
  @Prop() readonly hideDropMarker: HideDropMarker;
  @Prop() readonly showDropMarker: ShowDropMarker;

  onMouseOver() {
    const dropPosition = this.isBottom ? this.position + 1 : this.position;

    // do not display drop marker for the same position as dragged widget's original position
    if (this.containerIdentifier === this.dragAndDrop.sourceContainerIdentifier) {
      const bannedPosition = this.isBottom ? this.dragAndDrop.originalPosition : this.dragAndDrop.originalPosition + 1;

      if (dropPosition === bannedPosition) {
        // hide drop marker only if it is already displayed
        if (this.dragAndDrop.dropMarkerPosition !== null) {
          this.hideDropMarker();
        }
        return;
      }
    }

    // do not dispatch the action if the requested drop marker is same as the currently visible one
    if (this.containerIdentifier !== this.dragAndDrop.targetContainerIdentifier || dropPosition !== this.dragAndDrop.dropMarkerPosition) {
      this.showDropMarker(this.containerIdentifier, dropPosition);
    }
  }
}
</script>

<style lang="less" scoped>
.ktc-dropzone {
  position: absolute;
  z-index: @dropzone-z-index;
  width: 100%;
  box-sizing: border-box;

  &.ktc-dropzone--top {
    top: 0;
    height: 50%;
  }

  &.ktc-dropzone--bottom {
    bottom: 0;
    height: 50%;
  }

  &.ktc-dropzone--full {
    height: ~"calc(100% - @{border-width})";
  }

  &.ktc-dropzone--active {
    border: @border-radius-base solid @color-blue-100;
  }
}
</style>
