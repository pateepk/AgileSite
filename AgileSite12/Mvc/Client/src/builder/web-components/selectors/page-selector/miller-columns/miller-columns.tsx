import { Component, Listen, Prop, State, Watch } from "@stencil/core";
import SimpleBar from "simplebar";

import { GetString } from "../../selector-types";
import { ListingItem, MillerColumn, SelectItemEventDetail } from "../miller-columns-types";

const SCROLL_RIGHT_POSITION = 10000;

@Component({
  tag: "kentico-miller-columns",
  styleUrl: "miller-columns.less",
  shadow: false,
})
export class MillerColumnsContainer {
  listingContainerWrapper: HTMLElement;
  animationTime = 300;
  animateNewColumn: boolean = false;
  scrollElement: HTMLElement;

  @State() animateListing = false;

  @Prop() columns: Array<MillerColumn<ListingItem>>;
  @Prop() getString: GetString;

  @Watch("columns")
  onColumnsChange() {
    if (this.animateNewColumn) {
      this.animateListing = true;
      setTimeout(() => this.animateListing = this.animateNewColumn = false, this.animationTime);
    }
  }

  @Listen("select")
  onItemSelect({ detail: { item }}: CustomEvent<SelectItemEventDetail>) {
    if (item.showArrow) {
      this.animateNewColumn = true;
    }
  }

  adjustScrollPosition = () => {
    if (this.scrollElement) {
      // move horizontal scroll position to the most right, since a new column opens
      this.scrollElement.scrollLeft = SCROLL_RIGHT_POSITION;
    }
  }

  componentDidLoad() {
    const simpleBar = new SimpleBar(this.listingContainerWrapper);
    this.scrollElement = simpleBar.getScrollElement() as HTMLElement;

    // move horizontal scroll position to the most right
    this.scrollElement.scrollLeft = SCROLL_RIGHT_POSITION;
  }

  renderListings() {
    return this.columns
      .map((column, index) => {
        const shouldSlide = this.animateListing && index === this.columns.length - 1;

        return (
          <kentico-miller-column
            slide={shouldSlide}
            style={{ "z-index": (shouldSlide) ? "1" : "2" }}
            column={column}
            treeLevel={index}
            getString={this.getString}
            onLoad={this.adjustScrollPosition}
          />
        );
      });
  }

  render() {
    return (
      <div ref={(el) => this.listingContainerWrapper = el}>
        <div class="ktc-miller-columns">
          {this.renderListings()}
        </div>
      </div>
    );
  }
}
