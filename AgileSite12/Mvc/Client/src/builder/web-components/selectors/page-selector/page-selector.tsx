// ES6 methods are allowed in Stencil components because Stencil loads polyfills for missing ES6 methods
// tslint:disable:ban

import { Component, Listen, Method, Prop, State } from "@stencil/core";

import { DIALOG_FOOTER_HEIGHT, DIALOG_HEADER_HEIGHT } from "@/builder/constants";
import { GetString } from "../selector-types";
import { MillerColumn, SelectItemEventDetail } from "./miller-columns-types";
import { IdentifierMode, PageListingItem, SelectedPage, SelectedValue, TreeNode } from "./page-selector-types";

@Component({
  tag: "kentico-page-selector",
  styleUrl: "page-selector.less",
  shadow: true,
})
export class PageSelector {
  selectedPath: string;
  millerColumnsElement: HTMLElement;

  @State() selectedPage: PageListingItem;
  @State() pages: Array<MillerColumn<PageListingItem>>;

  @Prop() getString: GetString;
  @Prop() dataEndpoint: string;
  @Prop() aliasPathEndpoint: string;
  @Prop() rootPage: TreeNode;

  @Prop() identifierMode: IdentifierMode;
  @Prop() selectedValues: SelectedValue[];

  get selectedValueLabel() {
    return this.getString("kentico.components.pageselector.selectedvalue.label");
  }

  get selectedPaths() {
    const rootPath = this.rootPage.nodeAliasPath;

    // Selected path points to a different location than the root path => display the root path only
    if (!this.selectedPath.startsWith(rootPath)) {
      return [rootPath];
    }

    // Split and remove empty entries
    const pathParts = this.selectedPath.split("/").filter((pathPart) => Boolean(pathPart));
    let allPaths = pathParts.reduce((paths, path) => {
      const lastPath = paths[paths.length - 1];
      return [...paths, `${lastPath}${lastPath === "/" ? "" : "/"}${path}`];
    }, ["/"]);

    // Remove paths that are parents of the root path
    allPaths = allPaths.filter((path) => path.startsWith(rootPath));
    return allPaths;
  }

  @Method()
  getSelectedPage(): SelectedPage {
    if (this.selectedPage === undefined) {
      return null;
    }

    return {
      nodeId: this.selectedPage.nodeId,
      nodeGuid: this.selectedPage.nodeGuid,
      nodeAliasPath: this.selectedPage.nodeAliasPath,
      name: this.selectedPage.name,
      icon: this.selectedPage.icon,
      namePath: this.selectedPage.namePath,
      url: this.selectedPage.url,
      isValid: true,
    };
  }

  @Listen("select")
  async onSelectItem({ detail: { item, treeLevel } }: CustomEvent<SelectItemEventDetail>) {
    this.selectedPage = item as PageListingItem;
    const columnIndex = treeLevel + 1;

    if (this.selectedPage.showArrow) {
      const newColumn = await this.getListingItems(this.selectedPage.nodeAliasPath);
      this.pages = [
        ...this.pages.slice(0, columnIndex),
        {
          items: newColumn,
          parentItem: this.selectedPage,
        },
      ];
    } else if (!this.selectedPage.showArrow && (columnIndex < this.pages.length)) {
      this.pages = this.pages.slice(0, columnIndex);
    }
  }

  async componentWillLoad() {
    const parentItems = [this.getListingItem(this.rootPage)];
    let columns: PageListingItem[][] = [];
    const selectedIdentifier = this.getSelectedIdentifier();

    // Display already selected page
    if (selectedIdentifier) {

      this.selectedPath = await this.getSelectedPath(selectedIdentifier);

      if (this.selectedPath === this.rootPage.nodeAliasPath) {
        parentItems[0] = { ...parentItems[0], selected: true };
        columns.push(await this.getListingItems(this.rootPage.nodeAliasPath));
      } else {
        // Get child nodes for the parent nodes
        const selectedColumnsPromises =
          this.selectedPaths.slice(0, -1)
            .map((nodeAliasPath, index) => this.getListingItems(nodeAliasPath, this.selectedPaths[index + 1]));

        columns = await Promise.all(selectedColumnsPromises);

        // Collect parent items for pre-opened columns
        columns.slice(0, -1).forEach((items, columnIndex) => {
          const selectedColumnItem = items.find((item) => item.nodeAliasPath === this.selectedPaths[columnIndex + 1]);
          parentItems.push(selectedColumnItem);
        });
      }
    }

    // If no columns were retrieved according to the selected path or no page is selected => populate the root column with items specified by the root path.
    if (!columns.length) {
      columns.push(await this.getListingItems(this.rootPage.nodeAliasPath));
    }

    // Search for the selected page in the dialog root first
    if (this.selectedPath === this.rootPage.nodeAliasPath) {
      this.selectedPage = parentItems[0];
    } else {
      // In the last level of columns, search for the selected item by nodeAliasPath and if found, make it a selected item.
      this.selectedPage = columns[columns.length - 1].find((item) => item.nodeAliasPath === this.selectedPath);
    }

    this.pages = parentItems.map((parentItem, index): MillerColumn<PageListingItem> => ({
      parentItem,
      items: columns[index],
    }));
  }

