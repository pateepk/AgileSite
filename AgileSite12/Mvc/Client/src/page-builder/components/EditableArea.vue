<template>
  <EditableArea :identifier="identifier">
    <SectionMarkup slot="sectionMarkup" slot-scope="{ sectionMarkup, sectionIdentifier }"
      :sectionMarkup="sectionMarkup"
      :sectionIdentifier="sectionIdentifier"
      :areaIdentifier="identifier" />
    <SectionHeaderExtraButtons slot="sectionHeaderExtraButtons" slot-scope="{ sectionIdentifier }"
      :sectionIdentifier="sectionIdentifier" />
  </EditableArea>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import { EditableAreaComponentProperties } from "@/page-builder/declarations";

import EditableArea from "./EditableAreaProvider";
import SectionHeaderExtraButtons from "./SectionHeaderExtraButtonsProvider";
import SectionMarkup from "./SectionMarkup.vue";

@Component({
  components: {
    EditableArea,
    SectionMarkup,
    SectionHeaderExtraButtons,
  },
})
export default class EditableAreaPageBuilder extends Vue implements EditableAreaComponentProperties {
  @Prop({ required: true }) identifier: string;
}
</script>
