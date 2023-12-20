import { ListingItem } from "./miller-columns-types";

export interface PageListingItem extends ListingItem {
  readonly nodeId: string;
  readonly nodeGuid: string;
  readonly namePath: string;
  readonly nodeAliasPath: string;
  readonly url: string;
}

export interface TreeNode {
  readonly nodeId: string;
  readonly nodeGuid: string;
  readonly nodeAliasPath: string;
  readonly name: string;
  readonly namePath: string;
  readonly url: string;
  readonly icon: string;
  readonly hasChildNodes: boolean;
}

export interface PageSelectorItem {
  nodeGuid: string;
  nodeAliasPath: string;
}

export interface SelectedPage {
  readonly nodeId: string;
  readonly nodeGuid: string;
  readonly nodeAliasPath: string;
  readonly icon: string;
  readonly name: string;
  readonly namePath: string;
  readonly url: string;
  readonly isValid: boolean;
}

export interface SelectedValue {
  readonly identifier: string;
}

export enum IdentifierMode {
  Path = "path",
  Guid = "guid",
}
