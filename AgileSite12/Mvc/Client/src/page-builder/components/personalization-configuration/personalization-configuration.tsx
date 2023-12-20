import { Component, Element, Prop } from "@stencil/core";
import { AxiosInstance } from "axios";

import { BuilderConfig } from "@/builder/BuilderConfig";
import { Entities, LocalizationService, PersonalizationConditionTypeMetadata } from "@/builder/declarations/index";
import { arrayHelper } from "@/builder/helpers/index";
import { removeScriptElements, renderMarkup } from "@/builder/helpers/markup-helper";
import { logger } from "@/builder/logger/index";

@Component({
  tag: "kentico-personalization-configuration",
  styleUrl: "personalization-configuration.less",
  shadow: false,
})
export class PersonalizationConfiguration {
  variantNamePropertyName: string = "variantName";
  configurationElement: HTMLElement;
  configurationForm: HTMLFormElement;
  configurationMarkup: string;
  externalScriptIdentifier: string = "ktc-personalization-configuration-script";

  @Element()
  hostElement: HTMLElement;

  @Prop({ context: "getString" })
  getString: (localizationService: LocalizationService, key: string, ...parameters: any[]) => string;

  @Prop({ context: "ensureStyles" })
  ensureStyles: (markup: string, attributeName: string) => string;

  @Prop() localizationService: LocalizationService;
  @Prop() builderConfig: BuilderConfig;
  @Prop() httpClient: AxiosInstance;
  @Prop() personalizationConditionTypes: Entities<PersonalizationConditionTypeMetadata>;
  @Prop() selectedConditionType: string;
  @Prop() variantIdentifier: string;
  @Prop() variantName: string;
  @Prop() variantParameters: object;

  // functions
  @Prop() addVariant: (variantName: string, conditionType: string, conditionTypeParameters: object) => void;
  @Prop() updateVariant: (variantName: string, conditionTypeParameters: object, variantIdentifier: string) => void;
  @Prop() configurationLoaded: () => void;

  get buttonText(): string {
    const resourceKey = this.variantIdentifier ? "variant.apply" : "variant.add";
    return this.getString(this.localizationService, resourceKey);
  }

  get buttonTooltip(): string {
    const resourceKey = this.variantIdentifier ? "variant.applyTooltip" : "variant.addTooltip";
    return this.getString(this.localizationService, resourceKey);
  }

  componentWillLoad() {
    return this.getConditionMarkup();
  }

  componentDidLoad() {
    this.renderNewMarkup();
  }

  componentDidUnload() {
    removeScriptElements(this.externalScriptIdentifier);
  }

  logError = (resourceKey: string): void => {
    const error = new Error(this.getString(this.localizationService, resourceKey));
    logger.logExceptionWithMessage(error, this.getString(this.localizationService, "errors.generalerror"));
  }

  getConditionMarkup = async (): Promise<void> => {
    try {
      const markupUrl = this.personalizationConditionTypes[this.selectedConditionType].markupUrl;

      const postedData = this.variantParameters || {};
      const data = this.builderConfig.getComponentMarkupData({...postedData, [this.variantNamePropertyName]: this.variantName});

      const response = await this.httpClient.post(markupUrl, data);
      this.configurationMarkup = this.ensureStyles(response.data, this.hostElement.nodeName);
    } catch (exception) {
      const errorMessage = this.getString(this.localizationService, "errors.generalerror");
      this.configurationMarkup = `<div class="ktc-variant-configuration-error">${errorMessage}</div>`;

      logger.logExceptionWithMessage(exception, errorMessage);
    }
  }

  renderNewMarkup = (): void => {
    renderMarkup(this.configurationMarkup, this.configurationElement, this.externalScriptIdentifier);
    this.configurationForm = this.configurationElement.getElementsByTagName("form")[0];
    if (!this.configurationForm) {
      this.logError("conditiontype.missingConfigForm");
      return;
    }
    this.configurationForm.addEventListener("submit", this.onSubmit);
    this.configurationLoaded && this.configurationLoaded();
  }

  onSubmit = async (event: Event): Promise<void> => {
    let response;

    event.preventDefault();

    if (!this.configurationForm) {
      this.logError("conditiontype.missingConfigForm");
      return;
    }

    try {
      response = await this.httpClient.post(this.configurationForm.action, new FormData(this.configurationForm));
    } catch (error) {
      logger.logExceptionWithMessage(error, this.getString(this.localizationService, "errors.generalerror"));
      return;
    }

    if (arrayHelper.contains(response.headers["content-type"], "text/html")) {
      this.configurationMarkup = this.ensureStyles(response.data, this.hostElement.nodeName);
      removeScriptElements(this.externalScriptIdentifier);
      this.renderNewMarkup();
      return;
    }

    if (!arrayHelper.contains(response.headers["content-type"], "application/json")) {
      this.logError("conditiontype.wrongFormPostResponse");
      return;
    }

    const formData = response.data;
    const { [this.variantNamePropertyName]: variantName, ...formDataWithoutvariantName } = formData;

    if (variantName) {
      const trimmedVariantName = variantName.trim();
      if (this.variantIdentifier) {
        this.updateVariant(trimmedVariantName, formDataWithoutvariantName, this.variantIdentifier);
      } else {
        this.addVariant(trimmedVariantName, this.selectedConditionType, formDataWithoutvariantName);
      }
    } else {
      this.logError("conditiontype.missingVariantName");
    }
  }

  render = () =>
    <div class="ktc-variant-configuration ktc-form-wrapper">
      <div ref={(el) => (this.configurationElement = el)} class="ktc-form"/>
      <div class="ktc-configuration-footer">
        <kentico-submit-button
          onButtonClick={this.onSubmit}
          buttonText={this.buttonText}
          buttonTooltip={this.buttonTooltip}
        />
      </div>
    </div>
}
