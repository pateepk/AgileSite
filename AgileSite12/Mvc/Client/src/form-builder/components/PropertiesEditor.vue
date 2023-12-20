<template>
  <div class="ktc-form-builder-properties-panel">
    <PropertiesEditorForm v-for="widget in widgets"
      :key="widget.identifier"
      :widgetIdentifier="widget.identifier"
      :widgetType="widget.type"
      :widgetProperties="getWidgetProperties(widget)"
      :validationRulesMetadata="getCompatibleValidationRulesMetadata(widget)"
      v-if="widget.identifier === requestedWidgetIdentifier || markupsLoaded[widget.identifier]"
      v-show="widget.identifier === displayedWidgetIdentifier && markupsLoaded[widget.identifier]"
      @mousedown.native="onMouseDown"
      @contentLoaded="propertiesEditorFormContentLoaded"
      @removeValidationRule="removeValidationRule">
    </PropertiesEditorForm>
    <slot v-if="!displayedWidgetIdentifier"></slot>
  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from "vue-property-decorator";

import v4 from "uuid/v4";

import * as d from "@/builder/declarations";
import * as f from "@/form-builder/declarations";
import {
  FORM_BUILDER_FREEZE_UI, FORM_BUILDER_UNFREEZE_UI, PROPERTIES_STATE_CHANGED_EVENT,
  VALIDATION_RULE_STATE_CHANGED_EVENT, VALIDATION_RULE_CONFIGURATION_CLOSED, VISIBILITY_CONDITION_STATE_CHANGED_EVENT,
  INVALID_COMPONENT_TYPE_NAME,
  } from "@/form-builder/constants";
import PropertiesEditorForm from "@/form-builder/components/PropertiesEditorFormProvider";

@Component({
  components: {
    PropertiesEditorForm,
  },
})
export default class PropertiesEditor extends Vue implements f.PropertiesEditorComponentProperties {

  // Identifier of widget whose properties panel is requested. Once loaded, it is displayed, unless the selection changed in the meantime
  requestedWidgetIdentifier = null;

  // Identifier of widget whose properties panel is displayed
  displayedWidgetIdentifier = null;

  // Array of flags indicating whether widget markup was already loaded. Loaded widgets are never unmounted to prevent redundant server requests
  markupsLoaded = [];

  // state props
  @Prop() widgetVariants: d.Entities<d.WidgetVariant>;
  @Prop() widgets: d.Entities<d.Widget>;
  @Prop() widgetSelection: d.WidgetSelection;
  @Prop() validationRulesMetadataPerType: any;
  @Prop() widgetMetadata: d.Entities<d.WidgetMetadata>;
  @Prop() savingInProgress: boolean;

  // actions
  @Prop() freezeWidgetSelection;
  @Prop() setWidgetProperties;
  @Prop() thawWidgetSelection;
  @Prop() newValidationRuleAdded;
  @Prop() expandValidationRule;
  @Prop() disableWidgetClickAway;

  mounted() {
    document.addEventListener(FORM_BUILDER_FREEZE_UI, this.freezeWidgetSelectionEventCallback);
    document.addEventListener(PROPERTIES_STATE_CHANGED_EVENT, this.propertiesStateChangedEventCallback);
    document.addEventListener(VALIDATION_RULE_STATE_CHANGED_EVENT, this.validationRuleStateChangedEventCallback);
    document.addEventListener(FORM_BUILDER_UNFREEZE_UI, this.thawWidgetSelectionEventCallback);
    document.addEventListener(VALIDATION_RULE_CONFIGURATION_CLOSED, this.closeValidationRule);
    document.addEventListener(VISIBILITY_CONDITION_STATE_CHANGED_EVENT, this.visibilityConditionChangedEventCallback);
  }

  beforeDestroy() {
    document.removeEventListener(FORM_BUILDER_FREEZE_UI, this.freezeWidgetSelectionEventCallback);
    document.removeEventListener(PROPERTIES_STATE_CHANGED_EVENT, this.propertiesStateChangedEventCallback);
    document.removeEventListener(VALIDATION_RULE_STATE_CHANGED_EVENT, this.validationRuleStateChangedEventCallback);
    document.removeEventListener(FORM_BUILDER_UNFREEZE_UI, this.thawWidgetSelectionEventCallback);
    document.removeEventListener(VALIDATION_RULE_CONFIGURATION_CLOSED, this.closeValidationRule);
    document.removeEventListener(VISIBILITY_CONDITION_STATE_CHANGED_EVENT, this.visibilityConditionChangedEventCallback);
  }

  onMouseDown(): void {
    this.disableWidgetClickAway();
  }

  freezeWidgetSelectionEventCallback() {
    this.freezeWidgetSelection();
  }

  async propertiesStateChangedEventCallback(evt) {
    const widgetVariant = this.getWidgetVariant(evt.detail.identifier);
    const widget = this.getWidget(widgetVariant.identifier);

    const newProperties = {
      ...widgetVariant.properties,
      ...evt.detail.properties,
    };

    await this.setWidgetProperties(widget.identifier, newProperties);
    this.thawWidgetSelection();
  }

