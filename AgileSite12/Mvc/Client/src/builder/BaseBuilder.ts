import { injectable } from "inversify";
import { Store } from "redux";
import Vue, { Component as VueComponent, CreateElement } from "vue";

import { initHttpClient } from "@/builder/api/client";
import { BuilderConfig } from "@/builder/builderConfig";
import { GLOBAL_WRAPPER_ELEMENT_CLASS } from "@/builder/constants";
import { LocalizationService, State } from "@/builder/declarations";
import { initializeDragAndDrop, initializeSectionDragAndDrop } from "@/builder/drag-and-drop";
import { calculateListingOffset, getPosition } from "@/builder/helpers/position";
import { addCancelScreenLockEventListeners } from "@/builder/helpers/screen-lock";
import { logger } from "@/builder/logger";
import { MessagingService } from "@/builder/services/MessagingService";
import { PageConfigurationService } from "@/builder/services/page-configuration/PageConfigurationService";
import { postMessage } from "@/builder/services/post-message";
import { setPopupPosition } from "@/builder/store/pop-up/actions";
import { MessageTypes } from "@/builder/types";

import { createStoreProviderMixin } from "./helpers/connector";
import { registerModalDialogApi } from "./services/modal-dialog";

const ignoredElements = [
  /^kentico-/,
];

Vue.config.ignoredElements = ignoredElements;
// Turns off the 'You are running Vue in development mode...' warning on Vue startup.
Vue.config.productionTip = false;

@injectable()
export abstract class Builder {
  /**
   * Indicates whether the page builder was already initialized.
   */
  private wasInitialized = false;

  public constructor(
    protected readonly localizationService: LocalizationService,
    private readonly pageConfigurationService: PageConfigurationService,
    private readonly messagingService: MessagingService,
  ) { }

  /**
   * Initializes the builder application.
   * @param config Builder configuration object
   * @param configureStore Configuration function for the store.
   * @param areaComponent Initialization area component.
   * @param moveWidgetAction Function used for widget move handling.
   * @param moveSectionAction Function used for section move handling.
   */
  protected async initialize<TState extends State>(
    config: BuilderConfig,
    configureStore: (state: State) => Store<TState>,
    globalComponentWrapper: VueComponent,
    areaComponent: VueComponent,
    moveWidgetAction,
    moveSectionAction,
  ): Promise<Store<TState>> {
    if (this.wasInitialized) {
      return;
    }

    this.wasInitialized = true;

    try {
      config.validate();

      // Provide localization service to all components
      Vue.prototype.$_localizationService = this.localizationService;

      // Need to initialize the client before making any API calls
      initHttpClient(config);

      const pageConfiguration = await this.pageConfigurationService.getConfiguration(config);
      const store = configureStore(pageConfiguration);

      registerModalDialogApi(store.dispatch, this.localizationService);

      this.messagingService.registerListener(store.getState, store.dispatch, config.allowedOrigins);

      // Request displayed variants from session storage
      postMessage(MessageTypes.GET_DISPLAYED_WIDGET_VARIANTS, null, "*");

      // Update widget list position on resize event
      window.addEventListener("resize", () => {
        this.updatePopupContainerPosition(store);
      });

      initializeDragAndDrop(store.dispatch, store.getState, moveWidgetAction);
      this.initializeEditableAreas(store, areaComponent);
      initializeSectionDragAndDrop(store.dispatch, store.getState, moveSectionAction);

      this.initializeGlobalComponentWrapper(store, globalComponentWrapper);
      addCancelScreenLockEventListeners();

      return store;
    } catch (err) {
      logger.logException(err);
    }
  }

  private getGlobalComponentWrapperHost(): HTMLElement {
    return document.body;
  }

  /**
   * Initializes global component wrapper.
   * @param store Redux store.
   */
  private initializeGlobalComponentWrapper(store: Store<State>, globalComponentWrapper: VueComponent) {
    this.createGlobalComponentWrapperHost();
    new Vue({
      mixins: [createStoreProviderMixin(store)],
      render: (h: CreateElement) => h(globalComponentWrapper),
    }).$mount(`.${GLOBAL_WRAPPER_ELEMENT_CLASS}`);
  }

  /**
   * Creates a HTML element to host the GlobalComponentWrapper component.
   */
  private createGlobalComponentWrapperHost() {
    const globalComponentSlot = document.createElement("div");
    globalComponentSlot.classList.add(GLOBAL_WRAPPER_ELEMENT_CLASS);
    this.getGlobalComponentWrapperHost().appendChild(globalComponentSlot);
  }

  /**
   * Updates widget list position according to the browser's viewport.
   * @param store Store with current page configuration.
   */
  private updatePopupContainerPosition(store: Store<State>) {
    const componentIdentifier = store.getState().popup.componentIdentifier;

    if (componentIdentifier !== null) {
      const addComponent: Element = document.querySelector(
        "kentico-add-component-button[is-active='true'], " +
        "kentico-personalization-button[show-personalization-popup]");

      if (addComponent !== null) {
        const clientRect = addComponent.getBoundingClientRect();
        const newPosition = getPosition(clientRect);
        const newOffset = calculateListingOffset(clientRect);

        store.dispatch(setPopupPosition(newPosition, newOffset));
      }
    }
  }

  private initializeEditableAreas(store: Store<State>, areaComponent: VueComponent) {
    const editableAreasIdentifiers = Object.keys(store.getState().editableAreas);

    editableAreasIdentifiers.forEach((areaId) => {
      new Vue({
        mixins: [createStoreProviderMixin(store)],
        render: (h) => h(areaComponent, {
          props: {
            identifier: areaId,
          },
        }),
      }).$mount(`[data-kentico-editable-area-id="${areaId}"]`);
    });
  }
}
