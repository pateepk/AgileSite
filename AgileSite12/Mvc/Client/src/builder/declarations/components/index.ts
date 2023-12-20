import Vue from "vue";

export * from "./section";
export * from "./widget";
export * from "./widget-zone";
export * from "./border-container";
export * from "./editable-area";
export * from "./widget-markup";
export * from "./pop-up-listing";
export * from "./global-component-wrapper";

export type Refs<T extends object> = Vue["$refs"] & T;
