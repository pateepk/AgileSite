import { injectable } from "inversify";

import { ComponentMetadata, PopupListingElement } from "@/builder/declarations";
import { PopUpElementsService } from "@/builder/services/PopUpElementsService";

import { UNKNOWN_SECTION_TYPE_NAME } from "@/form-builder/constants";

@injectable()
class FormBuilderPopUpElementsService extends PopUpElementsService {
  getSectionElements(metadata: ComponentMetadata[]): PopupListingElement[] {
    metadata = metadata.filter((data) => data.typeIdentifier !== UNKNOWN_SECTION_TYPE_NAME);

    return this.getWidgetElements(metadata);
  }
}

export {
  FormBuilderPopUpElementsService,
};
