import { PopupListingElement } from "@/builder/declarations";

enum ABVariantListItemActionState {
  Hidden = "hidden",
  Disabled = "disabled",
  Enabled = "enabled",
}

interface AbTestVariantListingElement extends PopupListingElement {
  editActionState: ABVariantListItemActionState;
  removeActionState: ABVariantListItemActionState;
}

export {
  ABVariantListItemActionState,
  AbTestVariantListingElement,
};
