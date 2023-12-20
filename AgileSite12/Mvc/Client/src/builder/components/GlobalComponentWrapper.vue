<template>
  <div class="ktc-admin-ui">
    <ModalDialog v-for="dialog in modalDialogs" :key="dialog.identifier" :dialogIndex="dialog.index" />
    <AddComponentPopup v-if="isAddComponentPopup" :selectItem="selectItem" />
    <slot />
  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import { AddComponentPopup, ModalDialog } from "@/builder/components";
import { GlobalComponentWrapperComponentProperties, ModalDialog as IModalDialog } from "@/builder/declarations";

import { PopupType } from "../store/types";

@Component({
  components: {
    AddComponentPopup,
    ModalDialog,
  },
})
export default class GlobalComponentWrapper extends Vue implements GlobalComponentWrapperComponentProperties {

  @Prop() readonly modalDialogs: IModalDialog[];
  @Prop() readonly popupType: PopupType;
  @Prop() readonly selectItem: (componentName: string) => void;

  get isAddComponentPopup(): boolean {
    return this.popupType === PopupType.AddWidget || this.popupType === PopupType.AddSection;
  }
}
</script>
