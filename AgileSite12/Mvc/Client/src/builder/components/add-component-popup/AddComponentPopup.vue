<template>
  <div class="ktc-add-component-pop-up" :style="containerStyle">
    <kentico-pop-up-container
      :theme.prop="popupTheme"
      :showBackButton="false"
      :headerTitle.prop="headerTitle"
      :position.prop="popup.position"
      :localizationService.prop="$_localizationService"
      triangle
      v-show="!isDnDActive"
      @selectItem="onInsertComponent"
      @closePopup="closePopup"
    >
      <kentico-pop-up-listing
        slot="pop-up-content"
        :items.prop="items"
        :noItemsAvailableMessage.prop="noComponentsAvailableMessage"
      />
    </kentico-pop-up-container>
  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import { Popup, PopupListingElement } from "@/builder/declarations";
import { componentHelper } from "@/builder/helpers";
import { PopupType } from "@/builder/store/types";
import { Theme } from "@/builder/types";

import { AddComponentPopupComponentProperties, ClosePopup } from "./add-component-popup-types";

@Component
export default class AddComponent extends Vue implements AddComponentPopupComponentProperties {
  @Prop() popup: Popup;
  @Prop() isDnDActive: boolean;
  @Prop() items: PopupListingElement[];

  @Prop() closePopup: ClosePopup;
  @Prop() selectItem: (componentName: string) => void;

  get popupTheme(): Theme {
    return componentHelper.getTheme(this.popup.popupType);
  }

  get headerTitle(): string {
    switch (this.popup.popupType) {
      case(PopupType.AddWidget): {
        return this.$_localizationService.getLocalization("widget.headerTitle");
      }
      case(PopupType.AddSection): {
        return this.$_localizationService.getLocalization("section.headerTitle");
      }
      default:
        return "";
    }
  }

  get noComponentsAvailableMessage(): string {
    return this.$_localizationService.getLocalization("widgetlist.nowidgets");
  }

  get containerStyle(): object {
    return (this.popup && this.popup.listingOffset) ?
            {
              top: this.popup.listingOffset.top + "px",
              left: this.popup.listingOffset.left + "px",
            }
            : null;
  }

  onInsertComponent(event: CustomEvent<PopupListingElement>): void {
    this.selectItem(event.detail.key);
  }
}
</script>
<style lang="less" scoped>

.ktc-add-component-pop-up {
  position: absolute !important;
  width: @pop-up-container-width;
}

kentico-pop-up-container {
  position: absolute !important;
  z-index: @widget-zone-add-component !important;
}
</style>
