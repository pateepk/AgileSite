import { Component, Element, Event, EventEmitter, Prop, State, Watch } from "@stencil/core";
import { AxiosInstance } from "axios";

import { BuilderConfig } from "@/builder/BuilderConfig";
import { Entities, LocalizationService, PersonalizationConditionTypeMetadata, PopupListingElement } from "@/builder/declarations/index";
import { getPosition } from "@/builder/helpers/position";
import { ComponentPosition } from "@/builder/store/types";
import { Theme } from "@/builder/types";
import { VariantListingElement } from "@/page-builder/declarations";

enum PersonalizationDialogMode {
  ConditionTypeListing,
  VariantListing,
  VariantConfiguration,
}

@Component({
  tag: "kentico-personalization-button",
  styleUrl: "personalization-button.less",
  shadow: false,
})
export class PersonalizationButton {

  @Element() thisElement: HTMLElement;

  @State() selectedConditionType: string;
  @State() selectedVariantIdentifier: string;
  @State() variantParameters: object;
  @State() variantName: string;
  @State() dialogMode: PersonalizationDialogMode = PersonalizationDialogMode.ConditionTypeListing;

  @Prop({ context: "getString" }) getString: any;

  @Prop() localizationService: LocalizationService;
  @Prop() httpClient: AxiosInstance;
  @Prop() builderConfig: BuilderConfig;
  @Prop() showPersonalizationPopup: boolean;
  @Prop() popupItems: PopupListingElement[];
  @Prop() activeItemIdentifier: string;
  @Prop({ mutable: true }) personalizationPopupPosition: ComponentPosition;
  @Prop() personalizationConditionTypes: Entities<PersonalizationConditionTypeMetadata>;
  @Prop() conditionType?: string;

  // actions
  @Prop() deleteVariant: any;
  @Prop() addVariant: any;
  @Prop() updateVariant: any;
  @Prop() selectVariant: any;
  @Prop() changeVariantsPriority: any;

  @Event() openPersonalizationPopup: EventEmitter<void>;
  @Event() closePersonalizationPopup: EventEmitter<void>;

  get personalizationTooltip(): string {
    return this.getString(this.localizationService, "widget.personalizeTooltip");
  }

  get currentConditionType(): string {
    return this.selectedConditionType ? this.selectedConditionType : this.conditionType;
  }

  get hasConditionType(): boolean {
    return !!this.conditionType;
  }

  get headerText(): string {
    switch (this.dialogMode) {
      case PersonalizationDialogMode.ConditionTypeListing:
        return this.getString(this.localizationService, "widget.conditiontypepopup.header");
      case PersonalizationDialogMode.VariantListing:
        return this.personalizationConditionTypes[this.currentConditionType].name;
      case PersonalizationDialogMode.VariantConfiguration:
        return this.getString(this.localizationService, !this.selectedVariantIdentifier ? "variant.add" : "variant.edit");
    }
  }

  get conditionTypeListingContentHeaderText(): string {
    return this.getString(this.localizationService, "widget.conditiontypepopup.conditiontypeheader");
  }

  get variantListingContentHeaderText(): string {
    return this.getString(this.localizationService, "widget.conditiontypepopup.variantheader");
  }

  @Watch("showPersonalizationPopup")
  showPersonalizationPopupChanged(): void {
    this.reset(false);
  }

  reset = (updatePosition: boolean): void => {
    if (!this.hasConditionType) {
      this.dialogMode = PersonalizationDialogMode.ConditionTypeListing;
      this.selectedConditionType = null;
    } else {
      this.dialogMode = PersonalizationDialogMode.VariantListing;
    }

    this.selectedVariantIdentifier = null;
    this.variantParameters = null;
    this.variantName = null;

    if (updatePosition) {
      this.updatePositionDelayed();
    }
  }

  selectConditionTypeHandler = (event: CustomEvent<PopupListingElement>): void => {
    event.stopPropagation();
    this.selectedConditionType = event.detail.key;
    this.dialogMode = PersonalizationDialogMode.VariantConfiguration;
    this.updatePositionDelayed();
  }

  updatePosition = (): ComponentPosition => {
    return (this.personalizationPopupPosition = getPosition(this.thisElement.getBoundingClientRect()));
  }

