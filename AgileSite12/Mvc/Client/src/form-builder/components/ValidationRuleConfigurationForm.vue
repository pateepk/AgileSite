<template>
  <div class="ktc-rule-row ktc-edit-rule-row">
    <select class="ktc-form-control" v-model="model">
      <option v-for="rule in validationRulesWithMetadata" :key="rule.metadata.identifier" v-bind:value="rule">
      {{ rule.metadata.name }}
      </option>
    </select>
    <span class="ktc-rule-description">{{ model.metadata.description }}</span>
    <div :id="renderRuleMarkupId">
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Vue, Prop, Watch } from "vue-property-decorator";

import { getValidationRuleMetadataMarkup, renderMarkup } from "@/form-builder/api";

import { ValidationRuleConfiguration, ValidationRuleMetadata, ValidationRuleWithMetadata,
  WidgetVariantToValidationRuleBinding, ValidationRuleConfigurationFormComponentProperties } from "@/form-builder/declarations";

@Component
export default class ValidationRuleConfigurationForm extends Vue implements ValidationRuleConfigurationFormComponentProperties {
  validationRulesWithMetadata: ValidationRuleWithMetadata[] = [];
  validationRuleInstanceIdentifier = "";
  renderRuleMarkupId = "";

  model: ValidationRuleWithMetadata = null;

  @Prop({ default: [] })
  validationRulesMetadata: ValidationRuleMetadata[];

  @Prop({ default: null })
  editedValidationRuleConfiguration: ValidationRuleConfiguration;

  @Prop({ required: true })
  widgetVariantIdentifier: string;

  @Prop({ required: true })
  formFieldName: string;

  @Prop()
  newValidationRuleIdentifiers: WidgetVariantToValidationRuleBinding[];

  @Prop()
  refreshPropertiesPanelsNotifier: boolean;

  created() {
    this.validationRuleInstanceIdentifier = this.editedValidationRuleConfiguration ?
     this.editedValidationRuleConfiguration.validationRule.instanceIdentifier :
     this.newValidationRuleIdentifiers.filter((x) => x.widgetVariantIdentifier === this.widgetVariantIdentifier)[0].validationRuleIdentifier;

    this.renderRuleMarkupId = `jqueryLoaded-validationRuleConfigurationForm-${this.validationRuleInstanceIdentifier}`;

    this.validationRulesWithMetadata = this.validationRulesMetadata.map((metadata) => {
      if (this.editedValidationRuleConfiguration && this.editedValidationRuleConfiguration.identifier === metadata.identifier) {
        return {
          metadata,
          validationRule: {
            ...this.editedValidationRuleConfiguration.validationRule,
          },
        };
      } else {
        return {
          metadata,
          validationRule: {
            instanceIdentifier: this.validationRuleInstanceIdentifier,
          },
        };
      }
    });

    if (this.editedValidationRuleConfiguration !== null) {
      // Editing existing validation rule, other available validation rule types does not include edited one
      this.model = this.validationRulesWithMetadata.filter((x) => x.metadata.identifier === this.editedValidationRuleConfiguration.identifier)[0];
    } else if (this.validationRulesMetadata.length > 0) {
      this.model = this.validationRulesWithMetadata[0];
    }
  }

  @Watch("model")
  @Watch("refreshPropertiesPanelsNotifier")
  valueInSelectorChanged() {
    // Acquire form to configure selected validation rule
    getValidationRuleMetadataMarkup(this.widgetVariantIdentifier, this.model.metadata.identifier, this.model.validationRule, this.formFieldName).then((result) => {
      renderMarkup(this.renderRuleMarkupId, result);
    });
  }
}
</script>
