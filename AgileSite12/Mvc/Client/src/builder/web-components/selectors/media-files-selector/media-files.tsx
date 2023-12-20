import { Component, Listen, Method, Prop, State } from "@stencil/core";

import { MessagesPlaceholder } from "../messages-placeholder/messages-placeholder";
import { GetString } from "../selector-types";
import { GridItem } from "./grid/grid-item";
import { SelectorItem } from "./library-selector/selector-item";
import { TreeNode } from "./tree/tree-node";
import { MediaFile, MediaFilesSelectorItem, MediaLibrary, UploaderOptions } from "./types";

@Component({
  tag: "kentico-media-files",
  styleUrl: "media-files.less",
  shadow: true,
})
export class MediaFiles {
  messagesPlaceholder: MessagesPlaceholder;
  libraries: MediaLibrary[] = [];
  selectedLibrary: MediaLibrary;

  @State() selectedValues: MediaFilesSelectorItem[];
  @State() selectedFolderPath: string;
  @State() selectedLibraryName: string;
  @State() treeStructure: TreeNode;
  @State() gridItems: GridItem[];
  @State() librariesSelectorData: SelectorItem[];
  @State() loadingGridData: boolean;
  @State() errorMessage: string = null;
  @State() filterQuery: string;

  @Prop() getString: GetString;
  @Prop() values: MediaFilesSelectorItem[];
  @Prop() dataUrl: string;
  @Prop() uploaderUrl: string;
  @Prop() treeDataUrl: string;
  @Prop() selectorDataUrl: string;
  @Prop() modelDataUrl: string;
  @Prop() libraryName: string = "";
  @Prop() maxFilesLimit: number = 1;
  @Prop() allowedExtensions: string;

  componentWillLoad = async (): Promise<void> => {
    this.selectedValues = this.values || [];

    const selectedFiles: MediaFile[] = await this.getFiles(this.selectedValues.map((v) => v.fileGuid)) || [];
    const validFiles: MediaFile[] = selectedFiles.filter((file) => file.isValid);
    const selectedFile: MediaFile = validFiles.length > 0 ? validFiles[0] : null;
    const selectedLibraryName = selectedFile !== null ? selectedFile.libraryName : this.libraryName || "";
    const selectedFolderPath = selectedFile !== null ? selectedFile.folderPath : "";

    await this.loadLibrariesData();
    this.setSelectedLibrary(selectedLibraryName);
    this.setSelectedFolder(selectedFolderPath);

    await this.loadTreeData();
    await this.loadGridData();
  }

  @Method()
  async getSelectedFiles(): Promise<MediaFile[]> {
    return this.getFiles(this.selectedValues.map((v) => v.fileGuid));
  }

  getFiles = (identifiers: string[]): Promise<MediaFile[]> => {
    if (identifiers.length === 0) {
      return Promise.resolve(null);
    }

    return this.getData(this.modelDataUrl, "POST", identifiers);
  }

  setSelectedFolder = (folderPath: string): void => {
    this.selectedFolderPath = folderPath;
  }

  setSelectedLibrary = (name: string): void => {
    if (this.libraries === null || this.libraries.length <= 0) {
      return;
    }

    if (name !== "") {
      const selectedLibrary = this.libraries.filter((l) => l.identifier === name);
      if (selectedLibrary.length > 0) {
        this.selectedLibrary = selectedLibrary[0];
      }
    } else {
      this.selectedLibrary = this.libraries[0];
    }

    if (this.selectedLibrary) {
      this.selectedLibraryName = this.selectedLibrary.identifier;
    }
  }

  selectItemHandler = async (event: CustomEvent<{ value: string, remove: boolean }>): Promise<void> => {
    event.stopPropagation();
    const value = event.detail.value;
    const itemRemoving = event.detail.remove;

    if (itemRemoving) {
      if (this.maxFilesLimit === 1) {
        return;
      }
      this.selectedValues = this.selectedValues.filter((v) => v.fileGuid !== value);
    } else {
      if (this.maxFilesLimit === 1) {
        this.selectedValues = [{ fileGuid: value }];
      } else if (this.selectedValues.length < this.maxFilesLimit || this.maxFilesLimit === 0) {
        this.selectedValues = [
          ...this.selectedValues,
          { fileGuid: value },
        ];
      }
    }
  }

  selectLibraryHandler = async (event: CustomEvent<string>): Promise<void> => {
    event.stopPropagation();
    this.setSelectedLibrary(event.detail);
    this.setSelectedFolder("");
    this.messagesPlaceholder.clear();
    await this.loadTreeData();
    await this.loadGridData();
  }

  selectNodeHandler = async (event: CustomEvent<string>): Promise<void> => {
    event.stopPropagation();
    this.setSelectedFolder(event.detail);
    this.messagesPlaceholder.clear();
    await this.loadGridData();
  }

  loadTreeData = async (): Promise<void> => {
    const node: TreeNode = await this.getData(`${this.treeDataUrl}&libraryName=${encodeURIComponent(this.selectedLibraryName)}`);
    this.treeStructure = node === null ? {
      name: this.selectedLibraryName,
      children: [],
      path: "",
    } : node;
  }

  loadGridData = async (): Promise<void> => {
    this.filterQuery = null;
    this.loadingGridData = true;
    let url = `${this.dataUrl}&libraryName=${encodeURIComponent(this.selectedLibraryName)}&folderPath=${encodeURIComponent(this.selectedFolderPath)}`;
    if (this.allowedExtensions) {
      url += `&allowedExtensions=${encodeURIComponent(this.allowedExtensions)}`;
    }

    const mediaFiles: MediaFile[] = await this.getData(url);
    this.gridItems = mediaFiles === null ? [] : mediaFiles.map<GridItem>((file) => ({
      value: file.fileGuid,
      name: file.name + file.extension,
      thumbnailUrl: file.thumbnailUrls ? file.thumbnailUrls.medium : null,
    }));
    this.loadingGridData = false;
  }

