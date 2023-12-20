import { Component, Event, EventEmitter, Prop } from "@stencil/core";

import { PopupListingElement } from "@/builder/declarations/index";

@Component({
  tag: "kentico-pop-up-listing",
  styleUrl: "pop-up-listing.less",
  shadow: true,
})
export class PopupListing {

  @Prop() items: PopupListingElement[] = [];
  @Prop() activeItemIdentifier: string;
  @Prop() noItemsAvailableMessage: string;
  @Prop() singleColumn: boolean;

  @Event() selectItem: EventEmitter<PopupListingElement>;

  getItemTitle = (item: PopupListingElement): string => {
    return item.description ? `${item.name}\n${item.description}` : item.name;
  }

  onItemClick = (item: PopupListingElement): string => {
    if (item.key === this.activeItemIdentifier) {
      return;
    }

    this.selectItem.emit(item);
  }

  render() {
    const listItem = (item: PopupListingElement) =>
      <li class={{
        "ktc-pop-up-item": true,
        "ktc-pop-up-item--active": item.key === this.activeItemIdentifier,
        "ktc-pop-up-item-double-column": !this.singleColumn,
        "ktc-pop-up-item-single-column": this.singleColumn,
      }}
        title={this.getItemTitle(item)}
        onClick={() => this.onItemClick(item)}
        data-component-identifier={item.key}
      >
        {!item.iconClass ? "" :
          <div class="ktc-pop-up-item-icon">
            <i aria-hidden="true" class={item.iconClass}></i>
          </div>
        }
        <div class={{ "ktc-pop-up-item-label": true, "ktc-pop-up-item-label--no-icon": !item.iconClass }
        }>{item.name}</div>
      </li>;

    return (
      <div class={{ "ktc-pop-up-item-listing": true, "ktc-pop-up-item-listing--empty": this.items.length === 0 }}>
        {this.items.length === 0 ? this.noItemsAvailableMessage :
          <ul>
            {this.items.map(listItem)}
          </ul>
        }
      </div>
    );
  }
}
