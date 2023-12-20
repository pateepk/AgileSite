<template>
<div>
  <div class="ktc-rule-designer-rules">
    <ValidationRule v-for="ruleConfiguration in validationRuleConfigurations" :key="ruleConfiguration.validationRule.instanceIdentifier"
      :formFieldName="formFieldName"
      :isExpanded="ruleConfiguration.validationRule.instanceIdentifier === expandedValidationRuleIdentifier"
      :validationRuleConfiguration="ruleConfiguration"
      :validationRulesMetadata="validationRulesMetadata"
      @expandRule="expandRule"
      @removeRule="removeRule"
      :widgetVariantIdentifier="widgetVariantIdentifier"
    />

    <ValidationRuleConfigurationForm
    v-if="isNewRuleExpanded"
    :validationRulesMetadata="validationRulesMetadata"
    :formFieldName="formFieldName"
    :widgetVariantIdentifier="widgetVariantIdentifier"
    :newValidationRuleIdentifiers="newValidationRuleIdentifiers" />
  </div>

  <button v-if="validationRulesMetadata.length" v-on:click="newRuleExpand" class="ktc-btn ktc-btn-default">{{ this.localizationService.getLocalization("propertiespanel.validationtab.addvalidationrule") }}</button>

  <div class="ktc-alert ktc-alert-info" v-if="!validationRulesMetadata.length">
    <span class="ktc-alert-icon">
        <i class="icon-i-circle"></i>
        <span class="ktc-sr-only">Info</span>
    </span>
    <div class="ktc-alert-label">
        {{ this.localizationService.getLocalization("propertiespanel.validationtab.novalidationrules") }}
    </div>
  </div>
</div>
</template>

<script lang="ts">
import { Component, Vue, Prop } from "vue-property-decorator";

import { getService } from "@/builder/container";
import { LocalizationService } from "@/builder/declarations";
import { SERVICE_TYPES } from "@/builder/types";

import v4 from "uuid/v4";

import { ValidationRuleConfiguration, ValidationRuleMetadata, ValidationRulesComponentProperties, WidgetVariantToValidationRuleBinding } from "@/form-builder/declarations";

import ValidationRule from "./ValidationRule.vue";
import ValidationRuleConfigurationForm from "./ValidationRuleConfigurationFormProvider";

@Component({
  components: {
    ValidationRule,
    ValidationRuleConfigurationForm,
  },
})
export default class ValidationRules extends Vue implements ValidationRulesComponentProperties {
  localizationService = getService<LocalizationService>(SERVICE_TYPES.LocalizationService);

  @Prop({ default: [] })
  validationRuleConfigurations: ValidationRuleConfiguration[];

  @Prop({ required: true })
  formFieldName: string;

  @Prop({ required: true })
  widgetVariantIdentifier: string;

  @Prop({ default: [] })
  validationRulesMetadata: ValidationRuleMetadata[];

  @Prop({ default: "" })
  expandedValidationRuleIdentifier: string;

  @Prop()
  newValidationRuleIdentifiers: WidgetVariantToValidationRuleBinding[];

  @Prop()
  expandValidationRule;

  @Prop()
  createNewValidationRuleIdentifier;

  @Prop()
  freezeWidgetSelection;

  created() {
    // Prepare instanceIdentifier for yet non-existing validation rule
    this.createNewValidationRuleIdentifier(v4(), this.widgetVariantIdentifier);
  }

  newRuleExpand() {
    this.expandValidationRule(this.getWidgetVariantToNewValidationRuleBinding().validationRuleIdentifier);
  }

  expandRule(ruleIdentifier) {
    this.expandValidationRule(ruleIdentifier);
  }

  removeRule(ruleIdentifier) {
    if (confirm(this.localizationService.getLocalization("propertiespanel.validationtab.confirmationmessage.removevalidationrule"))) {
      this.$emit("removeValidationRule", ruleIdentifier, this.widgetVariantIdentifier);
    }
  }

  get isNewRuleExpanded() {
    const widgetVariantToNewValidationRuleBinding = this.getWidgetVariantToNewValidationRuleBinding();
    return widgetVariantToNewValidationRuleBinding && this.expandedValidationRuleIdentifier === widgetVariantToNewValidationRuleBinding.validationRuleIdentifier;
  }

  getWidgetVariantToNewValidationRuleBinding() {
    return this.newValidationRuleIdentifiers.filter((x) => x.widgetVariantIdentifier === this.widgetVariantIdentifier)[0];
  }
}
</script>