  componentDidLoad() {
    this.resize();
    window.document.body.style.overflow = "hidden";
    window.parent.addEventListener("resize", this.resize);
  }

  componentWillUnload() {
    window.parent.removeEventListener("resize", this.resize);
  }

  resize = () => {
    const maxHeight = window.parent.innerHeight * 0.95 - DIALOG_FOOTER_HEIGHT - DIALOG_HEADER_HEIGHT - 16;
    const idealDialogHeight = 500;
    const selectedPathHeight = 48;
    const pageSelectorHeight = maxHeight > idealDialogHeight ? idealDialogHeight : maxHeight;
    const millerColumnsHeight = pageSelectorHeight - selectedPathHeight;
    const millerColumnsDiv = this.millerColumnsElement.querySelector<HTMLElement>(".ktc-miller-columns");

    window.document.body.style.height = `${pageSelectorHeight}px`;
    millerColumnsDiv.style.height = `${millerColumnsHeight}px`;

    window.kentico.modalDialog.resize();
  }

  async getSelectedPath(selectedIdentifier: string): Promise<string> {
    let aliasPath: string;
    if (this.identifierMode === IdentifierMode.Guid) {
      // Make a call to server to translate node GUID to alias path
      aliasPath = await this.getAliasPathForGuid(selectedIdentifier);
    } else if (this.identifierMode === IdentifierMode.Path) {
      aliasPath = selectedIdentifier;
    }

    return aliasPath;
  }

  async getListingItems(nodeAliasPath: string, preselectedNodeAliasPath: string = null): Promise<PageListingItem[]> {
    const pages = await this.getChildPages(nodeAliasPath);
    return pages.map((page) => this.getListingItem(page, page.nodeAliasPath === preselectedNodeAliasPath));
  }

  getListingItem = (page: TreeNode, selected = false): PageListingItem => ({
    nodeId: page.nodeId,
    nodeGuid: page.nodeGuid,
    nodeAliasPath: page.nodeAliasPath,
    identifier: page.nodeAliasPath,
    name: page.name,
    namePath: page.namePath,
    url: page.url,
    icon: page.icon,
    showArrow: page.nodeAliasPath === this.rootPage.nodeAliasPath ? false : page.hasChildNodes,
    selected,
  })

  getChildPages(nodeAliasPath: string): Promise<TreeNode[]> {
    const endpointUrl = this.getChildPagesEndpointUrl(nodeAliasPath);

    return this.getData(endpointUrl);
  }

  getChildPagesEndpointUrl(nodeAliasPath: string) {
    return `${this.dataEndpoint}&nodeAliasPath=${nodeAliasPath}`;
  }

  getAliasPathEndpointUrl(pageGuid: string) {
    return `${this.aliasPathEndpoint}&pageGuid=${pageGuid}`;
  }

  getSelectedIdentifier(): string {
    if ((this.selectedValues !== undefined) && this.selectedValues.length) {
      return this.selectedValues[0].identifier;
    }

    return undefined;
  }

  getSelectedPathLeftPart() {
    return this.splitSelectedPath()[0];
  }

  getSelectedPathRightPart() {
    return this.splitSelectedPath()[1];
  }

  splitSelectedPath() {
    const path = (this.selectedPage && this.selectedPage.namePath) || "";
    const lastSlash = path.lastIndexOf("/");
    return [path.substring(0, lastSlash), path.substring(lastSlash)];
  }

  async getAliasPathForGuid(pageGuid: string) {
    return this.getData(this.getAliasPathEndpointUrl(pageGuid));
  }

  async getData(url: string, method: string = "GET", data = null) {

    function handleErrors(response: Response): Response {
      if (!response.ok) {
          throw Error(response.statusText);
      }
      return response;
    }

    let responseJson: Promise<any>;
    await fetch(url, {
      method,
      headers: {
        "Content-Type": "application/json",
        "pragma": "no-cache",
        "cache-control": "no-cache",
      },
      body: data !== null ? JSON.stringify(data) : null,
    })
    .then(handleErrors)
    .then((response) => responseJson = response.json());

    return responseJson;
  }

  render() {
    return (
      <div class="ktc-page-selector">
        <kentico-miller-columns
          columns={this.pages}
          getString={this.getString}
          ref={(el) => this.millerColumnsElement = el}
        ></kentico-miller-columns>
        <div class="ktc-selected-value-wrapper">
          <div class="ktc-selected-value">
            <span class="ktc-selected-value-label">
              {`${this.selectedValueLabel}: `}
            </span>
            <span class="ktc-selected-value-path-left-part">
              {this.getSelectedPathLeftPart()}
            </span>
            <span>
              {this.getSelectedPathRightPart()}
            </span>
          </div>
        </div>
      </div>
    );
  }
}
