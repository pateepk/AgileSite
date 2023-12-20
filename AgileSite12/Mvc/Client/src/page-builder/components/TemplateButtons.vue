<template>
  <div class="ktc-template-header">
    <ChangeTemplateButton
      v-if="hasTemplate"
      buttonTooltip="template.changebuttontooltip" />
    <TemplatePropertiesButton
      v-if="hasTemplateProperties"
      buttonTooltip="template.propertiesbuttontooltip"
      :identifier="templateIdentifier"
      :isTemplateButton="true"
      :dialogType="templateDialogType" />
  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import { ModalDialogType } from "@/builder/store/types";

import { TemplateButtonsComponentContext } from "../declarations";

import ChangeTemplateButton from "./ChangeTemplateButtonProvider";
import TemplatePropertiesButton from "./PropertiesButtonProvider";

@Component({
  components: {
    TemplatePropertiesButton,
    ChangeTemplateButton,
  },
})
export default class TemplateButtons extends Vue implements TemplateButtonsComponentContext {
  readonly templateDialogType = ModalDialogType.TemplateProperties;

  @Prop({ required: true }) templateIdentifier: string;
  @Prop({ required: true }) hasTemplate: boolean;
  @Prop({ required: true }) hasTemplateProperties: boolean;
}
</script>

<style lang="less">
.ktc-admin-ui {
  .ktc-template-header {
    opacity: 0.6;
    position: fixed;
    bottom: @base-unit;
    left: @base-unit;
    display: inline-flex;
  }

  .ktc-template-header:hover {
    opacity: 1;
    z-index: @active-element-z-index;
  }
}
</style>
