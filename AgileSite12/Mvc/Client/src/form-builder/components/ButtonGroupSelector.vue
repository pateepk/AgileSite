<template>
      <div>
        <div class="ktc-btn-group">
            <button v-for="tab in tabs" :key="tab.identifier" type="button" class="ktc-btn ktc-btn-default" :class="{ 'ktc-active': tab.isActive }" @click="selectTab(tab)" >
                {{ tab.getLocalizedString() }}
            </button>
        </div>

        <div class="ktc-tabs-details">
          <slot></slot>
        </div>
      </div>
</template>

<script lang="ts">
import { Component, Mixins } from "vue-property-decorator";

import { BaseTabs } from "./BaseTabs";

@Component
export default class ButtonGroupSelector extends Mixins(BaseTabs) {
}
</script>
