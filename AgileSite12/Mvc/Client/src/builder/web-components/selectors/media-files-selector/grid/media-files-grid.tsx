import { Component, Element, Event, EventEmitter, Prop } from "@stencil/core";

import { lineClamp } from "@/builder/helpers/markup-helper";

import { GetString } from "../../selector-types";
import { MediaFilesSelectorItem, UploaderOptions } from "../types";
import { GridItem } from "./grid-item";

@Component({
  tag: "kentico-media-files-grid",
  styleUrl: "media-files-grid.less",
  shadow: true,
})
export class MediaFilesGrid {

  @Element() mediaFilesGrid: HTMLElement;

  @Prop() getString: GetString;
  @Prop() items: GridItem[] = [];
  @Prop() selectedValues: MediaFilesSelectorItem[];
  @Prop() uploaderOptions: UploaderOptions;
  @Prop() filterQuery: string;

  @Event() changeItem: EventEmitter<{ value: string, remove: boolean }>;

  componentDidLoad() {
    this.shortenFileNames();
  }

  componentDidUpdate() {
    this.shortenFileNames();
  }

  get isFiltered() {
    return !!this.filterQuery;
  }

  onItemClick = (item: GridItem): void => {
    this.changeItem.emit(
      {
        value: item.value,
        remove: this.valueIsSelected(item.value),
      });
  }

  filterItems = (): GridItem[] => {
    if (!this.isFiltered) {
      return this.items;
    }
    const lowerCaseFilterQuery = this.filterQuery.toLowerCase();
    return this.items.filter((item) => item.name.toLowerCase().indexOf(lowerCaseFilterQuery) > -1);
  }

  valueIsSelected = (value: string): boolean => {
    return this.selectedValues.filter((v) => v.fileGuid === value).length > 0;
  }

  shortenFileNames = () => {
    const itemElements: NodeListOf<HTMLElement> = this.mediaFilesGrid.shadowRoot.querySelectorAll(".ktc-grid-item-label-box");
    Array.prototype.map.call(itemElements, ((element: HTMLElement) => {
      this.handleLabel(element);
    }));
  }

  handleLabel = (element: HTMLElement): void => {
    lineClamp(element.childNodes[0] as HTMLElement, 3);
  }

  renderItems = (items: GridItem[]) => (
    <div>
      {
        !this.isFiltered &&
          <kentico-media-files-uploader
            getString={this.getString}
            uploaderOptions={this.uploaderOptions}
          />
      }
      {items.map(this.renderGridItem)}
    </div>
  )

  renderGridItem = (item: GridItem) => (
    <div class={{
      "ktc-grid-item": true,
      "ktc-grid-item--active": this.valueIsSelected(item.value),
      "ktc-grid-item--no-thumbnail": item.thumbnailUrl === null,
    }}
      onClick={() => this.onItemClick(item)}
      data-value={item.value}
    >
      <div class="ktc-grid-item-thumbnail-container">
        <div class="ktc-grid-item-thumbnail">
          {item.thumbnailUrl === null ?
            <i aria-hidden="true" class="icon-doc ktc-cms-icon-150"></i> :
            <img class="ktc-grid-item-thumbnail-image" src={item.thumbnailUrl} />}
        </div>
        <div class="ktc-grid-item-label-box">
          <span class="ktc-grid-item-label" title={item.name}>{item.name}</span>
        </div>
      </div>
    </div>
  )

  renderEmptyFolder = () => (
    this.isFiltered ?
      <div class="ktc-grid-no-filtered-items-container">
        <div class="ktc-grid-no-filtered-items-label" innerHTML={this.getString("kentico.components.mediafileselector.filter.noFiles")} />
      </div>
      :
      <kentico-media-files-uploader
        getString={this.getString}
        uploaderOptions={this.uploaderOptions}
      />
  )

  render() {
    const filteredItems = this.filterItems();
    return (
      <div class="ktc-grid">
        {filteredItems.length > 0 ? this.renderItems(filteredItems) : this.renderEmptyFolder()}
        <div class="ktc-grid-clear"></div>
      </div>
    );
  }
}
