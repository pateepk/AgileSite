import Vue from "vue";
import { configure } from "@storybook/vue";
import { setOptions } from "@storybook/addon-options";

Vue.config.ignoredElements = [/^kentico-/];

setOptions({
  hierarchyRootSeparator: /\|/,
});

function loadStories() {
  // You can require as many stories as you need.
  require("../src/stories");
}

configure(loadStories, module);
