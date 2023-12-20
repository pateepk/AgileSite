/**
 * Class name which applies the styles to prevent page scrolling.
 */
const NO_SCROLL_CLASS = "ktc-no-scroll";

/**
 * Enables scrolling on the page.
 * @param document Document object.
 */
export const enablePageScrolling = (document: Document) => {
  document.documentElement.classList.remove(NO_SCROLL_CLASS);
};

/**
 * Disables scrolling on the page.
 * @param document Document object.
 */
export const disablePageScrolling = (document: Document) => {
  document.documentElement.classList.add(NO_SCROLL_CLASS);
};
