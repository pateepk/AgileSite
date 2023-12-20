/**
 * Module responsible for the store configuration.
 * @module store
 */

import { compose } from "redux";

const composeEnhancers = (developmentMode: boolean) => {
  const inDevMode = developmentMode || process.env.NODE_ENV === "development";
  const devToolsAvailable = window && window.__REDUX_DEVTOOLS_EXTENSION_COMPOSE__;

  return inDevMode && devToolsAvailable ?
    window.__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ :
    compose;
};

export {
  composeEnhancers,
};
