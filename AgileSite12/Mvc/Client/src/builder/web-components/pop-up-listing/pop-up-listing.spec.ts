import { TestWindow } from "@stencil/core/testing";

import { PopupListingElement } from "@/builder/declarations";
import { PopupListing } from "./pop-up-listing";

export const POPUP_LISTING_ITEMS: PopupListingElement[] = [
  { name: "NoDescriptionItem1", iconClass: "icon-test", key: "GUID1", description: "" },
  { name: "NoIconItem2", iconClass: null, key: "GUID2", description: "Item2" },
  { name: "NoIconNorDescriptionItem3", iconClass: null, key: "GUID3", description: "" },
  { name: "Item4", iconClass: "icon-test", key: "GUID4", description: "Item4" },
  { name: "Item5", iconClass: "icon-test", key: "GUID5", description: "Item5" },
];

describe("pop-up-listing", () => {
  it("should build", () => {
    expect(new PopupListing()).toBeTruthy();
  });

  describe("getItemTitle", () => {
    it("should return both name and description separated by new line", () => {
      const component = new PopupListing();
      const item = POPUP_LISTING_ITEMS[1];

      const expectedItemTitle = `${item.name}\n${item.description}`;
      const actualItemTitle = component.getItemTitle(item);

      expect(actualItemTitle).toBe(expectedItemTitle);
    });

    it("should return the name of the item", () => {
      const component = new PopupListing();
      const item = POPUP_LISTING_ITEMS[0];

      const expectedItemTitle = item.name;
      const actualItemTitle = component.getItemTitle(item);

      expect(actualItemTitle).toBe(expectedItemTitle);
    });
  });

  describe("onItemClick", () => {
    it("should emit event with item", () => {
      const component = new PopupListing();
      component.items = POPUP_LISTING_ITEMS;

      const selectItemSpy = jest.fn();
      component.selectItem = {
        emit: selectItemSpy,
      };

      component.onItemClick(POPUP_LISTING_ITEMS[0]);
      expect(selectItemSpy).toHaveBeenCalledWith(POPUP_LISTING_ITEMS[0]);
    });

    it("should not emit event when currently active item is passed", () => {
      const component = new PopupListing();
      component.items = POPUP_LISTING_ITEMS;
      component.activeItemIdentifier = POPUP_LISTING_ITEMS[0].key;

      const selectItemSpy = jest.fn();
      component.selectItem = {
        emit: selectItemSpy,
      };

      component.onItemClick(POPUP_LISTING_ITEMS[0]);
      expect(selectItemSpy).not.toHaveBeenCalled();
    });
  });

  describe("rendering", () => {
    let component: HTMLKenticoPopUpListingElement;
    let testWindow: TestWindow;

    beforeEach(async () => {
      testWindow = new TestWindow();
      component = await testWindow.load({
        components: [PopupListing],
        html: "<kentico-pop-up-listing></kentico-pop-up-listing>",
      });
    });

    it("should render with no items available message", async () => {
      component.noItemsAvailableMessage = "TEST-MESSAGE";
      await testWindow.flush();

      expect(component).toMatchSnapshot();
    });

    it("should render with item list", async () => {
      component.items = POPUP_LISTING_ITEMS;
      await testWindow.flush();

      expect(component).toMatchSnapshot();
    });
  });

  describe("item click", () => {
    let component: HTMLKenticoPopUpListingElement;
    let testWindow: TestWindow;

    beforeEach(async () => {
      testWindow = new TestWindow();
      component = await testWindow.load({
        components: [PopupListing],
        html: "<kentico-pop-up-listing></kentico-pop-up-listing>",
      });
      component.items = [ POPUP_LISTING_ITEMS[0] ];
    });

    it("click on normal item should emit event", async () => {
      await testWindow.flush();
      const eventSpy = jest.fn();
      testWindow.document.addEventListener("selectItem", eventSpy);

      component.querySelector("li").click();

      expect(eventSpy).toHaveBeenCalled();
    });

    it("click on active item should not emit event", async () => {
      component.activeItemIdentifier = POPUP_LISTING_ITEMS[0].key;
      await testWindow.flush();
      const eventSpy = jest.fn();
      testWindow.document.addEventListener("selectItem", eventSpy);

      component.querySelector("li").click();

      expect(eventSpy).not.toHaveBeenCalled();
    });
  });
});
