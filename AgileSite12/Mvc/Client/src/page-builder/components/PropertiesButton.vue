<template>
  <kentico-header-button
    :buttonTooltip.prop="localizedButtonTooltip"
    :is-template-button="isTemplateButton"
    :icon.prop="changePropertiesIcon"
    :button-type="buttonType"
    @buttonClick="onOpenPropertiesDialog"
  />
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import { ModalDialogType, OpenModalDialogOptions } from "@/builder/store/types";
import { ButtonType } from "@/builder/types";
import { ButtonComponentProperties } from "@/page-builder/declarations";
import { ButtonIcon } from "@/page-builder/types";

@Component
export default class PropertiesButton extends Vue implements ButtonComponentProperties {
  readonly changePropertiesIcon = ButtonIcon.Properties;
  readonly buttonType = ButtonType.Properties;

  @Prop({ required: true }) identifier: string;
  @Prop({ required: true }) buttonTooltip: string;
  @Prop({ required: true }) dialogType: ModalDialogType;

  @Prop({ default: false }) isTemplateButton: boolean;

  // Actions
  @Prop() openModalDialog: (options: OpenModalDialogOptions) => void;

  onOpenPropertiesDialog() {
    this.openModalDialog({
      componentIdentifier: this.identifier,
      dialogType: this.dialogType,
      showFooter: true,
    });
  }

  get localizedButtonTooltip() {
    return this.$_localizationService.getLocalization(this.buttonTooltip);
  }
}
</script>
