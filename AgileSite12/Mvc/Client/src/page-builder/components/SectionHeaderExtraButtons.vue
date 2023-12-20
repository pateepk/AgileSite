<template>
  <div v-if="showPropertiesButton" class="ktc-extra-header-buttons">
    <SectionPropertiesButton
      :identifier="sectionIdentifier"
      :dialogType="propertiesDialogType"
      :button-type="buttonType"
      buttonTooltip="section.propertiesButtonTooltip" />
  </div>
</template>


<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import { ModalDialogType } from "@/builder/store/types";
import { SectionHeaderExtraButtonsComponentProperties } from "@/page-builder/declarations";

import { ButtonType } from "@/builder/types";
import SectionPropertiesButton from "./PropertiesButtonProvider";

@Component({
  components: {
    SectionPropertiesButton,
  },
})
export default class SectionHeaderExtraButtons extends Vue implements SectionHeaderExtraButtonsComponentProperties {
  readonly buttonType = ButtonType.Properties;

  propertiesDialogType = ModalDialogType.SectionProperties;

  @Prop({ required: true }) sectionIdentifier: string;
  @Prop() showPropertiesButton: boolean;
}
</script>
