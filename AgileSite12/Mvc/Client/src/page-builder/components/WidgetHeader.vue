<template>
  <kentico-widget-header
    :localizationService.prop="$_localizationService"
    :class="headerClasses"
    :widgetTitle.prop="widgetTitle"
    @removeWidget="$emit($event.type)"
  >
    <div slot="extraHeaderButtons" class="ktc-extra-header-buttons">
      <WidgetPersonalization v-if="isPersonalizationAvailable" :widgetIdentifier="widgetIdentifier" />
      <WidgetPropertiesButton
        v-if="showPropertiesButton"
        :identifier="widgetIdentifier"
        :dialogType="propertiesDialogType"
        buttonTooltip="widget.propertiesButtonTooltip" />
    </div>
  </kentico-widget-header>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import { builderConfig } from "@/builder/api/client";
import { Entities, PersonalizationConditionTypeMetadata } from "@/builder/declarations";
import { objectHelper } from "@/builder/helpers";
import { ModalDialogType } from "@/builder/store/types";

import { WidgetHeaderComponentProperties } from "@/page-builder/declarations";
import { PageBuilderConfig } from "@/page-builder/PageBuilderConfig";

import WidgetPropertiesButton from "./PropertiesButtonProvider";
import WidgetPersonalization from "./WidgetPersonalizationProvider";

@Component({
  components: {
    WidgetPropertiesButton,
    WidgetPersonalization,
  },
})
export default class WidgetHeader extends Vue implements WidgetHeaderComponentProperties {
  showHeaderBelow = false;
  propertiesDialogType = ModalDialogType.WidgetProperties;

  @Prop({ required: true }) widgetTitle: string;
  @Prop({ required: true }) widgetIdentifier: string;
  @Prop({ required: true }) widgetTopOffset: number;

  // State
  @Prop() showWidgetHeader: boolean;
  @Prop() personalizationConditionTypes: Entities<PersonalizationConditionTypeMetadata>;
  @Prop() showPropertiesButton: boolean;

  get isPersonalizationAvailable() {
    const pageBuilderConfig = builderConfig as PageBuilderConfig;
    return pageBuilderConfig.featureSet.personalizationEnabled && !objectHelper.isEmpty(this.personalizationConditionTypes);
  }

  get headerClasses() {
    return {
      "ktc-widget-header--top": !this.showHeaderBelow,
      "ktc-widget-header--bottom": this.showHeaderBelow,
      "ktc-widget-header--hidden": !this.showWidgetHeader,
    };
  }

  mounted() {
    this.ensureHeaderPosition();
  }

  updated() {
    this.ensureHeaderPosition();
  }

  ensureHeaderPosition() {
    // If the offset is smaller than the header height show the header below the widget
    this.showHeaderBelow = this.widgetTopOffset < this.$el.clientHeight;
  }
}
</script>

<style lang="less">
/* global styles */

.ktc-widget.ktc-draggable-mirror {
  .ktc-admin-ui {
    kentico-personalization-button {
      display: none;
    }
  }
}

.ktc-admin-ui {
  .ktc-extra-header-buttons {
    display: inline-flex;
  }

  kentico-widget-header {
    &.ktc-widget-header--hidden {
      &, * {
        visibility: hidden;
      }
    }
  }
}
</style>

<style lang="less" src="@/builder/assets/widget-header.less" scoped></style>
