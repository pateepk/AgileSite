import { PopupListingElement } from "@/builder/declarations";

export interface VariantListingElement extends PopupListingElement {
  renderActionButtons: boolean;
  parameters: object;
}
