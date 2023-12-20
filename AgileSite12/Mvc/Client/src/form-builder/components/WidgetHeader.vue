<template>
  <kentico-widget-header
    v-show="showWidgetHeader"
    class="ktc-widget-header--top"
    :localizationService.prop="$_localizationService"
    :widgetTitle.prop="widgetTitle"
  />
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import { WidgetHeaderComponentProperties } from "@/form-builder/declarations";

@Component
export default class WidgetHeader extends Vue implements WidgetHeaderComponentProperties {

  @Prop({ required: true }) widgetTitle: string;
  @Prop({ required: true }) widgetIdentifier: string;

  // State
  @Prop() activeItemIdentifier: string;
  @Prop() showWidgetHeader: boolean;
}
</script>

<style lang="less" src="@/builder/assets/widget-header.less" scoped></style>
