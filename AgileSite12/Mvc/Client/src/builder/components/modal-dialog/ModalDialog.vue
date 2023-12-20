<template>
  <kentico-modal-dialog
    ref="modalDialog"
    :configuration.prop="dialogConfiguration"
    :messages.prop="dialogMessages"
    @apply="onDialogApply"
    @close="onDialogClose"
    @mousewheel.prevent.stop>
    <iframe ref="dialogContentWrapper"
            v-show="isDialogContentLoaded"
            class="ktc-modal-dialog__content"
            :src="dialog.markupUrl"
            :style="{ height: dialogContentStringHeight }"
            @load="onDialogContentLoaded"></iframe>
  </kentico-modal-dialog>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import { DIALOG_FOOTER_HEIGHT, DIALOG_HEADER_HEIGHT } from "@/builder/constants";
import { ModalDialog as ModalDialogState, Refs } from "@/builder/declarations";
import { addCancelScreenLockEventListeners } from "@/builder/helpers/screen-lock";
import { disablePageScrolling, enablePageScrolling } from "@/builder/services/modal-dialog/scrolling";
import { Theme } from "@/builder/types";
import { ModalDialogConfiguration, ModalDialogMessages } from "@/builder/web-components/modal-dialog/modal-dialog-types";

import { ApplyDialog, CloseDialog, ModalDialogComponentProperties } from "./modal-dialog-types";

@Component
export default class ModalDialog extends Vue implements ModalDialogComponentProperties {
  readonly $refs!: Refs<{
    dialogContentWrapper: HTMLIFrameElement;
    modalDialog: HTMLKenticoModalDialogElement;
  }>;
  readonly baseUnit = 16;

  dialogContentHeight = 0;
  isDialogContentLoaded = false;

  @Prop({ required: true }) readonly dialogIndex: number;

  @Prop() readonly dialog: ModalDialogState;
  @Prop() readonly theme: Theme;
  @Prop() readonly openedDialogsCount: number;

  @Prop() readonly applyDialog: ApplyDialog;
  @Prop() readonly closeDialog: CloseDialog;

  get dialogConfiguration(): ModalDialogConfiguration {
    const { title, markupUrl, showFooter, applyButtonText, cancelButtonText, width, maximized } = this.dialog;

    return {
      title,
      showFooter,
      applyButtonText,
      cancelButtonText,
      contentUrl: markupUrl,
      theme: this.theme,
      showLoader: !this.isDialogContentLoaded,
      dialogIndex: this.dialogIndex,
      openedDialogsCount: this.openedDialogsCount,
      width: this.isDialogContentLoaded ? width : "38%",
      maximized: this.isDialogContentLoaded ? maximized : false,
    };
  }

  get footerHeight(): number {
    return this.dialogConfiguration.showFooter ? DIALOG_FOOTER_HEIGHT : 0;
  }

  get dialogContentStringHeight() {
    return `${this.dialogContentHeight}px`;
  }

  get dialogMessages(): ModalDialogMessages {
    return {
      unsavedChanges: this.unsavedChangesMessage,
      loaderMessage: this.loaderMessage,
      closeTooltip: this.closeTooltip,
    };
  }

  get closeTooltip() {
    return this.$_localizationService.getLocalization("typelist.closeTooltip");
  }

  get unsavedChangesMessage() {
    return this.$_localizationService.getLocalization("modalDialog.close.confirmation");
  }

  get loaderMessage() {
    return this.$_localizationService.getLocalization("modaldialogs.loading");
  }

  getDialogContentHeight() {
    const iFrameElement = this.$refs.dialogContentWrapper;

    // Preserve dialog's min height
    const dialogContentHeight = iFrameElement.scrollHeight > iFrameElement.contentDocument.body.scrollHeight
                                  ? iFrameElement.scrollHeight
                                  : iFrameElement.contentDocument.body.scrollHeight;

    const wholeDialogHeight = dialogContentHeight + DIALOG_HEADER_HEIGHT + this.footerHeight;
    const maxDialogHeight = window.innerHeight - 2 * this.baseUnit;

    if (this.dialog.maximized || wholeDialogHeight > maxDialogHeight) {
      return maxDialogHeight - DIALOG_HEADER_HEIGHT - this.footerHeight;
    }

    return dialogContentHeight;
  }

  mounted() {
    // disable page scrolling when dialog is open
    disablePageScrolling(document);

    window.addEventListener("resize", this.onResize);
  }

  beforeDestroy() {
    // remove page scrolling prevention when current dialog is the last one opened
    if (this.dialogIndex === 0) {
      enablePageScrolling(document);
    }

    window.removeEventListener("resize", this.onResize);
  }

  onResize() {
      this.dialogContentHeight = this.getDialogContentHeight();
      this.$nextTick(() => {
        this.$refs.modalDialog.adjustDialogHeight(this.dialogContentHeight + DIALOG_HEADER_HEIGHT + this.footerHeight);
      });
  }

  onDialogApply() {
    this.applyDialog(this.$refs.dialogContentWrapper.contentWindow);
  }

  onDialogClose() {
    this.closeDialog(this.$refs.dialogContentWrapper.contentWindow);
  }

  onDialogContentLoaded() {
    this.isDialogContentLoaded = true;
    this.injectResizeToIframe();
    this.hookCancelScreenLock();

    // wait until the iframe gets shown
    this.$nextTick(() => {
      this.dialogContentHeight = this.getDialogContentHeight();

      // Wait until the calculated dialog height is applied
      this.$nextTick(() => {
        this.$refs.modalDialog.adjustDialogHeight(this.dialogContentHeight + DIALOG_HEADER_HEIGHT + this.footerHeight);
      });
    });
  }

  hookCancelScreenLock = () => {
    const iFrameDocument = this.$refs.dialogContentWrapper.contentDocument.documentElement;
    addCancelScreenLockEventListeners(iFrameDocument);
  }

  injectResizeToIframe = () => {
    const iframeWindow = this.$refs.dialogContentWrapper.contentWindow as any;

    iframeWindow.kentico = iframeWindow.kentico || {};
    iframeWindow.kentico.modalDialog = iframeWindow.kentico.modalDialog || {};
    iframeWindow.kentico.modalDialog.resize = this.onResize;
  }
}
</script>

<style lang="less" scoped>
.ktc-modal-dialog__content {
  border: 0;
  width: 100%;
  overflow: hidden;
  min-height: @dialog-min-height - @modal-dialog-header-height - @modal-dialog-footer-height - 1px;

  // This ensures that the iframe has no additional white space on the bottom
  display: block;
}
</style>
