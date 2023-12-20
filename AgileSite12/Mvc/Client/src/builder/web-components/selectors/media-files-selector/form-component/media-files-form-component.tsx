import { Component, Element, Event, EventEmitter, Prop, State } from "@stencil/core";

import { GetString } from "../../selector-types";
import { MediaFile, MediaFilesSelectorItem } from "../types";

@Component({
  tag: "kentico-media-files-form-component",
  shadow: false,
})
export class MediaFilesFormComponent {
  hiddenInput: HTMLElement;

  @Element() el: HTMLElement;

  @State() selectedFiles: MediaFile[] = [];
  @State() allFilesShowed: boolean;
  @State() isListMode: boolean;

  @Prop() getString: GetString;
  @Prop() libraryName: string;
  @Prop() maxFilesLimit: number;
  @Prop() allowedExtensions: string;
  @Prop() inputName: string;
  @Prop() selectedData: string;

  @Event() kenticoPropertiesDialogInputInit: EventEmitter<HTMLElement>;

  componentWillLoad() {
    if (this.selectedData) {
      this.selectedFiles = JSON.parse(this.selectedData) as MediaFile[];
    }
  }

  componentDidLoad() {
    this.kenticoPropertiesDialogInputInit.emit(this.el);
  }

  getSelectedValues = (): string => {
    if (this.selectedFiles && this.selectedFiles.length > 0) {
      return JSON.stringify(this.selectedFiles.map<MediaFilesSelectorItem>((item) => ({
        fileGuid: item.fileGuid,
      })));
    }
    return "";
  }

  selectValues = (files: MediaFile[]): void => {
    this.selectedFiles = files;
    this.hiddenInput.dispatchEvent(new CustomEvent("change"));
  }

  render() {
    return (
      <div>
        {this.maxFilesLimit === 1 ?
          <kentico-media-files-single-form-component
            getString={this.getString}
            libraryName={this.libraryName}
            allowedExtensions={this.allowedExtensions}
            selectedFile={this.selectedFiles.length > 0 ? this.selectedFiles[0] : null}
            selectValues={this.selectValues}
          />
          :
          <kentico-media-files-multiple-form-component
            getString={this.getString}
            libraryName={this.libraryName}
            maxFilesLimit={this.maxFilesLimit}
            allowedExtensions={this.allowedExtensions}
            selectedFiles={this.selectedFiles}
            selectValues={this.selectValues}
          />
        }
        <input type="hidden" ref={(el) => this.hiddenInput = el} name={this.inputName} value={this.getSelectedValues()} />
      </div>
    );
  }
}
