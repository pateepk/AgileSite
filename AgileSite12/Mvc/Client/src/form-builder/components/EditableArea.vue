<template>
  <EditableArea :identifier="identifier">
    <template slot="sectionMarkup" slot-scope="{ sectionMarkup, sectionIdentifier }">
      <SectionMarkup :sectionMarkup="sectionMarkup" :sectionIdentifier="sectionIdentifier" :areaIdentifier="identifier" />
    </template>
  </EditableArea>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import { EditableAreaComponentProperties } from "@/form-builder/declarations";

import EditableArea from "./EditableAreaProvider";
import SectionMarkup from "./SectionMarkup.vue";

@Component({
  components: {
    EditableArea,
    SectionMarkup,
  },
})
export default class EditableAreaPageBuilder extends Vue implements EditableAreaComponentProperties {
  @Prop({ required: true }) identifier: string;
}
</script>
