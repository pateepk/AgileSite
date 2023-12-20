import { PageSelectorFormComponent } from "./page-selector-form-component";

describe("kentico-page-selector-form-component", () => {
  beforeEach(() => {
    window.kentico = window.kentico || {};
    window.kentico.localization = window.kentico.localization || { getString: (test: string) => test};
  });
  it("should build", () => {
    expect(new PageSelectorFormComponent()).toBeTruthy();
  });
});
