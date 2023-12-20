import { injectable } from "inversify";

import { ComponentMetadata, IPopUpElementsService, PopupListingElement } from "@/builder/declarations";

@injectable()
class PopUpElementsService implements IPopUpElementsService {
  getWidgetElements(metadata: ComponentMetadata[]): PopupListingElement[] {
    return metadata.map(({ typeIdentifier, iconClass, name, description }) => ({
      description,
      iconClass,
      name,
      key: typeIdentifier,
    }));
  }

  getSectionElements(metadata: ComponentMetadata[]): PopupListingElement[] {
    return this.getWidgetElements(metadata);
  }
}

export {
  PopUpElementsService,
};
