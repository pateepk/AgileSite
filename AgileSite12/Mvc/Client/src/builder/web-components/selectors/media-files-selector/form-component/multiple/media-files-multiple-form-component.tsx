import { Component, Prop, State } from "@stencil/core";

import { ModalDialogApplyCallbackResult } from "@/builder/declarations";

import { GetString } from "../../../selector-types";
import { MediaFile, MediaFilesDialogOptions, MediaFilesSelectorItem } from "../../types";
import { getErrorText, getFileName, getTitle} from "../media-files-form-component-helper";

@Component({
  tag: "kentico-media-files-multiple-form-component",
  styleUrl: "media-files-multiple-form-component.less",
  shadow: false,
})
export class MediaFilesMultipleFormComponent {

  MAX_THUMBNAIL_COUNT = 12;
  MAX_LIST_COUNT = 9;

  @State() allFilesShowed: boolean;
  @State() isListMode: boolean;

  @Prop() getString: GetString;
  @Prop() libraryName: string;
  @Prop() maxFilesLimit: number;
  @Prop() allowedExtensions: string;
  @Prop() selectedFiles: MediaFile[] = [];
  @Prop() selectValues: (files: MediaFile[]) => void;

  componentWillLoad() {
    this.setMultiFileViewMode(this.selectedFiles);
  }

  getSelectedValues = (): MediaFilesSelectorItem[] => {
    if (this.selectedFiles && this.selectedFiles.length > 0) {
      return this.selectedFiles.map<MediaFilesSelectorItem>((file) => ({
        fileGuid: file.fileGuid,
      }));
    }
    return [];
  }

  setMultiFileViewMode = (files: MediaFile[]) => {
    this.isListMode = this.containsNonImageFile(files);
  }

  containsNonImageFile = (files: MediaFile[]): boolean => files.filter((file) => file.thumbnailUrls === null && file.isValid).length > 0;

  transferInvalidFlag = (files: MediaFile[], invalidFiles: MediaFile[]) => {
    const result: MediaFile[] = [];
    files.forEach((file) => {
      const i = invalidFiles.filter((x) => x.fileGuid === file.fileGuid)[0];
      if (i) {
        result.push(i);
      } else {
        result.push(file);
      }
    });

    return result;
  }

