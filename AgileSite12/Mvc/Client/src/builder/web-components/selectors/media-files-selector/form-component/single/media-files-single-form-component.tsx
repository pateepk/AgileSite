import { Component, Element, Prop } from "@stencil/core";

import { ModalDialogApplyCallbackResult } from "@/builder/declarations";
import { lineClamp } from "@/builder/helpers/markup-helper";

import { GetString } from "../../../selector-types";
import { MediaFile, MediaFilesDialogOptions } from "../../types";

import {getFileName, getTitle} from "../media-files-form-component-helper";

@Component({
  tag: "kentico-media-files-single-form-component",
  styleUrl: "media-files-single-form-component.less",
  shadow: false,
})
export class MediaFilesSingleFormComponent {

  MAX_FILES_LIMIT = 1;

  @Element() element: HTMLElement;

  @Prop() getString: GetString;
  @Prop() libraryName: string;
  @Prop() allowedExtensions: string;
  @Prop() selectedFile: MediaFile;
  @Prop() selectValues: (files: MediaFile[]) => void;

  componentDidLoad() {
    this.shortenFileName();
  }

  componentDidUpdate() {
    this.shortenFileName();
  }

  shortenFileName = () => {
    const label: HTMLElement = this.element.querySelector(".ktc-label-filename");
    if (label !== null) {
      lineClamp(label, 3);
    }
  }

  openDialog = () => {
    const options: MediaFilesDialogOptions = {
      libraryName: this.libraryName,
      maxFilesLimit: this.MAX_FILES_LIMIT,
      allowedExtensions: this.allowedExtensions,
      selectedValues: (this.selectedFile) ? [{ fileGuid: this.selectedFile.fileGuid }] : [],
      applyCallback: (files: MediaFile[]): ModalDialogApplyCallbackResult => {
        if (files !== null && files.length === 1) {
          const newFile = files[0];
          if (this.selectedFile && newFile.fileGuid === this.selectedFile.fileGuid) {
            return {
              closeDialog: true,
            };
          }

          this.selectValues([files[0]]);
        } else {
          this.clear();
        }

        return {
          closeDialog: true,
        };
      },
    };
    window.kentico.modalDialog.mediaFilesSelector.open(options);
  }

  clear = () => this.selectValues([]);

  renderImageFile = () =>
    <div class="ktc-selector-thumbnail">
      <img class="ktc-thumbnail-image" src={this.selectedFile.thumbnailUrls.large} />
    </div>

  renderNoImageFile = () =>
    <div class="ktc-selector-file-icon">
      <i aria-hidden="true" class="icon-doc ktc-cms-icon-150"></i>
      <div class="ktc-label-filename">
        {getFileName(this.selectedFile, this.getString)}
      </div>
    </div>

  renderInvalidFile = () =>
    <div class="ktc-selector-file-icon ktc-invalid-file">
      <i aria-hidden="true" class="icon-exclamation-triangle ktc-cms-icon-150"></i>
      <div class="ktc-label-filename">
        {getFileName(this.selectedFile, this.getString)}
      </div>
    </div>

  renderBigThumbnail = () => {
    if (!this.selectedFile) {
      return (
        <div class="ktc-selector-empty">
          <i aria-hidden="true" class="icon-ban-sign ktc-cms-icon-200"></i>
        </div>
        );
    }

    if (!this.selectedFile.isValid) {
      return this.renderInvalidFile();
    }

    return this.selectedFile.thumbnailUrls ? this.renderImageFile() : this.renderNoImageFile();
  }

  render() {
    return (
      <div class="ktc-mediafile-selector-component">
        <div class="ktc-form-control ktc-file-box"  title={getTitle(this.selectedFile, this.getString)}>
          {this.renderBigThumbnail()}
          {
            this.selectedFile ?
              <div class="ktc-overlay">
                <div>
                  <button type="button" class="ktc-btn ktc-btn-default" onClick={this.openDialog} title="">
                    {this.getString("kentico.components.mediafileselector.selectdifferent")}
                  </button>
                  <span>{this.getString("kentico.components.mediafileselector.or")}</span>&nbsp;
              <a onClick={this.clear} title="">{this.getString("kentico.components.mediafileselector.clear")}</a>
                </div>
              </div>
              :
              <button type="button" class="ktc-btn ktc-btn-default ktc-select-button" onClick={this.openDialog}>
                {this.getString("kentico.components.mediafileselector.select")}
              </button>
          }
        </div>
      </div>
    );
  }
}
