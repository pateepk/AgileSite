<template>
  <GlobalComponentWrapper>
    <TemplateButtons
      v-if="hasTemplate || hasTemplateProperties"
      :templateIdentifier="templateIdentifier"
      :hasTemplate="hasTemplate"
      :hasTemplateProperties="hasTemplateProperties"
    />
    <PropertiesDialog v-if="showPropertiesDialog" />
  </GlobalComponentWrapper>
</template>

<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import { PageBuilderGlobalComponentWrapperComponentState } from "../declarations";
import GlobalComponentWrapper from "./GlobalComponentWrapperProvider";
import PropertiesDialog from "./PropertiesDialogProvider";
import TemplateButtons from "./TemplateButtons.vue";

@Component({
  components: {
    PropertiesDialog,
    TemplateButtons,
    GlobalComponentWrapper,
  },
})
export default class PageBuilderGlobalComponentWrapper extends Vue implements PageBuilderGlobalComponentWrapperComponentState {
  @Prop() readonly hasTemplate: boolean;
  @Prop() readonly hasTemplateProperties: boolean;
  @Prop() readonly templateIdentifier: string;
  @Prop() readonly showPropertiesDialog: boolean;
}
</script>
