<template>
      <div class="ktc-form-builder-tabs">
        <div class="ktc-form-builder-tabs-nav">
            <a v-for="tab in tabs" :key="tab.identifier" class="ktc-form-builder-tab-link" :class="{ 'ktc-active': tab.isActive }" @click="selectTab(tab)" >
                {{ tab.getLocalizedString() }}
            </a>
        </div>

        <div class="ktc-form-builder-tab-content">
          <div class="ktc-form-builder-tab-content-container">
            <slot></slot>
          </div>
        </div>
      </div>
</template>

<script lang="ts">
import { Component, Mixins } from "vue-property-decorator";

import { BaseTabs } from "./BaseTabs";

@Component
export default class Tabs extends Mixins(BaseTabs) {
}
</script>
