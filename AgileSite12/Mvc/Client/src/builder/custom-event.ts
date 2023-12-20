// https://developer.mozilla.org/en-US/docs/Web/API/CustomEvent/CustomEvent#Polyfill

// tslint:disable-next-line:only-arrow-functions
(function() {
  if (typeof (window as any).CustomEvent === "function") {
    return false;
  }

  function CustomEvent(event, params) {
    params = params || { bubbles: false, cancelable: false, detail: undefined };
    const evt = document.createEvent("CustomEvent");
    evt.initCustomEvent(event, params.bubbles, params.cancelable, params.detail);

    return evt;
  }

  CustomEvent.prototype = (window as any).Event.prototype;

  (window as any).CustomEvent = CustomEvent;
}());
