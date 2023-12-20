import { ActionType } from "typesafe-actions";

import * as actions from "./actions";
import { addSection } from "./thunks";

export type AddSectionThunk = typeof addSection;

export type AddSectionAction = ActionType<typeof actions.addSection>;
export type RemoveSectionAction = ActionType<typeof actions.removeSection>;
export type MoveSectionAction = ActionType<typeof actions.moveSection>;
export type ChangeSectionAction = ActionType<typeof actions.changeSection>;
export type SectionsAction = ActionType<typeof actions>;
