import { Component, Event, EventEmitter, Prop } from "@stencil/core";
import Dropzone from "dropzone";

import { GetString } from "../../selector-types";
import { UploaderOptions } from "../types";

@Component({
  tag: "kentico-media-files-uploader",
  styleUrl: "media-files-uploader.less",
  shadow: true,
})
export class MediaFilesUploader {
  previewTemplate: HTMLDivElement;
  uploader: any;

  @Prop() getString: GetString;
  @Prop() uploaderOptions: UploaderOptions;

  @Event() uploadError: EventEmitter<string>;
  @Event() uploadSuccessful: EventEmitter<string>;

  componentWillLoad() {
    this.previewTemplate = document.createElement("div");
    this.previewTemplate.innerHTML = `
    <div class="ktc-uploader-preview ktc-uploader-file-preview" data-kentico-media-files-uploader>
      <div class="ktc-uploader-preview-container" data-kentico-media-files-uploader>
        <div class="ktc-uploader-details" data-kentico-media-files-uploader>
          <div class="ktc-uploader-filename" data-kentico-media-files-uploader><span data-ktc-uploader-name></span></div>
        </div>
        <div class="ktc-uploader-progress" data-kentico-media-files-uploader>
          <span class="ktc-uploader-upload" data-ktc-uploader-uploadprogress data-kentico-media-files-uploader></span>
        </div>
        <div id="ktc-uploader-error" data-kentico-media-files-uploader>
          <div class="ktc-uploader-error-message" data-kentico-media-files-uploader></div>
          <div class="ktc-uploader-error-button" data-kentico-media-files-uploader>
            <button type="button" class="ktc-btn ktc-btn-default" data-ktc-uploader-reset data-kentico-media-files-uploader>Try again</button>
          </div>
        </div>
      </div>
    </div>`;
  }

  componentDidLoad() {
    this.setupDropzone();
  }

  componentDidUpdate() {
    this.setupDropzone();
  }

  setupDropzone() {
    if (!this.uploaderOptions.enabled) {
      return;
    }

    if (this.uploader.dropzone) {
      this.uploader.dropzone.options.url = this.uploaderOptions.url;
    } else {
      this.createNewDropzone(this.uploader);
    }
  }

  createNewDropzone(uploader: any) {
    const dropzone = new Dropzone(uploader,
      {
        acceptedFiles:  this.uploaderOptions.allowedExtensions.replace(/;/g, ","),
        url: this.uploaderOptions.url,
        createImageThumbnails: false,
        clickable: uploader,
        previewTemplate: this.previewTemplate.innerHTML,
        addedfile: this.addedFileHandler,
        maxFilesize: null,
        dictInvalidFileType: "kentico.components.mediafileuploader.forbiddenextension",
        dictResponseError: "kentico.components.mediafileuploader.uploaderror",
        timeout: 0,
    });

    let timeouted: boolean = false;

    dropzone.on("sending", (file, xhr: XMLHttpRequest) => {
      dropzone.element.querySelector("[data-ktc-uploader-name]").innerText = file.name;
      xhr.ontimeout = () => {
        this.uploadError.emit(this.getString("kentico.components.mediafileuploader.timeout", file.name));
        this.clearUploader(dropzone, true);
        timeouted = true;
      };
    });

    dropzone.on("totaluploadprogress", (totalUploadProgress) => {
      const progressElement = dropzone.element.querySelector("[data-ktc-uploader-uploadprogress]");
      if (progressElement) {
        progressElement.style.width = totalUploadProgress + "%";
      }
    });

    dropzone.on("queuecomplete", () => {
      if (timeouted) {
        timeouted = false;
        return;
      }

      if (dropzone.getUploadingFiles().length !== 0 || dropzone.getQueuedFiles().length !== 0) {
        // There are files being processed. (This event is fired multiple times and not once as expeced.)
        return;
      }

      const successFiles = dropzone.getFilesWithStatus(Dropzone.SUCCESS);
      const errorFiles = dropzone.getFilesWithStatus(Dropzone.ERROR);

      if (successFiles.length === 0 && errorFiles.length === 0) {
        // There are no processed files. (This event is fired multiple times and not once as expeced.)
        return;
      }

      if (errorFiles.length === 0) {
        this.uploadSuccessful.emit(this.getString("kentico.components.mediafileuploader.uploadsuccess"));
      }

      if (successFiles.length > 0) {
        setTimeout(() => this.uploaderOptions.uploadHandler(), 50);
      }

      this.clearUploader(dropzone);
    });

    dropzone.on("error", (file, ...parameters: any[]) => {
      this.uploadError.emit(this.getString(parameters[0], file.name, file.type));
    });
  }

  clearUploader(dropzone, allFiles: boolean = false) {
    const previewElement = dropzone.element.querySelector(".ktc-uploader-preview");
    previewElement.remove();
    dropzone.removeAllFiles(allFiles);
  }

  addedFileHandler() {
    const dropzone: any = this;
    if (dropzone.previewsContainer) {
      let previewElement = dropzone.element.querySelector(".ktc-uploader-preview");

      if (!previewElement) {
        previewElement = Dropzone.createElement(dropzone.options.previewTemplate.trim());
      }

      dropzone.previewsContainer.appendChild(previewElement);
    }
  }

  render() {
    const renderUploader = () => (
      this.uploaderOptions.enabled ?
      <div class="ktc-uploader-container">
        <div class="dz-clickable ktc-uploader" ref={(el) => this.uploader = el}>
          <div class="dz-message ktc-uploader-message">
            <i class="icon-arrow-up-line"></i>
            <div>
              {this.getString("kentico.components.mediafileuploader.droptext")}
            </div>
          </div>
        </div>
      </div> : ""
    );

    const renderEmptyUploader = () => (
      <div class="ktc-uploader-empty-container">
        <div class={{
          "dz-clickable":   true,
          "ktc-uploader": true,
          "ktc-uploader-disabled": !this.uploaderOptions.enabled,
          }} ref={(el) => this.uploader = el}>
          <div class="dz-message ktc-uploader-message">
            <span class="ktc-uploader-message-header">{this.uploaderOptions.title}</span>
            {this.uploaderOptions.enabled &&
              <div>
                <i class="icon-arrow-up-line"></i>
                <div>
                  {this.getString("kentico.components.mediafileuploader.droptext")}
                </div>
              </div>
            }
          </div>
        </div>
      </div>
    );

    return this.uploaderOptions.showEmptyContainer ? renderEmptyUploader() : renderUploader();
  }
}
