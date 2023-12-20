<template>
  <!-- IMPORTANT: Do not remove the 2 wrapping divs, otherwise lists from medium editor would break personalization by breaking the property binding -->
  <!-- IMPORTANT: Do not remove "key" attribute as it is essential to ensure re-rendering of the component according to currently displayed variant -->
  <div :key="variantIdentifier">
    <div>
      <div class="ktc-widget-body-wrapper" ref="widgetMarkup" :data-markup.prop="markup" />
    </div>
  </div>
</template>

<script lang="ts">
import v4 from "uuid/v4";
import { Component, Prop, Vue } from "vue-property-decorator";

import { WidgetMarkupComponentProperties } from "@/builder/declarations";
import { linkHelper } from "@/builder/helpers";
import { removeScriptElements, renderMarkup } from "@/builder/helpers/markup-helper";
import { invokeInlineEditorsDestroy } from "@/builder/services/inline-editors";

@Component
export default class WidgetMarkup extends Vue implements WidgetMarkupComponentProperties {
  @Prop({ required: true }) markup: string;
  @Prop({ required: true }) variantIdentifier: string;

  externalScriptIdentifier: string = `ktc-widget-markup-script-${v4()}`;

  beforeDestroy() {
    this.cleanup();
  }

  mounted() {
    const element: HTMLElement = this.$refs.widgetMarkup as HTMLElement;
    renderMarkup(this.markup, element, this.externalScriptIdentifier);
    linkHelper.disableElementLinks(element);
    this.initializeEditors();
  }

  beforeUpdate() {
    this.cleanup();
  }

  updated() {
    const element: HTMLElement = this.$refs.widgetMarkup as HTMLElement;
    renderMarkup(this.markup, element, this.externalScriptIdentifier);
    linkHelper.disableElementLinks(element);
    this.initializeEditors();
  }

  cleanup(): void {
    invokeInlineEditorsDestroy(this.$refs.widgetMarkup as HTMLElement);
    removeScriptElements(this.externalScriptIdentifier);
  }

  initializeEditors(): void {
    this.$emit("markupUpdated", this.$refs.widgetMarkup as HTMLElement);
  }
}
</script>
