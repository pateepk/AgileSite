/**
 * Type declerations for global development variables
 */

declare interface Window {
  __REDUX_DEVTOOLS_EXTENSION_COMPOSE__?: <F extends Function>(f: F) => F;
  kentico: any;
  Event: any;
}

type Nullable<T> = T | null;

/**
 * Makes read-only collection mutable.
 * Caution: Use only in tests!
 */
type Mutable<T> = {
  -readonly [P in keyof T]: T[P] extends ReadonlyArray<infer U> ? Mutable<U>[] : Mutable<T[P]>
};
