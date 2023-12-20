<template>
<div class="ktc-form-builder-visibility-section">
  <ButtonGroupSelector :onTabChange="onTabChange">
    <Tab :identifier="tabAlwaysIdentifier" name="propertiespanel.visibilityconditions.tab.always" :selected="visibilityConditionConfiguration == null">
    </Tab>
    <Tab :identifier="neverVisibleVisibilityCondition" name="propertiespanel.visibilityconditions.tab.never" :selected="visibilityConditionConfiguration && visibilityConditionConfiguration.identifier == neverVisibleVisibilityCondition">
    </Tab>
    <Tab identifier="condition" ref="conditionTab" name="propertiespanel.visibilityconditions.tab.condition" :selected="visibilityConditionConfiguration && visibilityConditionConfiguration.identifier != neverVisibleVisibilityCondition">
      <div :id="renderConditionMarkupId">
      </div>
    </Tab>
  </ButtonGroupSelector>
</div>
</template>

<script lang="ts">
import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import { getVisibilityConditionMarkup, renderMarkup } from "@/form-builder/api";

import { VISIBILITY_CONDITION_STATE_CHANGED_EVENT } from "@/form-builder/constants";
import { VisibilityConditionEditorFormState } from "@/form-builder/declarations";

import ButtonGroupSelector from "./ButtonGroupSelector.vue";
import Tab from "./Tab.vue";

@Component({
  components: {
    Tab,
    ButtonGroupSelector,
    VisibilityConditionEditorForm,
  },
})
export default class VisibilityConditionEditorForm extends Vue
  implements VisibilityConditionEditorFormState {
  renderConditionMarkupId = "";

  tabAlwaysIdentifier = "always";

  // Identifier of the visibility condition that is used to always hide a form component
  // This identifier has to match identifier used for registration of 'NeverVisibleVisibilityCondition' on the server side
  neverVisibleVisibilityCondition = "Kentico.NeverVisible";

  // Currently selected widget
  @Prop({ required: true })
  widgetVariantIdentifier: string;

  @Prop({ required: true })
  visibilityConditionConfiguration: any;

  @Prop({ required: true })
  formFieldName: string;

  @Prop() refreshPropertiesPanelsNotifier: boolean;

  created() {
    this.renderConditionMarkupId = `jqueryLoaded-visibilityCondition-${
      this.widgetVariantIdentifier
    }`;
  }

  mounted() {
    this.refreshMarkup();
  }

  onTabChange(selectedTab: Tab) {
    let visibilityCondition = null;
    if (selectedTab.identifier === this.tabAlwaysIdentifier) {
      visibilityCondition = {
        detail: {
          identifier: this.widgetVariantIdentifier,
          visibilityConditionConfiguration: null,
        },
      };
    } else if (
      selectedTab.identifier === this.neverVisibleVisibilityCondition
    ) {
      visibilityCondition = {
        detail: {
          identifier: this.widgetVariantIdentifier,
          visibilityConditionConfiguration: {
            identifier: this.neverVisibleVisibilityCondition,
            visibilityCondition: {},
          },
        },
      };
    } else {
      this.$nextTick(this.refreshMarkup);
      return;
    }

    // When the tab is changed to 'Always' or 'Never' dispatch event to save the properties
    document.dispatchEvent(
      new CustomEvent(
        VISIBILITY_CONDITION_STATE_CHANGED_EVENT,
        visibilityCondition,
      ),
    );
  }

  @Watch("refreshPropertiesPanelsNotifier")
  async refreshMarkup() {
    if ((this.$refs.conditionTab as Tab).isActive) {
      const result = await getVisibilityConditionMarkup(
        this.widgetVariantIdentifier,
        this.formFieldName,
      );

      renderMarkup(this.renderConditionMarkupId, result);
    }
  }
}
</script>
