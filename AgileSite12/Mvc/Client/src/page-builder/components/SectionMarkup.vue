<script lang="ts">
import { Component, Mixins } from "vue-property-decorator";

import { SectionMarkupBase } from "@/builder/components/SectionMarkupBase";

import WidgetZone from "./WidgetZone.vue";

@Component
export default class SectionMarkup extends Mixins(SectionMarkupBase) {
  created() {
    this.widgetZone = WidgetZone;
  }
}
</script>
