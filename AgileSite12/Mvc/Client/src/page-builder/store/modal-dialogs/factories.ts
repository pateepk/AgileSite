import _isEqual from "lodash.isequal";

import { PropertiesValidationResponseData } from "@/builder/declarations";
import { closeModalDialog } from "@/builder/store/thunks";
import { ModalDialogType } from "@/builder/store/types";

import { ThunkAction } from "@/page-builder/declarations";
import { postForm } from "@/page-builder/services/modal-dialog";

import { SetPageTemplatePropertiesThunk, SetSectionPropertiesThunk, SetWidgetPropertiesThunk } from "../types";
import { invalidateModalDialog, updateModalDialogMarkup } from "./actions";

type SetPropertiesThunk = SetWidgetPropertiesThunk | SetSectionPropertiesThunk | SetPageTemplatePropertiesThunk;

export class SubmitPropertiesDialogFormFactory {
  constructor(
    private readonly setWidgetPropertiesThunk: SetWidgetPropertiesThunk,
    private readonly setSectionPropertiesThunk: SetSectionPropertiesThunk,
    private readonly setPageTemplatePropertiesThunk: SetPageTemplatePropertiesThunk,
  ) { }

  public submitPropertiesDialogForm = (dialogIndex: number, actionUrl: string, currentProperties: object, formData: FormData): ThunkAction<Promise<void>> =>
    async (dispatch, getState, services) => {
      try {
        const response = await postForm(actionUrl, formData);

        if (this.isFormValid(response)) {
          const { modalDialogs } = getState();
          const currentDialog = modalDialogs.dialogs[dialogIndex];

          // Properties need to be merged with existing because only editable properties are fetched from server.
          const mergedProperties = Object.assign({}, currentProperties, response.data.properties);

          // Only update properties if changes were made
          if (!_isEqual(currentProperties, mergedProperties)) {
            const setProperties: SetPropertiesThunk = this.getSetPropertiesThunk(currentDialog.type);
            await dispatch(setProperties(currentDialog.identifier, mergedProperties));
          }

          dispatch(closeModalDialog(dialogIndex));
        } else {
          dispatch(invalidateModalDialog(dialogIndex));
          dispatch(updateModalDialogMarkup(dialogIndex, response.data));
        }
      } catch (error) {
        services.logger.logException(error);
      }
    }

  private getSetPropertiesThunk = (dialogType: ModalDialogType): SetPropertiesThunk => {
    switch (dialogType) {
      case ModalDialogType.WidgetProperties:
        return this.setWidgetPropertiesThunk;
      case ModalDialogType.SectionProperties:
        return this.setSectionPropertiesThunk;
      case ModalDialogType.TemplateProperties:
        return this.setPageTemplatePropertiesThunk;
    }
  }

  private isFormValid = (response: PropertiesValidationResponseData): boolean => (response.status === "valid");
}