  async visibilityConditionChangedEventCallback(evt) {
    const instanceIdentifier = evt.detail.identifier;
    const visibilityConditionConfiguration = evt.detail.visibilityConditionConfiguration;

    const widgetVariant = this.getWidgetVariant(instanceIdentifier);
    const widget = this.getWidget(widgetVariant.identifier);

    const widgetProperties: any = { ...widgetVariant.properties };
    widgetProperties.visibilityConditionConfiguration = visibilityConditionConfiguration;
    const newProperties = {
      ...widgetProperties,
    };

    await this.setWidgetProperties(widget.identifier, newProperties);
    this.thawWidgetSelection();
  }

  // Creates or updates validation rule for a form component
  async validationRuleStateChangedEventCallback(evt) {
    const instanceIdentifier = evt.detail.identifier;
    const validationRuleConfiguration = evt.detail.validationRuleConfiguration;
    const validationRuleInstance = validationRuleConfiguration.validationRule;

    const widgetVariant = this.getWidgetVariant(instanceIdentifier);
    const widget = this.getWidget(widgetVariant.identifier);

    const widgetProperties: any = { ...widgetVariant.properties };
    const validationRuleConfigurations = widgetProperties.validationRuleConfigurations;
    let existingValidationRuleConfiguration = validationRuleConfigurations.filter((v: any) => v.validationRule.instanceIdentifier === validationRuleInstance.instanceIdentifier)[0];

    let newValidationRules = null;
    if (!existingValidationRuleConfiguration) {
      newValidationRules = [...validationRuleConfigurations, validationRuleConfiguration];
      this.newValidationRuleAdded(validationRuleConfiguration.validationRule.instanceIdentifier, v4(), instanceIdentifier);
    } else {
      existingValidationRuleConfiguration = {
        ...existingValidationRuleConfiguration,
        ...validationRuleConfiguration,
      };

      newValidationRules = [
        ...validationRuleConfigurations.map((x) =>
        (x.validationRule.instanceIdentifier !== existingValidationRuleConfiguration.validationRule.instanceIdentifier) ? x : existingValidationRuleConfiguration),
      ];

      // Close validation rule after updating existing rule
      this.closeValidationRule();
    }

    widgetProperties.validationRuleConfigurations = newValidationRules;
    const newProperties = {
      ...widgetProperties,
    };

    await this.setWidgetProperties(widget.identifier, newProperties);
    this.thawWidgetSelection();
  }

  async removeValidationRule(validationRuleIdentifier: string, widgetVariantIdentifier: string) {
    const widgetVariant = this.getWidgetVariant(widgetVariantIdentifier);
    const widget = this.getWidget(widgetVariant.identifier);

    const widgetProperties: any = { ...widgetVariant.properties };
    const validationRuleConfigurations = widgetProperties.validationRuleConfigurations;
    const newValidationRules = validationRuleConfigurations.filter((v: any) => v.validationRule.instanceIdentifier !== validationRuleIdentifier);

    widgetProperties.validationRuleConfigurations = newValidationRules;
    const newProperties = {
      ...widgetProperties,
    };

    await this.setWidgetProperties(widget.identifier, newProperties);
  }

  closeValidationRule() {
    this.expandValidationRule(null);
  }

  getWidgetVariant(instanceIdentifier: string) {
    return Object.values(this.widgetVariants).filter((v: any) => v.properties.guid === instanceIdentifier)[0];
  }

  getWidget(widgetVariantIdentifier: string) {
    return Object.values(this.widgets).filter((w) => w.variants[0] === widgetVariantIdentifier)[0];
  }

  thawWidgetSelectionEventCallback() {
    this.thawWidgetSelection();
  }

  getWidgetProperties(widget) {
    return this.widgetVariants[widget.variants[0]].properties;
  }

  getCompatibleValidationRulesMetadata(widget): f.ValidationRuleMetadata[] {
    const widgetMetadata: any = this.widgetMetadata[widget.type];
    return this.validationRulesMetadataPerType[widgetMetadata.valueType];
  }

  propertiesEditorFormContentLoaded(data) {
    this.markupsLoaded[data.identifier] = true;
    if (this.requestedWidgetIdentifier === data.identifier) {
      this.displayedWidgetIdentifier = data.identifier;
    }
  }

  @Watch("widgetSelection.identifier")
  @Watch("savingInProgress")
  loadPropertiesEditor() {
    if (this.savingInProgress) {
      return;
    }

    const selectedWidget = this.widgets[this.widgetSelection.identifier];
    if (!selectedWidget || selectedWidget.type === INVALID_COMPONENT_TYPE_NAME) {
      this.displayedWidgetIdentifier = null;
      return;
    }

    this.requestedWidgetIdentifier = this.widgetSelection.identifier;

    if (this.markupsLoaded[this.requestedWidgetIdentifier]) {
      this.displayedWidgetIdentifier = this.requestedWidgetIdentifier;
    }

    if (!this.widgetSelection.identifier) {
      this.displayedWidgetIdentifier = null;
    }
  }
}
</script>

<style>
</style>