  updatePositionDelayed = (): void => {
    window.requestAnimationFrame(this.updatePosition);
  }

  closePersonalizationPopupHandler = (event: CustomEvent<any>): void => {
    event.stopPropagation();
    this.closePersonalizationPopup.emit();
  }

  onAddVariantButtonClick = (event: CustomEvent<void>): void => {
    event.stopPropagation();
    this.dialogMode = PersonalizationDialogMode.VariantConfiguration;
    this.updatePositionDelayed();
  }

  onEditVariantButtonClick = (variant: VariantListingElement): void => {
    this.selectedVariantIdentifier = variant.key;
    this.variantParameters = variant.parameters;
    this.variantName = variant.name;
    this.dialogMode = PersonalizationDialogMode.VariantConfiguration;
    this.updatePositionDelayed();
  }

  render = (): JSX.Element =>
  <div class="ktc-widget-header-personalization-wrapper">
    <div class="ktc-widget-header-personalization">
      <a class="ktc-widget-header-icon" title={this.personalizationTooltip} onClick={() => this.openPersonalizationPopup.emit()}>
        {
          this.hasConditionType
            ? <i aria-hidden="true" class="icon-personalisation-variants"></i>
            : <i aria-hidden="true" class="icon-personalisation"></i>
        }
      </a>
      {
        this.showPersonalizationPopup &&
        <kentico-pop-up-container
          localizationService={this.localizationService}
          headerTitle={this.headerText}
          theme={Theme.Widget}
          showBackButton={this.dialogMode === PersonalizationDialogMode.VariantConfiguration}
          position={this.personalizationPopupPosition}
          onClick={(e) => e.stopPropagation()}
          onClosePopup={this.closePersonalizationPopupHandler}
          onBackClick={this.reset.bind(this, true)}
        >
          <div slot="pop-up-content">
            {this.renderPopupContent()}
          </div>
          {
            this.dialogMode === PersonalizationDialogMode.VariantListing ?
              <div slot="pop-up-footer" class="ktc-popup-footer">
                <kentico-submit-button
                  onButtonClick={this.onAddVariantButtonClick}
                  buttonText={this.getString(this.localizationService, "variant.add")}
                  buttonTooltip={this.getString(this.localizationService, "variant.addTooltip")}
                />
              </div>
              : null
          }
        </kentico-pop-up-container>
      }
    </div>
  </div>

  renderPopupContent = (): JSX.Element => {
    switch (this.dialogMode) {
      case PersonalizationDialogMode.VariantListing:
        return (
          <div>
            <div class="ktc-pop-up-content-header">
              {this.variantListingContentHeaderText}
            </div>
            <kentico-variant-listing
              activeItemIdentifier={this.activeItemIdentifier}
              variants={this.popupItems as VariantListingElement[]}
              localizationService={this.localizationService}
              deleteVariant={this.deleteVariant}
              editVariant={this.onEditVariantButtonClick}
              selectVariant={this.selectVariant}
              changeVariantsPriority={this.changeVariantsPriority}
            />
          </div>
        );
      case PersonalizationDialogMode.ConditionTypeListing:
        return (
          <div>
            <div class="ktc-pop-up-content-header">
              {this.conditionTypeListingContentHeaderText}
            </div>
            <kentico-pop-up-listing
              activeItemIdentifier={this.activeItemIdentifier}
              items={this.popupItems}
              onSelectItem={this.selectConditionTypeHandler}
              singleColumn={true}
            />
          </div>
        );
      case PersonalizationDialogMode.VariantConfiguration:
        const hint = this.getString(this.localizationService, this.personalizationConditionTypes[this.currentConditionType].hint);
        return (
          <div>
            {hint &&
              <div class="ktc-pop-up-content-header">
                {hint}
              </div>
            }
            <kentico-personalization-configuration
              builderConfig={this.builderConfig}
              httpClient={this.httpClient}
              localizationService={this.localizationService}
              personalizationConditionTypes={this.personalizationConditionTypes}
              selectedConditionType={this.currentConditionType}
              variantIdentifier={this.selectedVariantIdentifier}
              variantName={this.variantName}
              variantParameters={this.variantParameters}
              configurationLoaded={this.updatePositionDelayed}
              addVariant={this.addVariant}
              updateVariant={this.updateVariant}
            />
          </div>
        );
    }
  }
}
