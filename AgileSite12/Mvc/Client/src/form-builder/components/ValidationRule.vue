<template>
<div>
  <div class="ktc-rule-row" v-show="!isExpanded">
    <span class="ktc-rule-expandable-title" v-on:click="expandContent">{{ validationRuleConfiguration.validationRule.title }}</span>
    <div class="ktc-rule-actions">
      <button type="button" v-on:click="remove" class="ktc-remove-rule ktc-icon-only ktc-btn-icon ktc-btn" :title="removeRuleLabel">
        <i aria-hidden="true" class="icon-bin"></i>
        <span class="ktc-sr-only">{{ removeRuleLabel }}</span>
      </button>
    </div>
  </div>

  <ValidationRuleConfigurationForm
    v-if="isExpanded"
    :editedValidationRuleConfiguration="validationRuleConfiguration"
    :validationRulesMetadata="validationRulesMetadata"
    :widgetVariantIdentifier="widgetVariantIdentifier"
    :formFieldName="formFieldName" />
</div>
</template>

<script lang="ts">
import { Component, Vue, Prop } from "vue-property-decorator";

import { getService } from "@/builder/container";
import { LocalizationService } from "@/builder/declarations";
import { SERVICE_TYPES } from "@/builder/types";

import { ValidationRuleConfiguration, ValidationRuleMetadata } from "@/form-builder/declarations";

import ValidationRuleConfigurationForm from "./ValidationRuleConfigurationFormProvider";

@Component({
  components: {
    ValidationRuleConfigurationForm,
  },
})
export default class ValidationRule extends Vue {
  removeRuleLabel = getService<LocalizationService>(SERVICE_TYPES.LocalizationService).getLocalization("propertiespanel.validationtab.removerule");

  @Prop({ required: true })
  formFieldName: string;

  @Prop({ default: false })
  isExpanded: boolean;

  @Prop({ required: true })
  widgetVariantIdentifier: string;

  @Prop({ required: true })
  validationRuleConfiguration: ValidationRuleConfiguration;

  @Prop({ default: [] })
  validationRulesMetadata: ValidationRuleMetadata[];

  expandContent() {
    this.$emit("expandRule", this.validationRuleConfiguration.validationRule.instanceIdentifier);
  }

  remove() {
    this.$emit("removeRule", this.validationRuleConfiguration.validationRule.instanceIdentifier);
  }
}
</script>
