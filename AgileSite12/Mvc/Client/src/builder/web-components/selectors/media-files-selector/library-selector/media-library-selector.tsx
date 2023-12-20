import { Component, Event, EventEmitter, Prop } from "@stencil/core";

import { GetString } from "../../selector-types";
import { SelectorItem } from "./selector-item";

@Component({
  tag: "kentico-media-library-selector",
  styleUrl: "media-library-selector.less",
  shadow: true,
})
export class MediaLibrarySelector {

  @Prop() getString: GetString;
  @Prop() selectedValue: string;
  @Prop() items: SelectorItem[];

  @Event() selectItem: EventEmitter<string>;

  selectChanged = (event: Event) => {
    const selectElement = event.target as HTMLSelectElement;
    this.selectItem.emit(selectElement.value);
  }

  render() {
    const listItem = (item: SelectorItem) =>
      <option value={item.value} selected={this.selectedValue === item.value}>{item.name}</option>;

    return (
      <div>
        <div class="ktc-label-container">
          <label>
            {this.getString("kentico.components.mediafileselector.libraryname")}
          </label>
        </div>
        <div class="ktc-select-container">
          <select class="ktc-form-control" onChange={this.selectChanged}>
            {this.items.map(listItem)}
          </select>
        </div>
      </div>
    );
  }
}
