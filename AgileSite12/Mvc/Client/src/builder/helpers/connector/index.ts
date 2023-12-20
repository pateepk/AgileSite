import { ActionCreatorsMapObject, bindActionCreators, Store } from "redux";
import Vue, { ComponentOptions, CreateElement, VueConstructor } from "vue";

import { State } from "@/builder/declarations";

import { normalizeProps, normalizeSlots, shallowEqual } from "./utils";

const defaultMapStateToProps = () => ({});
const defaultMapDispatchToProps = () => ({});

interface ProviderComponent extends Vue {
  state: any;
  unsubscribe: () => void;
  readonly _events: any;
  readonly store: Store;
  readonly stateToProps: object;
  readonly dispatchToProps: object;
  readonly updateState: () => void;
}

/**
 * Represents state to props mapping function.
 */
export type MapStateToProps<
  TState extends State = State,
  TContext extends object = object,
  TProps extends object = object> = (state: TState, context: TContext) => TProps;

/**
 * Represents dispatch to props mapping function.
 */
export type MapDispatchToProps<TActions extends object = object> = () => TActions;

/**
 * Connects a Vue component to a Redux store.
 * @param mapStateToProps Maps state from the store to corresponding component properties.
 * @param mapDispatchToProps Maps action creators to corresponding component properties and wraps them into a dispatch call.
 */
export const connect = (
  mapStateToProps: MapStateToProps = defaultMapStateToProps,
  mapDispatchToProps: MapDispatchToProps = defaultMapDispatchToProps,
) => (component: VueConstructor): ComponentOptions<Vue> => ({
  name: `connected-${component.name}`,
  inject: ["store"],
  data: () => ({
    state: null,
  }),
  props: {
    ...normalizeProps((component as VueConstructor & { options: ComponentOptions<Vue> }).options.props),
  },
  render(this: ProviderComponent, h: CreateElement) {
    return h(component, {
      on: this._events,
      props: {
        ...this.$props,
        ...this.stateToProps,
        ...this.dispatchToProps,
      },
      scopedSlots: this.$scopedSlots,
    }, normalizeSlots(this.$slots));
  },
  computed: {
    stateToProps(this: ProviderComponent) {
      return mapStateToProps(this.state, this.$options.propsData);
    },
    dispatchToProps(this: ProviderComponent) {
      return {
        ...bindActionCreators(mapDispatchToProps() as ActionCreatorsMapObject, this.store.dispatch),
      };
    },
  },
  methods: {
    updateState(this: ProviderComponent) {
      this.state = this.store.getState();
    },
  },
  created(this: ProviderComponent) {
    // we use the parent's $createElement instead of our own
    // this is necessary so that the wrapped component can properly resolve the slots.
    // https://github.com/vuejs/vue/issues/6201#issuecomment-318927628
    this.$createElement = this.$parent.$createElement;

    const observeStore = (store, onChange) => {
      let previousMappedState = mapStateToProps(store.getState(), this.$options.propsData || {});

      return store.subscribe(() => {
        const nextMappedState = mapStateToProps(store.getState(), this.$options.propsData);
        if (!shallowEqual(previousMappedState, nextMappedState)) {
          previousMappedState = nextMappedState;
          onChange();
        }
      });
    };

    this.unsubscribe = observeStore(this.store, this.updateState);

    this.updateState();
  },
  destroyed(this: ProviderComponent) {
    if (this.unsubscribe) {
      this.unsubscribe();
    }
  },
});

/**
 * Creates a mixin for providing store to components.
 * @param store Redux store.
 */
export const createStoreProviderMixin = (store: Store<State>): ComponentOptions<Vue> => ({
  data: {
    store,
  },
  provide() {
    return {
      store: (this as any).store,
    };
  },
});