  openDialog = (): void => {
    const options: MediaFilesDialogOptions = {
      libraryName: this.libraryName,
      maxFilesLimit: this.maxFilesLimit,
      allowedExtensions: this.allowedExtensions,
      selectedValues: this.getSelectedValues(),
      applyCallback: (files: MediaFile[]): ModalDialogApplyCallbackResult => {

        const invalidFiles = this.selectedFiles.filter((file) => !file.isValid);
        files = this.transferInvalidFlag(files, invalidFiles);

        if (files !== null && files.length > 0) {
          this.setMultiFileViewMode(files);
          this.selectValues(files);
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

  changeSingleFileDialog = (file: MediaFile): void => {
    const index = this.selectedFiles.indexOf(file);
    const options: MediaFilesDialogOptions = {
      libraryName: this.libraryName,
      maxFilesLimit: 1,
      allowedExtensions: this.allowedExtensions,
      selectedValues: [{ fileGuid: file.fileGuid}],
      applyCallback: (files: MediaFile[]): ModalDialogApplyCallbackResult => {
        if (files !== null && files.length === 1) {
          const newFile = files[0];
          if (newFile.fileGuid === file.fileGuid) {
            return {
              closeDialog: true,
            };
          }

          const newSelectedMediaFiles = [...this.selectedFiles];
          if (this.selectedFiles.filter((f) => f.fileGuid === newFile.fileGuid).length > 0) {
            newSelectedMediaFiles.splice(index, 1);
          } else {
            newSelectedMediaFiles[index] = newFile;
          }

          this.setMultiFileViewMode(newSelectedMediaFiles);
          this.selectValues(newSelectedMediaFiles);
        }

        return {
          closeDialog: true,
        };
      },
    };
    window.kentico.modalDialog.mediaFilesSelector.open(options);
  }

  clear = () => this.selectValues([]);

  remove = (file: MediaFile) => this.selectValues(this.selectedFiles.filter((f) => f !== file));

  showAllFiles = () => this.allFilesShowed = true;

  renderShowAllLink = () =>
    <div class="ktc-show-all-items-container">
      <div>
        <a onClick={this.showAllFiles}>{this.getString("kentico.components.mediafileselector.showAll")}</a>
      </div>
    </div>

  renderThumbnail = (file: MediaFile) =>
      <div class="ktc-selector-thumbnail" title={getTitle(file, this.getString)}>
      <div class="ktc-overlay"></div>
      <i aria-hidden="true"
        title={this.getString("kentico.components.mediafileselector.changeFile")}
        class="icon-rotate-double-right ktc-cms-icon-80 ktc-action-icon"
        onClick={() => this.changeSingleFileDialog(file)}></i>
      <i aria-hidden="true"
        title={this.getString("kentico.components.mediafileselector.removeFile")}
        class="icon-bin ktc-cms-icon-80 ktc-action-icon"
        onClick={() => this.remove(file)}></i>
      {file.isValid ?
        <img class="ktc-thumbnail-image" src={file.thumbnailUrls.small} /> :
        <span class="ktc-invalid-file">
          <i aria-hidden="true" class="icon-exclamation-triangle ktc-cms-icon-130 ktc-type-icon"></i>
          {getErrorText(file, this.getString)}
          </span>}
    </div>

  renderList = (file: MediaFile) =>
    <li class={{"ktc-file-item": true, "ktc-invalid-file": !file.isValid}} title={getTitle(file, this.getString)} >
      <i aria-hidden="true" class={`${this.getIconClassName(file)} ktc-cms-icon-80 ktc-type-icon`}></i>
      <span>{getFileName(file, this.getString)}</span>
      <i aria-hidden="true"
        class="icon-rotate-double-right ktc-cms-icon-80 ktc-action-icon"
        title={this.getString("kentico.components.mediafileselector.changeFile")}
        onClick={() => this.changeSingleFileDialog(file)}></i>
      <i
        aria-hidden="true"
        class="icon-bin ktc-cms-icon-80 ktc-action-icon"
        title={this.getString("kentico.components.mediafileselector.removeFile")}
        onClick={() => this.remove(file)}></i>
    </li>

  getIconClassName = (file: MediaFile) => {
    if (!file.isValid) {
      return "icon-exclamation-triangle";
    }
    if (!file.thumbnailUrls) {
      return "icon-doc";
    }

    return "icon-picture";
  }

  renderEmptySelector = () =>
    <div class="ktc-selector-empty">
      {this.getString("kentico.components.mediafileselector.empty")}
    </div>

  renderAllSelectedFiles = () => {
    if (!this.selectedFiles || this.selectedFiles.length === 0) {
      return this.renderEmptySelector();
    } else {
      if (this.isListMode) {
        return this.renderNonImageFiles();
      } else {
        return this.renderThumbnails();
      }
    }
  }

  renderNonImageFiles = () => {
    if (this.selectedFiles.length > this.MAX_LIST_COUNT && !this.allFilesShowed) {
      return [
        <ul>{[...this.selectedFiles].slice(0, this.MAX_LIST_COUNT).map(this.renderList)}</ul>,
        this.renderShowAllLink(),
      ];
    }
    return <ul>{this.selectedFiles.map(this.renderList)}</ul>;
  }

  renderThumbnails = () => {
    if (this.selectedFiles.length > this.MAX_THUMBNAIL_COUNT && !this.allFilesShowed) {
      return [
        [...this.selectedFiles].slice(0, this.MAX_THUMBNAIL_COUNT).map(this.renderThumbnail),
        this.renderShowAllLink(),
      ];
    }

    return this.selectedFiles.map(this.renderThumbnail);
  }

  render() {
    return (
      <div class="ktc-mediafile-selector-component">
        <div class="ktc-form-control ktc-multi-file-box">
          {this.renderAllSelectedFiles()}
        </div>
        <div class="ktc-multi-file-buttons">
          <button type="button" class="ktc-btn ktc-btn-default" onClick={this.openDialog}>
            {this.getString("kentico.components.mediafileselector.select")}
          </button>
          <button type="button" class="ktc-btn ktc-btn-default" onClick={this.clear}>
            {this.getString("kentico.components.mediafileselector.clearall")}
          </button>
        </div>
      </div>
    );
  }
}
