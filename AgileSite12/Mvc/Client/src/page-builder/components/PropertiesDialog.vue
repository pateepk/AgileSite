<template>
  <kentico-form-modal-dialog
    :localizationService.prop="$_localizationService"
    :formMarkup.prop="formMarkup"
    :componentName.prop="componentName"
    :isValid.prop="formIsValid"
    :theme.prop="dialogTheme"
    :openedDialogsCount.prop="dialogsCount"
    @click.stop
    @closeModalDialog="onClosePropertiesPopup"
    @submitPropertiesForm="onSubmitPropertiesForm"
  />
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import { Theme } from "@/builder/types";

import { PropertiesDialogComponentProperties } from "@/page-builder/declarations";

@Component
export default class PropertiesDialog extends Vue implements PropertiesDialogComponentProperties {
  dialogIndex: number = 0;

  @Prop() readonly formMarkup: string;
  @Prop() readonly properties: object;
  @Prop() readonly propertiesFormUrl: string;
  @Prop() readonly componentName: string;
  @Prop() readonly formIsValid: boolean;
  @Prop() readonly dialogTheme: Theme;
  @Prop() readonly dialogsCount: number;

  @Prop() readonly closeDialog: (dialogIndex: number) => void;
  @Prop() readonly fetchMarkup: (dialogIndex: number, formUrl: string, properties: object) => void;
  @Prop() readonly submitDialogForm: (dialogIndex: number, actionUrl: string, currentProperties: object, formData: FormData) => void;

  async mounted() {
    await this.fetchMarkup(this.dialogIndex, this.propertiesFormUrl, this.properties);
  }

  onClosePropertiesPopup() {
    this.closeDialog(this.dialogIndex);
  }

  async onSubmitPropertiesForm(event: CustomEvent<{ configurationForm: HTMLFormElement, formData: FormData }>) {
    await this.submitDialogForm(this.dialogIndex, event.detail.configurationForm.getAttribute("action"), this.properties, event.detail.formData);
  }
}
</script>
