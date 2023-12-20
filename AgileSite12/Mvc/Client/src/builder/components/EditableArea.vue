<template>
  <BorderContainer
    :id="identifier"
    :class="editableAreaClasses"
    :banned="isBanned"
    :bannedMessage="bannedAreaMessage"
  >
    <div class="ktc-admin-ui">
      <kentico-drop-marker v-if="showAreaDropMarker" />
    </div>
    <Section v-for="(sectionIdentifier, index) in sectionsInArea" class="ktc-section"
      :id="sectionIdentifier"
      :key="sectionIdentifier"
      :position="index"
      :sectionIdentifier="sectionIdentifier"
      :areaIdentifier="identifier"
      @removeSection="onRemoveSection"
      @changeSection="onChangeSection"
      @sectionMarkupChanged="onSectionMarkupChanged"
    >
      <template slot="sectionMarkup" slot-scope="{ sectionMarkup }">
        <slot name="sectionMarkup" :sectionIdentifier="sectionIdentifier" :sectionMarkup="sectionMarkup" />
      </template>
      <template slot="sectionHeaderExtraButtons" slot-scope="myScope">
        <slot name="sectionHeaderExtraButtons" :sectionIdentifier="sectionIdentifier" />
      </template>
    </Section>
  </BorderContainer>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import * as d from "@/builder/declarations";
import { EntityType } from "@/builder/EntityType";
import { dragAndDropHelper } from "@/builder/helpers";
import { isAreaBanned } from "@/builder/helpers/widget-restrictions";

import BorderContainer from "./BorderContainer.vue";
import Section from "./SectionProvider";

@Component({
  components: {
    Section,
    BorderContainer,
  },
})
export default class EditableArea extends Vue implements d.EditableAreaComponentProperties {
  @Prop({ required: true }) identifier: string;

  // state props
  @Prop() sectionsInArea: string[];
  @Prop() dragAndDrop: d.DragAndDrop;

  // actions
  @Prop() removeSection;
  @Prop() changeSection;

  get editableAreaClasses() {
    return {
      "ktc-editable-area": true,
      "ktc-editable-area--banned": this.isBanned,
    };
  }

  get isBanned() {
    return isAreaBanned(this.dragAndDrop.bannedContainers, this.identifier);
  }

  get showAreaDropMarker() {
    return (
      this.dragAndDrop.entity === EntityType.Section &&
      this.dragAndDrop.targetContainerIdentifier === this.identifier &&
      this.dragAndDrop.dropMarkerPosition === 0
    );
  }

  get bannedAreaMessage() {
    return this.dragAndDrop.entity === EntityType.Widget ?
      this.$_localizationService.getLocalization("areas.bannedwidgetmessage") :
      this.$_localizationService.getLocalization("areas.bannedsectionmessage");
  }

  onRemoveSection(sectionIdentifier: string) {
    const message = this.$_localizationService.getLocalization("section.remove.confirmation");

    if (confirm(message)) {
      dragAndDropHelper.removeSectionFromDragAndDrop(sectionIdentifier);
      this.removeSection(sectionIdentifier, this.identifier);
    }

    if (this.sectionsInArea.length === 1) {
      dragAndDropHelper.mountSectionToDragAndDropAfterNextTick(this, sectionIdentifier);
    }
  }

  async onChangeSection(event: CustomEvent<d.PopupListingElement>, sectionIdentifier: string) {
    dragAndDropHelper.removeSectionFromDragAndDrop(sectionIdentifier);

    await this.changeSection(
      sectionIdentifier,
      event.detail.key,
    );

    dragAndDropHelper.mountSectionToDragAndDropAfterNextTick(this, sectionIdentifier);
  }

  onSectionMarkupChanged(sectionIdentifier: string) {
    dragAndDropHelper.removeSectionFromDragAndDrop(sectionIdentifier);
    dragAndDropHelper.mountSectionToDragAndDropAfterNextTick(this, sectionIdentifier);
  }
}
</script>

<style lang="less" scoped>
.ktc-editable-area {
  position: relative;
  min-width: 100px;

  // To disable default draggable border
  &:focus {
    outline: 0;
  }

  &.ktc-editable-area--banned {
    &, * {
      cursor: not-allowed;
    }

    &:before {
      content: ' ';
      display: block;
      height: 100%;
      position: absolute;
      width: 100%;
      background-color: @color-red-70;
      opacity: 0.2;
      z-index: @banned-widget-zone-z-index;
    }
  }

  kentico-drop-marker {
    position: absolute;
    width: 100%;
  }
}

</style>
