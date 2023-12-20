import { AbTestVariantListingElement, ABVariantListItemActionState } from "@/page-builder/declarations/components/abtest-variant-listing";
import { Component, Element, Method, Prop, State, Watch } from "@stencil/core";

@Component({
  tag: "kentico-abtest-variant-listing",
  styleUrl: "abtest-variant-listing.less",
  shadow: true,
})
export class AbVariantListing {
  containerClass = "ktc-variant-item-list";
  newVariantName: string;
  confirmButton: HTMLElement;

  @Element() hostElement: HTMLElement;

  @State() editedItemIdentifier: string = null;
  @State() hoveredItemKey: string = null;
  @State() renameInputElement: HTMLInputElement;

  @Prop() editActionIconTitle: string;
  @Prop() removeActionIconTitle: string;
  @Prop() actionDisabledTitle: string;
  @Prop() removeConfirmationMessage: string;
  @Prop({ mutable: true }) variants: AbTestVariantListingElement[] = [];
  @Prop({ mutable: true }) activeItemIdentifier: string;
  @Prop() maximumNameLength: number;

  // functions
  @Prop() selectVariant: (variant: AbTestVariantListingElement) => void;
  @Prop() renameVariant: (variant: AbTestVariantListingElement) => void;
  @Prop() removeVariant: (variantIdentifier: string) => void;

  componentDidLoad() {
    setTimeout(() => {
      this.hostElement.style.width = this.hostElement.getBoundingClientRect().width + "px";
    }, 0);
  }

  onRemoveVariant = (event: Event, item: AbTestVariantListingElement): void => {
    event.stopPropagation();

    if (item.removeActionState === ABVariantListItemActionState.Disabled) {
      return;
    }

    if (confirm(this.removeConfirmationMessage)) {
      this.variants = this.variants.filter((v) => v.key !== item.key);
      this.removeVariant && this.removeVariant(item.key);
    }
  }

  onEditButtonClick = (event: Event, item: AbTestVariantListingElement): void => {
    event.stopPropagation();

    if (item.editActionState === ABVariantListItemActionState.Disabled) {
      return;
    }

    this.newVariantName = item.name;
    this.editedItemIdentifier = item.key;
  }

  onItemClick = (item: AbTestVariantListingElement): void => {
    if (item.key === this.activeItemIdentifier || item.key === this.editedItemIdentifier) {
      return;
    }

    this.selectVariant && this.selectVariant(item);
  }

  confirmRenameKeypress = (event: KeyboardEvent, item: AbTestVariantListingElement) => {
    if (event.key === "Enter") {
      // Prevent the pop up container from closing in Edge and IE.
      event.preventDefault();

      if (this.newVariantName !== "") {
        this.confirmRename(event, item);
      }
    }
  }

  confirmRename = (event: Event, item: AbTestVariantListingElement) => {
    event.stopPropagation();
    if (item.name === this.newVariantName) {
      this.closeEditor();
      return;
    }

    const renamedVariant = { ...item, name: this.newVariantName };
    this.variants = this.variants.map((v) => v.key === item.key ? renamedVariant : v);

    this.renameVariant(renamedVariant);
    this.closeEditor();
  }

  cancelRename = (event: Event) => {
    event.stopPropagation();
    this.closeEditor();
  }

  @Method()
  closeEditor() {
    this.editedItemIdentifier = null;
  }

  @Watch("renameInputElement")
  focusEditor(element: HTMLInputElement) {
    element.select();
  }

  inputChange(event) {
    this.newVariantName = event.target.value;
    if (this.newVariantName === "") {
      this.confirmButton.classList.add("disabled");
    } else {
      this.confirmButton.classList.remove("disabled");
    }
  }

  editNameMarkup(item) {
    return (
      this.editedItemIdentifier && this.editedItemIdentifier === item.key &&
      <div class="ktc-variant-edit-wrapper">
        <input type="text" maxLength={this.maximumNameLength} value={this.newVariantName} ref={(el) => this.renameInputElement = el as HTMLInputElement}
          onInput={(event) => this.inputChange(event)} onKeyPress={(event) => this.confirmRenameKeypress(event, item)} />
        <div class="ktc-variant-edit-icons">
          <i class="icon-check-circle" ref={(el) => this.confirmButton = el} onClick={(event) => this.confirmRename(event, item)}></i>
          <i class="icon-times-circle" onClick={(event) => this.cancelRename(event)}></i>
        </div>
      </div>
    );
  }

  editIconsMarkup(item) {
    return (
      <div class="ktc-variant-action-icons">
        {
          item.editActionState !== ABVariantListItemActionState.Hidden &&
          <a class="ktc-variant-action-icon">
            <i aria-hidden="true"
              class={{
                [item.editActionState.toString()]: true,
                "icon-edit": true,
              }}
              title={item.editActionState === ABVariantListItemActionState.Disabled ? this.actionDisabledTitle : this.editActionIconTitle}
              onClick={(e) => this.onEditButtonClick(e, item)} />
          </a>
        }
        {
          item.removeActionState !== ABVariantListItemActionState.Hidden &&
          <a class="ktc-variant-action-icon">
            <i aria-hidden="true"
              class={{
                [item.editActionState.toString()]: true,
                "icon-bin": true,
              }}
              title={item.removeActionState === ABVariantListItemActionState.Disabled ? this.actionDisabledTitle : this.removeActionIconTitle}
              onClick={(e) => this.onRemoveVariant(e, item)} />
          </a>
        }
      </div>
    );
  }

  render() {
    const listItem = (item: AbTestVariantListingElement) => {
      const isEditable = item.editActionState !== ABVariantListItemActionState.Hidden || item.removeActionState !== ABVariantListItemActionState.Hidden;

      return (
        <li onClick={() => this.onItemClick(item)}
          class={{
            "ktc-variant-item": true,
            "ktc-variant-item--active": item.key === this.activeItemIdentifier,
            "ktc-variant-item--editable": isEditable,
            "ktc-is-hovered": item.key === this.hoveredItemKey,
          }}
          onMouseEnter={() => this.hoveredItemKey = item.key}
          onMouseLeave={() => this.hoveredItemKey = null}
        >
          {
            (!this.editedItemIdentifier || this.editedItemIdentifier !== item.key) &&
            <div class="ktc-variant-item-label" title={item.name}>{item.name}</div>
          }
          {
            isEditable && (!this.editedItemIdentifier || this.editedItemIdentifier !== item.key) &&
            this.editIconsMarkup(item)
          }
          {
            this.editNameMarkup(item)
          }
        </li >
      );
    };

    return (
      <ul class={this.containerClass}>
        {this.variants.map(listItem)}
      </ul>
    );
  }
}
