import { Component, Element, Event, EventEmitter, Prop, State, Watch } from "@stencil/core";
import SimpleBar from "simplebar";

import { GetString } from "../../selector-types";
import { ListingItem, MillerColumn, SelectItemEventDetail } from "../miller-columns-types";

@Component({
  tag: "kentico-miller-column",
  styleUrl: "miller-column.less",
  shadow: false,
})
export class MillerColumnItem {
  listingElement: HTMLElement;

  @Element() hostElement: HTMLElement;

  @State() selectedItemIndex: number = null;

  @Prop() column: MillerColumn<ListingItem>;
  @Prop() slide: boolean;
  @Prop() treeLevel: number;
  @Prop() getString: GetString;

  @Event() select: EventEmitter<SelectItemEventDetail>;
  @Event() load: EventEmitter;

  @Watch("column")
  itemsChanged() {
    this.selectedItemIndex = null;
  }

  componentDidLoad() {
    const simpleBar = new SimpleBar(this.listingElement);
    const selectedItem = this.hostElement.querySelector<HTMLElement>(".ktc-miller-column__item--selected");

    // if there is a pre-selected item in the column, scroll to its position
    if (selectedItem) {
      // had to set timeout, otherwise the scrollTop change wouldn't reflect
      setTimeout(() => {
        const scrollElement = simpleBar.getScrollElement();
        scrollElement.scrollTop = selectedItem.offsetTop;
      }, 10);
    }

    this.load.emit();
  }

  onItemClick(item: ListingItem, index: number) {
    if (this.isParentItem(index) || !item.showArrow && this.selectedItemIndex === index) {
      return;
    }

    this.selectedItemIndex = index;
    this.select.emit({
      item,
      treeLevel: this.treeLevel,
    });
  }

  renderListingItem = (item: ListingItem, index: number) => (
    <li key={index}
        class={{
          "ktc-miller-column__item": true,
          "ktc-miller-column__item--parent": this.isParentItem(index),
          "ktc-miller-column__item--root": this.isRootItem(index),
          "ktc-miller-column__item--selected": (!this.isParentItem(index) && item.selected && this.selectedItemIndex === null) || (this.selectedItemIndex === index),
    }}>
      <a onClick={() => this.onItemClick(item, index)}>
        <div class="ktc-listing-item">
          <div><i aria-hidden="true" class={item.icon}></i></div>
          <div class="ktc-listing-item-name" title={item.name}>
            { item.name }
          </div>
          { index !== -1 && item.showArrow && <div class="ktc-arrow"></div> }
        </div>
      </a>
    </li>
  )

  isRootItem = (itemIndex: number) => this.treeLevel === 0 && itemIndex === -1;

  isParentItem = (itemIndex: number) => this.treeLevel > 0 && itemIndex === -1;

  render() {
    return (
      <div class={{ "ktc-miller-column__wrapper": true, "ktc-slide": this.slide }}>
        <ul class="ktc-miller-column__parent">{ this.renderListingItem(this.column.parentItem, -1) }</ul>
        <div ref={(el) => this.listingElement = el} class="ktc-miller-column">
          <ul>
            { this.column.items.map(this.renderListingItem) }
          </ul>
        </div>
      </div>
    );
  }
}
