<template>
  <Tabs>
    <Tab identifier="propertiesTab" name="propertiespanel.propertiestab.name" :selected="true">
      <div :id="propertiesTabIdentifier"></div>
    </Tab>
    <Tab identifier="validationTab" name="propertiespanel.validationtab.name" :selected="false">
      <div class="ktc-form-builder-tab-content-inner">
        <div id="form-builder-validation" style="display: block;">
          <div class="ktc-label-property ktc-Top">
            <span class="ktc-control-label">{{ this.localizationService.getLocalization("propertiespanel.validationtab.rules") }}</span>
          </div>
          <div class="ktc-field-property">
            <span class="ktc-field-rule-designer">
              <ValidationRules
              :validationRuleConfigurations="widgetProperties.validationRuleConfigurations"
              :validationRulesMetadata="validationRulesMetadata"
              :formFieldName="widgetProperties.name"
              :widgetVariantIdentifier="widgetProperties.guid"
              @removeValidationRule="removeValidationRule"
              />
            </span>
          </div>
        </div>
        <div :id="validationTabIdentifier"></div>
      </div>
    </Tab>
    <Tab identifier="visibilityTab" name="propertiespanel.visibilitytab.name">
      <VisibilityConditionEditorForm
        :formFieldName="widgetProperties.name"
        :widgetVariantIdentifier="widgetProperties.guid"
        :visibilityConditionConfiguration="widgetProperties.visibilityConditionConfiguration"
        class="ktc-form-builder-tab-content-inner"
      />
    </Tab>
  </Tabs>
</template>

<script lang="ts">
import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import { getPropertiesEditorMarkup } from "@/form-builder/api";

import { getService } from "@/builder/container";
import { LocalizationService } from "@/builder/declarations";
import { SERVICE_TYPES } from "@/builder/types";

import { renderMarkup } from "@/form-builder/api";
import { PropertiesEditorFormComponentState, ValidationRuleMetadata } from "@/form-builder/declarations";

import ValidationRules from "./ValidationRulesProvider";
import VisibilityConditionEditorForm from "./VisibilityConditionEditorFormProvider";
import Tabs from "./Tabs.vue";
import Tab from "./Tab.vue";

@Component({
  components: {
    ValidationRules,
    Tab,
    Tabs,
    VisibilityConditionEditorForm,
  },
})
export default class PropertiesEditorForm extends Vue implements PropertiesEditorFormComponentState {
  propertiesTabIdentifier = `jqueryLoaded-propertiesTab-${this.widgetProperties.guid}`;
  validationTabIdentifier = `jqueryLoaded-validationTab-${this.widgetProperties.guid}`;

  localizationService = getService<LocalizationService>(SERVICE_TYPES.LocalizationService);

  @Prop({ required: true })
  widgetIdentifier: string;

  @Prop({ required: true })
  widgetType: string;

  @Prop({ required: true })
  widgetProperties: any;

  @Prop({ default: () => [] })
  validationRulesMetadata: ValidationRuleMetadata[];

  @Prop()
  refreshPropertiesPanelsNotifier: boolean;

  mounted() {
    this.renderPropertiesEditor();
  }

  @Watch("refreshPropertiesPanelsNotifier")
  renderPropertiesEditor() {
    const formComponent = {
      identifier: this.widgetIdentifier,
      type: this.widgetType,
      properties: this.widgetProperties,
    };

    // Promise has to be used instead async/await so the processing can continue and create a div for rendering markup
    getPropertiesEditorMarkup(formComponent).then((result) => {
      renderMarkup(this.propertiesTabIdentifier, result);

      this.$emit("contentLoaded", { identifier: this.widgetIdentifier });
    });
  }

  removeValidationRule(validationRuleIdentifier: string, widgetVariantIdentifier: string) {
    this.$emit("removeValidationRule", validationRuleIdentifier, widgetVariantIdentifier);
  }
}
</script>
