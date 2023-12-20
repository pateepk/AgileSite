<template>
  <WidgetZone
    :sectionIdentifier="sectionIdentifier"
    :zoneIndex="zoneIndex"
    :areaIdentifier="areaIdentifier"
  >
    <template slot="widgetHeader" slot-scope="{ widgetTitle, widgetIdentifier }">
      <WidgetHeader
        :widgetTitle="widgetTitle"
        :widgetIdentifier="widgetIdentifier"
      />
    </template>
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
export default class FormBuilderWidgetZone extends WidgetZoneBase {

}
</script>
