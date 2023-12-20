import { Component, Element, Prop } from "@stencil/core";
import sortable from "html5sortable/dist/html5sortable.es";

import { LocalizationService } from "@/builder/declarations/index";
import { VariantListingElement } from "@/page-builder/declarations";

@Component({
  tag: "kentico-variant-listing",
  styleUrl: "variant-listing.less",
  shadow: true,
})
export class VariantListing {
  variantListing: HTMLElement;
  items: VariantListingElement[];
  containerClass = "ktc-variant-item-list";
  containerSortingClass = "ktc-is-sorting";
  itemHoverClass = "ktc-is-hovered";

  @Element() hostElement: HTMLElement;

  @Prop({ context: "getString" }) getString: any;

  @Prop() variants: VariantListingElement[] = [];
  @Prop() activeItemIdentifier: string;
  @Prop() localizationService: LocalizationService;

  // functions
  @Prop() selectVariant: (variant: VariantListingElement) => void;
  @Prop() editVariant: (variant: VariantListingElement) => void;
  @Prop() deleteVariant: (variantIdentifier: string) => void;
  @Prop() changeVariantsPriority: (variants: string[]) => void;

  get deleteActionIconTitle(): string {
    return this.getString(this.localizationService, "variant.delete");
  }

  get deleteConfirmationMessage(): string {
    return this.getString(this.localizationService, "variant.delete.confirmation");
  }

  get editActionIconTitle(): string {
    return this.getString(this.localizationService, "variant.edit");
  }

  get dragTooltip(): string {
    return this.getString(this.localizationService, "variant.drag.tooltip");
  }

  componentWillLoad() {
    // Since the html5sortable modifies the DOM directly we need to render the component through a non-reactive property
    this.items = this.variants;
  }

  componentDidLoad() {
    this.variantListing = this.hostElement.shadowRoot.querySelector(`.${this.containerClass}`);

    const sortableInstance = sortable(this.variantListing, {
      items: ".ktc-variant-item--editable",
      placeholder: `<li data-${this.hostElement.nodeName}></li>`,
    })[0];
    sortableInstance.addEventListener("sortupdate", ({ detail }: CustomEvent<{ origin, destination, item: HTMLElement }>) => {
      const { items } = detail.destination;
      const variants = items.map((item) => item.dataset.itemKey).reverse();
      this.changeVariantsPriority && this.changeVariantsPriority(variants);

      // IE bug: Hover class is applied to item's original position also therefore we need to remove the class on both elements
      this.removeHoveredClassesAfterDrag();
    });

    sortableInstance.addEventListener("sortstart", () => this.variantListing.classList.add(this.containerSortingClass));
    sortableInstance.addEventListener("sortstop", () => this.variantListing.classList.remove(this.containerSortingClass));
  }

  componentDidUnload() {
    sortable(this.variantListing, "destroy");
  }

  onDeleteVariant = (event: Event, variantIdentifier: string): void => {
    event.stopPropagation();
    if (confirm(this.deleteConfirmationMessage)) {
      this.deleteVariant && this.deleteVariant(variantIdentifier);
    }
  }

  onEditButtonClick = (event: Event, item: VariantListingElement): void => {
    event.stopPropagation();
    this.editVariant && this.editVariant(item);
  }

  onItemClick = (item: VariantListingElement): void => {
    if (item.key === this.activeItemIdentifier) {
      return;
    }

    this.selectVariant && this.selectVariant(item);
  }

  removeHoveredClassesAfterDrag = () => {
    const elementsWithHoveredClass = this.hostElement.shadowRoot.querySelectorAll(`.${this.itemHoverClass}`);

    // tslint:disable-next-line:prefer-for-of
    for (let i = 0; i < elementsWithHoveredClass.length; i++) {
      elementsWithHoveredClass[i].classList.remove(this.itemHoverClass);
    }
  }

  addItemHoverClass = (item: VariantListingElement) => {
    this.hostElement.shadowRoot.querySelector(`[data-item-key="${item.key}"]`).classList.add(this.itemHoverClass);
  }

  removeItemHoverClass = (item: VariantListingElement) => {
    this.hostElement.shadowRoot.querySelector(`[data-item-key="${item.key}"]`).classList.remove(this.itemHoverClass);
  }

  render() {
    const listItem = (item: VariantListingElement) =>
      <li onClick={() => this.onItemClick(item)}
        class={{
          "ktc-variant-item": true,
          "ktc-variant-item--active": item.key === this.activeItemIdentifier,
          "ktc-variant-item--editable": item.renderActionButtons,
        }}
        data-item-key={item.key}
        onMouseEnter={() => this.addItemHoverClass(item)}
        onMouseLeave={() => this.removeItemHoverClass(item)}
      >
        {
          item.renderActionButtons &&
          <div class="ktc-drag-icon">
            <i aria-hidden="true" title={this.dragTooltip} class="icon-dots-vertical"></i>
          </div>
        }
        <div class="ktc-variant-item-label" title={item.name}>{item.name}</div>
        {
          item.renderActionButtons &&
          <div class="ktc-variant-action-icons">
            <a class="ktc-variant-action-icon">
              <i aria-hidden="true" class="icon-edit" title={this.editActionIconTitle} onClick={(e) => this.onEditButtonClick(e, item)} />
            </a>
            <a class="ktc-variant-action-icon">
              <i aria-hidden="true" class="icon-bin" title={this.deleteActionIconTitle} onClick={(e) => this.onDeleteVariant(e, item.key)} />
            </a>
          </div>
        }
      </li>;

    return (
      <ul class={this.containerClass}>
        {
          /* Render the collection through a non-reactive property in order to prevent re-renders when the variants collection gets updated,
            since the html5sortable library modifies the DOM directly */
        }
        {this.items.map(listItem)}
      </ul>
    );
  }
}
