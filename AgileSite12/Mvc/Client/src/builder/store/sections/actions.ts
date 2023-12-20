import { createAction } from "typesafe-actions";

import { Section, SectionMarkup, Widget } from "@/builder/declarations";

export const addSection = createAction("sections/ADD", (resolve) =>
  (section: Section, areaIdentifier: string, position: number, markup: SectionMarkup) => resolve({
    section,
    areaIdentifier,
    position,
    markup,
  }),
);

export const removeSection = createAction("sections/REMOVE", (resolve) =>
  (section: Section, areaIdentifier: string, widgetsToRemove: Widget[]) => resolve({
    section,
    areaIdentifier,
    widgetsToRemove,
  }),
);

export const moveSection = createAction("sections/MOVE", (resolve) =>
  (sectionIdentifier: string, originalAreaIdentifier: string, targetAreaIdentifier: string, position: number) => resolve({
    sectionIdentifier,
    originalAreaIdentifier,
    targetAreaIdentifier,
    position,
  }),
);

export const changeSection = createAction("sections/CHANGE", (resolve) =>
  (oldSection: Section, newSection: Section, newMarkup: SectionMarkup, widgetsToRemove: Widget[] = []) => resolve({
    oldSection,
    newSection,
    newMarkup,
    widgetsToRemove,
  }),
);
