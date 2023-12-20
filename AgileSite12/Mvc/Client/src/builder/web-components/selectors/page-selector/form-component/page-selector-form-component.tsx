import { Component, Element, Event, EventEmitter, Prop, State } from "@stencil/core";

import { PageSelectorDialogOptions } from "@/builder/services/modal-dialog/selectors/page-selector";
import { IdentifierMode, PageSelectorItem, SelectedPage, SelectedValue } from "@/builder/web-components/selectors/page-selector/page-selector-types";
import { GetString } from "@/builder/web-components/selectors/selector-types";

@Component({
  tag: "kentico-page-selector-form-component",
  styleUrl: "page-selector-form-component.less",
  shadow: false,
})
export class PageSelectorFormComponent {
  hiddenInput: HTMLElement;

  @Element() el: HTMLElement;

  @State() selectedPage: SelectedPage = null;

  @Prop() getString: GetString;
  @Prop() inputName: string;
  @Prop() mode: IdentifierMode;
  @Prop() selectedPageData: string;
  @Prop() rootPath: string;

  @Event() kenticoPropertiesDialogInputInit: EventEmitter<HTMLElement>;

  componentWillLoad() {
    if (this.selectedPageData) {
      this.selectedPage = JSON.parse(this.selectedPageData) as SelectedPage;
    }
  }

  componentDidLoad() {
    this.kenticoPropertiesDialogInputInit.emit(this.el);
  }

  openDialog = () => {
    const options: PageSelectorDialogOptions = {
      applyCallback: (pages: SelectedPage[]) => {
        if (pages && pages[0]) {
          this.selectedPage = pages[0];
          this.fireChangeEvent();
        }

        return { closeDialog: true };
      },
      selectedValues: this.getDialogSelectedValues(),
      identifierMode: this.mode,
      rootPath: this.rootPath,
    };

    window.kentico.modalDialog.pageSelector.open(options);
  }

  clearFormComponent = () => {
    this.selectedPage = null;
    this.fireChangeEvent();
  }

  fireChangeEvent = () => {
    this.hiddenInput.dispatchEvent(new CustomEvent("change"));
  }

  getDialogSelectedValues = (): SelectedValue[] => {
    if (this.selectedPage && this.selectedPage.isValid) {
      return [{ identifier: this.mode === IdentifierMode.Guid ? this.selectedPage.nodeGuid : this.selectedPage.nodeAliasPath }];
    }
    return [];
  }

  getInputValue = (): string => {
    if (this.selectedPage) {
      const pageSelectorItem: PageSelectorItem = {
        nodeGuid: this.selectedPage.nodeGuid,
        nodeAliasPath: this.selectedPage.nodeAliasPath,
      };

      return JSON.stringify([pageSelectorItem]);
    }
    return "";
  }

  renderPageName = () =>
    <div class="ktc-page-selector-selected-page" title={this.selectedPage.namePath}>
      {this.selectedPage.icon && <i aria-hidden="true" class={this.selectedPage.icon}></i>}
      {this.selectedPage.name}
    </div>

  renderNoPageSelected = () =>
    <div class="ktc-page-selector-selected-page">{this.getString("kentico.components.pageselector.nopageselected")}</div>

  renderInvalidPage = () =>
    <div class="ktc-page-selector-selected-page ktc-invalid-value" title={this.getString("kentico.components.pageselector.invalidvalue.explanationtext")}>
      <i aria-hidden="true" class="icon-exclamation-triangle"></i>
      {this.getString("kentico.components.pageselector.invalidvalue")}
    </div>

  render() {
    return (
      <div class="ktc-page-selector-form-component-wrapper">
        <div class="ktc-page-selector-form-component">
          {!this.selectedPage && this.renderNoPageSelected()}
          {this.selectedPage && this.selectedPage.isValid && this.renderPageName()}
          {this.selectedPage && !this.selectedPage.isValid && this.renderInvalidPage()}
          <div class="ktc-page-selector-buttons">
            <button type="button" class="ktc-btn ktc-btn-default select-button" onClick={this.openDialog}>
              {this.getString("kentico.components.pageselector.button.select")}
            </button>
            <button type="button" class="ktc-btn ktc-btn-default select-button" onClick={this.clearFormComponent}>
              {this.getString("kentico.components.pageselector.button.clear")}
            </button>
          </div>
        </div>
        <input type="hidden" ref={(el) => this.hiddenInput = el} name={this.inputName} value={this.getInputValue()} />
      </div>
    );
  }
}
