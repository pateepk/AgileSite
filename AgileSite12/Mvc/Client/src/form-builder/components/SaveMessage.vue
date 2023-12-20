<template>
  <div v-show="showMessage">
    <slot v-if="savingInProgress" name="saving"></slot>
    <slot v-if="!savingInProgress" name="saved"></slot>
  </div>
</template>

<script lang="ts">
import { Component, Vue, Prop, Watch } from "vue-property-decorator";

import { SaveMessageComponentState } from "@/form-builder/declarations/components/save-message";

@Component
export default class SaveMessage extends Vue implements SaveMessageComponentState {
  timeoutID: number | null = null;
  showMessage = false;

  @Prop() savingInProgress: boolean = null;
  @Prop({ default: 2000 }) fadeOutDuration: number;

  @Watch("savingInProgress")
  onSave(newValue: boolean) {
    this.showMessage = true;

    if (!newValue) {
      this.timeoutID = window.setTimeout(() => {
        this.showMessage = false;
      }, this.fadeOutDuration);
    } else if (newValue && this.timeoutID) {
      // clear pending fadeout
      window.clearTimeout(this.timeoutID);
      this.timeoutID = null;
    }
  }
}
</script>
