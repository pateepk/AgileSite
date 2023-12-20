<template>
  <div :class="borderClasses">
    <div class="ktc-admin-ui">
      <kentico-alert-box v-if="banned" :message="bannedMessage"></kentico-alert-box>
      <div class="ktc-border ktc-border-top"></div>
      <div class="ktc-border ktc-border-right" v-show="showVerticalBorder"></div>
      <div class="ktc-border ktc-border-bottom"></div>
      <div class="ktc-border ktc-border-left" v-show="showVerticalBorder"></div>
    </div>
    <slot></slot>
  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import { BorderContainerComponentProperties } from "@/builder/declarations";

@Component
export default class BorderContainer extends Vue implements BorderContainerComponentProperties {
  @Prop({ required: false, default: false })
  highlighted: boolean;

  @Prop({ required: false, default: false })
  selected: boolean;

  @Prop({ required: false, default: false })
  banned: boolean;

  @Prop({ required: false })
  bannedMessage: string;

  // Changes border highlighted/selected color to blue
  @Prop({ required: false, default: false })
  primary: boolean;

  // This solves unwanted merging of dashed borders
  @Prop({ required: false, default: false })
  hideVerticalBorder: boolean;

  get borderClasses() {
    return {
      "ktc-border-root": true,
      "ktc-border-root--primary": this.primary,
      "ktc-border-root--highlighted": this.highlighted,
      "ktc-border-root--selected": this.selected,
      "ktc-border-root--banned": this.banned,
    };
  }

  get showVerticalBorder() {
    return !this.hideVerticalBorder || !this.highlighted || !this.selected;
  }
}
</script>

<style lang="less" scoped>
.ktc-section {
  &.ktc-section--dragged,
  &.ktc-draggable-mirror {
    .ktc-widget--selected.ktc-border-root--selected {
      > .ktc-admin-ui {
        > .ktc-border {
          border-style: @border-style;
          border-color: @border-color;
        }
      }
    }
  }
}

.ktc-border-root {

  // This part makes sure that the top & bottom margins will be included withing the widget border
  &:before, &:after {
    content: ' ';
    display: table;
  }

  > .ktc-admin-ui {
    > kentico-alert-box {
      z-index: @banned-widget-zone-z-index;
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
    }

    > .ktc-border {
      display: block;
      position: absolute;
      z-index: @border-z-index;
      box-sizing: border-box;

      border-top: @border-width @border-style @border-color;
      border-left: @border-width @border-style @border-color;
      border-right-width: 0;
      border-bottom-width: 0;

      &.ktc-border-top {
        top: -@border-width;
        left: -@border-width;
        width: @widget-width-including-border;
      }
      &.ktc-border-left {
        top: 0;
        left: -@border-width;
        height: 100%;
      }
      &.ktc-border-right {
        top: 0;
        right: -@border-width;
        height: 100%;
      }
      &.ktc-border-bottom {
        left: -@border-width;
        bottom: 0;
        width: @widget-width-including-border;
      }
    }
  }

  &.ktc-border-root--highlighted,
  &.ktc-border-root--selected {
    > .ktc-admin-ui {
      > .ktc-border {
        display: block;
        z-index: @active-element-z-index;
        border-color: @border-highlighted-color;
      }
    }
  }

  &.ktc-border-root--selected {
    > .ktc-admin-ui {
      >.ktc-border {
        border-style: solid;
      }
    }
  }

  &.ktc-border-root--banned {
    > .ktc-admin-ui {
      > .ktc-border {
        display: block;
        z-index: @banned-widget-zone-z-index;
        border-color: @color-red-70;
        border-width: 2px;
      }
    }
  }

  &.ktc-border-root--primary {
    &.ktc-border-root--highlighted,
    &.ktc-border-root--selected {
      > .ktc-admin-ui {
        > .ktc-border {
          border-color: @color-blue-70;
        }
      }
    }
  }
}
</style>
