<template>
  <div v-show="isActive"><slot></slot></div>
</template>

<script lang="ts">
import { Component, Vue, Prop } from "vue-property-decorator";

import { getService } from "@/builder/container";
import { LocalizationService } from "@/builder/declarations";
import { SERVICE_TYPES } from "@/builder/types";

@Component
export default class Tab extends Vue {
  isActive: boolean = false;

  @Prop({ required: true })
  identifier: string;

  @Prop({ required: true })
  name: string;

  @Prop({ default: false })
  selected: boolean;

  mounted() {
    this.isActive = this.selected;
  }

  getLocalizedString() {
    return getService<LocalizationService>(SERVICE_TYPES.LocalizationService).getLocalization(this.name);
  }
}
</script>
