import { TestWindow } from "@stencil/core/testing";

import { DialogHeader } from "./dialog-header";

describe("drop-marker", () => {
  it("should build", () => {
    expect(new DialogHeader()).toBeTruthy();
  });

  describe("rendering", () => {
    const TEST_HEADER_TITLE = "Test";
    let component: HTMLKenticoDialogHeaderElement;
    let testWindow: TestWindow;

    beforeEach(async () => {
      testWindow = new TestWindow();
      component = await testWindow.load({
        components: [DialogHeader],
        html: "<kentico-dialog-header></kentico-dialog-header>",
      });
      component.headerTitle = TEST_HEADER_TITLE;
    });

    it("should have title visible when set", async () => {
      await testWindow.flush();

      const title = component.querySelector(".ktc-dialog-header-title").textContent;

      expect(title).toBe(TEST_HEADER_TITLE);
    });

    it("should not display back button when showBackButton set to false", async () => {
      await testWindow.flush();

      const backButton = component.querySelectorAll(".ktc-dialog-header-back");
      const isBackButtonVisible = backButton.length > 0;

      expect(isBackButtonVisible).toBe(false);
    });

    it("should dispatch 'close' event when clicking on close button", async () => {
      await testWindow.flush();
      const eventSpy = jest.fn();
      testWindow.document.addEventListener("close", eventSpy);

      const closeButton = component.querySelector<HTMLElement>(".ktc-dialog-header-controls > a");
      closeButton.click();

      expect(eventSpy).toHaveBeenCalled();
    });

    it("should dispatch 'back' event when clicking on back button", async () => {
      component.showBackButton = true;
      await testWindow.flush();
      const eventSpy = jest.fn();
      testWindow.document.addEventListener("back", eventSpy);

      const backButton = component.querySelector<HTMLElement>(".ktc-dialog-header-back > a");
      backButton.click();

      expect(eventSpy).toHaveBeenCalled();
    });
  });
});
