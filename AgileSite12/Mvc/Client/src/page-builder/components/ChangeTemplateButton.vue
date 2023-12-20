<template>
  <kentico-header-button
    :buttonTooltip.prop="localizedButtonTooltip"
    :icon.prop="changeTemplateIcon"
    :is-template-button="true"
    :button-type="buttonType"
    @buttonClick="openChangeTemplatePopup" />
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import { CustomModalDialogOptions, ModalDialogApplyCallbackResult } from "@/builder/declarations/modal-dialog";
import * as messaging from "@/builder/services/post-message";
import { ButtonType, MessageTypes, Theme } from "@/builder/types";
import { ChangeTemplateButtonComponentProperties } from "@/page-builder/declarations";
import { ButtonIcon } from "@/page-builder/types";

@Component
export default class ChangeTemplateButton extends Vue implements ChangeTemplateButtonComponentProperties {
  readonly changeTemplateIcon = ButtonIcon.Change;
  readonly buttonType = ButtonType.Change;

  @Prop({ required: true }) readonly buttonTooltip: string;

  @Prop() readonly dialogUrl: string;
  @Prop() readonly currentTemplateIdentifier: string;

  // Actions
  @Prop() openModalDialog: (options: CustomModalDialogOptions) => void;

  openChangeTemplatePopup() {
    this.openModalDialog({
      url: this.dialogUrl,
      data: { currentTemplateIdentifier: this.currentTemplateIdentifier },
      title: this.$_localizationService.getLocalization("template.change"),
      theme: Theme.Template,
      applyCallback: this.applyCallback,
      showFooter: true,
    });
  }

  changePageTemplate(identifier: string) {
    messaging.postMessage(MessageTypes.CONFIGURATION_CHANGED, null, "*");
    messaging.postMessage(MessageTypes.CHANGE_TEMPLATE, identifier, "*");
  }

  get localizedButtonTooltip() {
    return this.$_localizationService.getLocalization(this.buttonTooltip);
  }

  applyCallback(window: Window): ModalDialogApplyCallbackResult {
    const selectedItem = window.document.querySelector(".ktc-FlatSelectedItem");
    const isSelected = !!selectedItem;

    if (!isSelected) {
      return {
        closeDialog: false,
      };
    }

    if (selectedItem.getAttribute("data-identifier") === this.currentTemplateIdentifier) {
      return {
        closeDialog: true,
      };
    }

    const confirmationMessage = this.$_localizationService.getLocalization("template.change.confirmation");
    if (!confirm(confirmationMessage)) {
      return {
        closeDialog: false,
      };
    }

    const identifier = (selectedItem as HTMLElement).dataset.identifier;
    this.changePageTemplate(identifier);

    return {
      closeDialog: true,
    };
  }
}
</script>