  loadLibrariesData = async (): Promise<void> => {
    this.libraries = await this.getData(this.selectorDataUrl);

    if (this.libraries === null) {
      this.librariesSelectorData = [];
      return;
    }

    const selectorLibraries = this.libraryName !== null && this.libraryName !== "" ?
      this.libraries.filter((l) => l.identifier === this.libraryName) :
      this.libraries;

    this.librariesSelectorData = selectorLibraries.map<SelectorItem>((library) => ({
      value: library.identifier,
      name: library.name,
    }));
  }

  getData = async (url: string, method: string = "GET", data = null) => {
    if (this.errorMessage) {
      return null;
    }

    const response = await fetch(url, {
      method,
      headers: {
        "Content-Type": "application/json",
        "pragma": "no-cache",
        "cache-control": "no-cache",
      },
      body: data !== null ? JSON.stringify(data) : null,
    });

    if (response.status === 403) {
      this.errorMessage = this.getString("kentico.components.mediafileselector.notauthorized");
      return null;
    }

    if (response.status === 404) {
      this.errorMessage = this.getString("kentico.components.mediafileselector.librarynotfound");
      return null;
    }

    if (response.status === 500) {
      this.errorMessage = this.getString("kentico.components.mediafileselector.cannotloaddata");
      return null;
    }

    this.errorMessage = null;
    return response.json();
  }

  reloadGrid = async (): Promise<void> => {
    await this.loadGridData();
  }

  getUploaderOptions = (): UploaderOptions => {
    return {
      allowedExtensions: this.allowedExtensions,
      showEmptyContainer: this.gridItems.length === 0,
      enabled: this.selectedLibrary.createFile,
      title: this.getString("kentico.components.mediafileselector.emptyfolder"),
      uploadHandler: this.reloadGrid,
      url: `${this.uploaderUrl}&libraryName=${encodeURIComponent(this.selectedLibraryName)}&librarySubfolder=${this.selectedFolderPath}`,
    };
  }

  onFilter = (event) => {
    this.messagesPlaceholder.clear();
    this.filterQuery = event.target.value;
  }

  @Listen("uploadError")
  uploadErrorHandler(event: CustomEvent<string>) {
    event.stopPropagation();
    this.messagesPlaceholder.showError(event.detail);
  }

  @Listen("uploadSuccessful")
  uploadSuccessfulHandler(event: CustomEvent<string>) {
    event.stopPropagation();
    this.messagesPlaceholder.showSuccess(event.detail);
  }

  render() {
    const renderCounter = () => (
      <div class={{
        "ktc-counter": true,
        "ktc-counter-visible": this.selectedValues.length > 0,
      }} title={this.maxFilesLimit !== 0 ?
          this.getString("kentico.components.mediafileselector.countTitle", this.selectedValues.length, this.maxFilesLimit) :
          this.getString("kentico.components.mediafileselector.countNotLimitedTitle", this.selectedValues.length)}>
        {this.selectedValues.length}{this.maxFilesLimit !== 0 ? "/" + this.maxFilesLimit : ""}
      </div>
    );

    const renderLoader = () => (
      <div class="ktc-loader-container-wrapper">
        <div class="ktc-loader-container">
            <kentico-loader loaderMessage={this.getString("kentico.builder.modaldialogs.loading")} delayed={true} />
        </div>
      </div>
    );

    const renderError = () => (
      <div class="ktc-message-box">
        {this.errorMessage}
      </div>
    );

    const renderTree = () => (
      <kentico-media-files-tree
      getString={this.getString}
      selectedPath={this.selectedFolderPath}
      treeStructure={this.treeStructure}
      onSelectNode={this.selectNodeHandler} />
    );

    const renderGrid = () => (
      <kentico-media-files-grid
      getString={this.getString}
      selectedValues={this.selectedValues}
      items={this.gridItems}
      filterQuery={this.filterQuery}
      uploaderOptions={this.getUploaderOptions()}
      onChangeItem={this.selectItemHandler}/>
    );

    const renderHeader = () => (
      <div class="ktc-header-content">
      {this.librariesSelectorData.length > 1 &&
        <kentico-media-library-selector
        getString={this.getString}
        onSelectItem={this.selectLibraryHandler}
        selectedValue={this.selectedLibraryName}
        items={this.librariesSelectorData} />
      }
      {this.errorMessage === null &&
        <div class="ktc-media-filter">
          <input
            type="text"
            class="ktc-form-control"
            placeholder={this.getString("kentico.components.mediafileselector.filter.placeholder")}
            onInput={this.onFilter}
            value={this.filterQuery} />
        </div>
      }
      </div>
    );

    return (
      <div class="ktc-container">
        <div class="ktc-header">
          {renderHeader()}
        </div>
        <div class="ktc-content">
          {this.errorMessage === null && renderTree()}
          <div class="ktc-files-container">
            <kentico-messages-placeholder ref={(el) => this.messagesPlaceholder = el as any} getString={this.getString} />
            {!this.loadingGridData && this.errorMessage === null && renderGrid()}
            {this.errorMessage !== null && renderError()}
          </div>
          {this.maxFilesLimit !== 1 && renderCounter()}
          {this.loadingGridData && renderLoader()}
        </div>
      </div>
    );
  }
}
