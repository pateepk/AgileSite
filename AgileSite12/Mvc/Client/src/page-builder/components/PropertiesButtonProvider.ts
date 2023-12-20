import { LocalizationService } from "@/builder/declarations";
import { connect } from "@/builder/helpers/connector";
import { OpenModalDialogFactory } from "@/builder/store/modal-dialogs/factories";
import { DialogData } from "@/builder/store/modal-dialogs/types";
import { ModalDialogType, OpenModalDialogOptions } from "@/builder/store/types";
import { Theme } from "@/builder/types";

import { ButtonComponentActions, PageBuilderState } from "../declarations";
import PropertiesButton from "./PropertiesButton.vue";

const mapDispatchToProps = (): ButtonComponentActions => ({
  openModalDialog: new OpenModalDialogFactory(getPropertiesDialogData).openModalDialog,
});

const getPropertiesDialogData = (options: OpenModalDialogOptions, state: PageBuilderState, localizationService: LocalizationService): DialogData => {
  const { componentIdentifier, dialogType} = options;
  const { displayedWidgetVariants, metadata, sections, widgets, pageTemplate, widgetVariants } = state;
  let dialogData: DialogData;

  switch (dialogType) {
    case ModalDialogType.WidgetProperties:
      dialogData = {
        title: metadata.widgets[widgets[componentIdentifier].type].name,
        markupUrl: metadata.widgets[widgets[componentIdentifier].type].propertiesFormMarkupUrl,
        model: widgetVariants[displayedWidgetVariants[componentIdentifier]].properties,
        theme: Theme.Widget,
      };
      break;

    case ModalDialogType.SectionProperties:
      dialogData = {
        title: metadata.sections[sections[componentIdentifier].type].name,
        markupUrl: metadata.sections[sections[componentIdentifier].type].propertiesFormMarkupUrl,
        model: sections[componentIdentifier].properties,
        theme: Theme.Section,
      };
      break;

    case ModalDialogType.TemplateProperties:
      dialogData = {
        title: metadata.pageTemplates[pageTemplate.identifier].name,
        markupUrl: metadata.pageTemplates[pageTemplate.identifier].propertiesFormMarkupUrl,
        model: pageTemplate.properties,
        theme: Theme.Template,
      };
      break;
  }

  return {
    ...dialogData,
    title: localizationService.getLocalization("widgetproperties.header", dialogData.title),
  };
};

export default connect(() => ({}), mapDispatchToProps)(PropertiesButton);
