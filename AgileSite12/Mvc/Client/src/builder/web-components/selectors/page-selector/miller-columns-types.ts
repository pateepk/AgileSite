export interface ListingItem {
  readonly identifier: string;
  readonly name: string;
  readonly icon: string;
  readonly showArrow: boolean;
  readonly selected: boolean;
}

export interface SelectItemEventDetail {
  readonly item: ListingItem;
  readonly treeLevel: number;
}

export interface MillerColumn<T extends ListingItem> {
  readonly items: T[];
  readonly parentItem: T;
}
