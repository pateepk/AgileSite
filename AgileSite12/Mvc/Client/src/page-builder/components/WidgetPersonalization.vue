<template>
  <kentico-personalization-button
    ref="personalizationButton"
    :localizationService.prop="$_localizationService"
    :builderConfig.prop="builderConfig"
    :httpClient.prop="httpClient"
    :show-personalization-popup="isPersonalizationOpen"
    :popupItems.prop="popupItems"
    :activeItemIdentifier.prop="activeItemIdentifier"
    :personalizationPopupPosition.prop="personalizationDialogPosition"
    :personalizationConditionTypes.prop="personalizationConditionTypes"
    :conditionType.prop="conditionType"
    @openPersonalizationPopup="onOpenPersonalizationPopup"
    :selectVariant.prop="selectVariant"
    :addVariant.prop="addVariant"
    :updateVariant.prop="updateVariant"
    :deleteVariant.prop="deleteVariant"
    :changeVariantsPriority.prop="changeVariantsOrder"
    @closePersonalizationPopup="closePopup"
    v-on-clickaway="onClickAway"
    :button-type="buttonType"
  />
</template>

<script lang="ts">
import { mixin as clickaway } from "vue-clickaway";
import { Component, Prop, Vue } from "vue-property-decorator";

import { builderConfig as config, http} from "@/builder/api/client";
import { Entities, PersonalizationConditionTypeMetadata, PopupListingElement } from "@/builder/declarations";
import { getPosition } from "@/builder/helpers/position";
import { ComponentPosition, PopupType } from "@/builder/store/types";
import { ButtonType } from "@/builder/types";
import { VariantListingElement, WidgetPersonalizationComponentProperties } from "@/page-builder/declarations";

@Component({
  mixins: [clickaway],
})
export default class WidgetPersonalization extends Vue implements WidgetPersonalizationComponentProperties {
  readonly buttonType = ButtonType.Personalization;

  builderConfig = config;
  httpClient = http;

  @Prop({ required: true }) widgetIdentifier: string;

  // State
  @Prop() isPersonalizationOpen: boolean;
  @Prop() popupItems: PopupListingElement[] | VariantListingElement[];
  @Prop() personalizationDialogPosition: ComponentPosition;
  @Prop() activeItemIdentifier: string;
  @Prop() conditionType?: string;
  @Prop() personalizationConditionTypes: Entities<PersonalizationConditionTypeMetadata>;

  // Actions
  @Prop() addWidgetVariant;
  @Prop() updateWidgetConditionTypeParameters;
  @Prop() removeWidgetVariant;
  @Prop() closePopup: () => void;
  @Prop() openPopup;
  @Prop() selectWidgetVariant;
  @Prop() changeVariantsPriority;

  onOpenPersonalizationPopup() {
    if (this.isPersonalizationOpen) {
      this.closePopup();
      return;
    }

    const personalizationButton = this.$refs.personalizationButton as Element;

    this.openPopup(
      this.widgetIdentifier,
      getPosition(personalizationButton.getBoundingClientRect()),
      PopupType.Personalization,
    );
  }

  selectVariant(variant: PopupListingElement): void {
    this.selectWidgetVariant(this.widgetIdentifier, this.activeItemIdentifier, variant.key);
  }

  addVariant(variantName: string, conditionType: string, conditionTypeParameters: object): void {
    this.addWidgetVariant(this.widgetIdentifier, variantName, conditionType, conditionTypeParameters);
  }

  updateVariant(variantName: string, conditionTypeParameters: object, variantIdentifier: string): void {
    this.updateWidgetConditionTypeParameters(variantIdentifier, variantName, conditionTypeParameters);
  }

  deleteVariant(variantIdentifier: string): void {
    this.removeWidgetVariant(this.widgetIdentifier, variantIdentifier);
  }

  changeVariantsOrder(variants: string[]) {
    this.changeVariantsPriority(this.widgetIdentifier, variants);
  }

  onClickAway() {
    // Ensure that props from redux were updated
    this.$nextTick(() => {
      if (this.isPersonalizationOpen) {
        this.closePopup();
      }
    });
  }
}
</script>
