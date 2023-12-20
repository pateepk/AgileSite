<template>
  <WidgetZone
    :sectionIdentifier="sectionIdentifier"
    :zoneIndex="zoneIndex"
    :areaIdentifier="areaIdentifier"
  >
    <WidgetHeader slot="widgetHeader" slot-scope="{ widgetTitle, widgetIdentifier, widgetTopOffset }"
      :widgetTitle="widgetTitle"
      :widgetIdentifier="widgetIdentifier"
      :widgetTopOffset="widgetTopOffset"
    />
  </WidgetZone>
</template>

<script lang="ts">
import { Component } from "vue-property-decorator";

import { WidgetZoneBase } from "@/builder/components/WidgetZoneBase";

import WidgetHeader from "./WidgetHeaderProvider";
import WidgetZone from "./WidgetZoneProvider";

@Component({
  components: {
    WidgetZone,
    WidgetHeader,
  },
})
export default class PageBuilderWidgetZone extends WidgetZoneBase {

}
</script>
