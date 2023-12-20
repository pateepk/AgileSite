<template>
  <kentico-section-divider ref="addSection"
    :active="isSectionListOpen"
    :add-button-position="sectionDividerPosition"
    :localizationService.prop="$_localizationService"
    :show-component-list.prop="isSectionListOpen"
    :addTooltip.prop="addTooltip"
    v-on-clickaway="clickedAway"
    @openComponentList="onOpenComponentList"
    @closePopup="closePopup"
  />
</template>

<script lang="ts">
import { mixin as clickaway } from "vue-clickaway";
import { Component, Prop, Vue } from "vue-property-decorator";

import { SECTION_DIVIDER_ADD_BUTTON_OFFSET } from "@/builder/constants";
import { calculateListingOffset, getPosition } from "@/builder/helpers/position";
import { ComponentPosition, PopupType } from "@/builder/store/types";

import { ClosePopup, OpenPopup, SectionDividerComponentProperties, SetPopupPosition } from "./section-divider-types";

@Component({
  mixins: [clickaway],
})
export default class SectionDivider extends Vue implements SectionDividerComponentProperties {
  sectionDividerPosition = null;

  @Prop({ required: true }) readonly sectionIdentifier: string;
  @Prop({ required: true }) readonly areaIdentifier: string;

  // state props
  @Prop() readonly isSectionListOpen: boolean;

  // actions
  @Prop() readonly closePopup: ClosePopup;
  @Prop() readonly openPopup: OpenPopup;
  @Prop() readonly setPopupPosition: SetPopupPosition;

  get addTooltip() {
    return this.$_localizationService.getLocalization("section.addTooltip");
  }

  clickedAway() {
    if (this.isSectionListOpen) {
      this.closePopup();
    }
  }

  onOpenComponentList({ detail }: CustomEvent): void {
    // Close section list after clicking on the same addButton 2x
    if (this.isSectionListOpen) {
      this.closePopup();
      return;
    }

    this.openPopup(
      this.sectionIdentifier,
      getPosition(detail),
      PopupType.AddSection,
      detail,
      this.areaIdentifier,
    );
  }

  setDividerButtonPosition() {
    const sectionDivider = this.$refs.addSection as HTMLElement;
    const isSpace =
      sectionDivider.parentElement.getBoundingClientRect().left >
      SECTION_DIVIDER_ADD_BUTTON_OFFSET;
    this.sectionDividerPosition = isSpace
      ? ComponentPosition.Left
      : ComponentPosition.Right;

    // Calculate new popup listing position
    if (this.isSectionListOpen) {
      this.$nextTick(() => {
        const addComponentClientRect = document
                                      .querySelector(".ktc-section-divider[active='true']")
                                      .shadowRoot.querySelector("kentico-add-component-button[is-active='true']")
                                      .getBoundingClientRect();
        const newPosition = getPosition(addComponentClientRect);
        const newOffset = calculateListingOffset(addComponentClientRect);

        this.setPopupPosition(newPosition, newOffset);
      });
    }
  }

  beforeDestroy() {
    window.removeEventListener("resize", this.setDividerButtonPosition);
  }

  mounted() {
    this.setDividerButtonPosition();

    window.addEventListener("resize", this.setDividerButtonPosition);
  }
}
</script>
