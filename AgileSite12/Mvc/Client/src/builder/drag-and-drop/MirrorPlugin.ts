import { Draggable } from "@shopify/draggable/lib/es5/draggable.bundle.legacy";

// BEGIN Kentico
const draggableSymbols = Object.getOwnPropertySymbols(Draggable.Plugins.Mirror.prototype);
const onMirrorCreated = draggableSymbols[4];
const onMirrorMove = draggableSymbols[5];
// END Kentico

export class MirrorPlugin extends Draggable.Plugins.Mirror {
  public draggable: any;
  public initialX: any;
  public initialY: any;
  public mirrorOffset: any;
  public options: any;
  public scrollOffset: any;

  /**
   * Mirror created handler
   * @param {MirrorCreatedEvent} mirrorEvent
   * @return {Promise}
   * @private
   */
  [onMirrorCreated]({ mirror, source, sensorEvent }) {
    const mirrorClass = this.draggable.getClassNameFor("mirror");

    const setState = ({ mirrorOffset, initialX, initialY, ...args }) => {
      this.mirrorOffset = mirrorOffset;
      this.initialX = initialX;
      this.initialY = initialY;
      return { mirrorOffset, initialX, initialY, ...args };
    };

    const initialState = {
      mirror,
      mirrorClass,
      sensorEvent,
      source,
      options: this.options,
      scrollOffset: this.scrollOffset,
    };

    return (
      Promise.resolve(initialState)
        // Fix reflow here
        .then(computeMirrorDimensions)
        .then(calculateMirrorOffset)
        .then(resetMirror)
        .then(addMirrorClasses)
        .then(positionMirror({ initial: true }))
        .then(removeMirrorID)
        .then(setState)
    );
  }

  /**
   * Mirror move handler
   * @param {MirrorMoveEvent} mirrorEvent
   * @return {Promise|null}
   * @private
   */
  [onMirrorMove](mirrorEvent) {
    if (mirrorEvent.canceled()) {
      return null;
    }

    const initialState = {
      initialX: this.initialX,
      initialY: this.initialY,
      mirror: mirrorEvent.mirror,
      mirrorOffset: this.mirrorOffset,
      options: this.options,
      scrollOffset: this.scrollOffset,
      sensorEvent: mirrorEvent.sensorEvent,
    };

    return Promise.resolve(initialState).then(positionMirror({ withFrame: true }));
  }
}

/**
 * Computes mirror dimensions based on the source element
 * Adds sourceRect to state
 * @param {Object} state
 * @param {HTMLElement} state.source
 * @return {Promise}
 * @private
 */
function computeMirrorDimensions({ source, ...args }) {
  return withPromise((resolve) => {
    const sourceRect = source.getBoundingClientRect();
    resolve({ source, sourceRect, ...args });
  });
}

/**
 * Applies mirror styles
 * @param {Object} state
 * @param {HTMLElement} state.mirror
 * @param {HTMLElement} state.source
 * @param {Object} state.options
 * @return {Promise}
 * @private
 */
function resetMirror({ mirror, source, options, ...args }) {
  return withPromise((resolve) => {
    let offsetHeight;
    let offsetWidth;

    if (options.constrainDimensions) {
      const computedSourceStyles = getComputedStyle(source);
      offsetHeight = computedSourceStyles.getPropertyValue("height");
      offsetWidth = computedSourceStyles.getPropertyValue("width");
    }

    mirror.style.position = "fixed";
    mirror.style.pointerEvents = "none";
    mirror.style.top = 0;
    mirror.style.left = 0;
    mirror.style.margin = 0;

    // BEGIN Kentico
    mirror.style.transformOrigin = "0 0";
    mirror.style.opacity = "0.7";
    mirror.style.backgroundColor = getNearestExplicitBackgroundColor(mirror);
    // END Kentico

    if (options.constrainDimensions) {
      mirror.style.height = offsetHeight;
      mirror.style.width = offsetWidth;
    }

    resolve({ mirror, source, options, ...args });
  });
}

/**
 * Calculates mirror offset
 * Adds mirrorOffset to state
 * @param {Object} state
 * @param {SensorEvent} state.sensorEvent
 * @param {DOMRect} state.sourceRect
 * @return {Promise}
 * @private
 */
function calculateMirrorOffset({ sensorEvent, sourceRect, options, ...args }) {
  return withPromise((resolve) => {
    const top = options.cursorOffsetY === null ? (sensorEvent.clientY - sourceRect.top) /*BEGIN Kentico*/ * 0.5 /*END Kentico*/ : options.cursorOffsetY;
    const left = options.cursorOffsetX === null ? (sensorEvent.clientX - sourceRect.left) /*BEGIN Kentico*/ * 0.5 /*END Kentico*/ : options.cursorOffsetX;

    const mirrorOffset = { top, left };

    resolve({ sensorEvent, sourceRect, mirrorOffset, options, ...args });
  });
}

/**
 * Positions mirror with translate3d
 * @param {Object} state
 * @param {HTMLElement} state.mirror
 * @param {SensorEvent} state.sensorEvent
 * @param {Object} state.mirrorOffset
 * @param {Number} state.initialY
 * @param {Number} state.initialX
 * @param {Object} state.options
 * @return {Promise}
 * @private
 */
function positionMirror({ withFrame = false, initial = false } = {}) {
  return ({ mirror, sensorEvent, mirrorOffset, initialY, initialX, scrollOffset, options, ...args }) => {
    return withPromise(
      (resolve) => {
        const result: any = {
          mirror,
          mirrorOffset,
          options,
          sensorEvent,
          ...args,
        };
        if (mirrorOffset) {
          const x = sensorEvent.clientX - mirrorOffset.left - scrollOffset.x;
          const y = sensorEvent.clientY - mirrorOffset.top - scrollOffset.y;

          if ((options.xAxis && options.yAxis) || initial) {
            mirror.style.transform = `translate3d(${x}px, ${y}px, 0) ${/*BEGIN Kentico*/"scale(0.5)"/*END Kentico*/}`;
          } else if (options.xAxis && !options.yAxis) {
            mirror.style.transform = `translate3d(${x}px, ${initialY}px, 0)`;
          } else if (options.yAxis && !options.xAxis) {
            mirror.style.transform = `translate3d(${initialX}px, ${y}px, 0)`;
          }

          if (initial) {
            result.initialX = x;
            result.initialY = y;
          }
        }

        resolve(result);
      },
      { raf: withFrame },
    );
  };
}

/**
 * Applies mirror class on mirror element
 * @param {Object} state
 * @param {HTMLElement} state.mirror
 * @param {String} state.mirrorClass
 * @return {Promise}
 * @private
 */
function addMirrorClasses({ mirror, mirrorClass, ...args }) {
  return withPromise((resolve) => {
    mirror.classList.add(mirrorClass);
    resolve({ mirror, mirrorClass, ...args });
  });
}

/**
 * Removes source ID from cloned mirror element
 * @param {Object} state
 * @param {HTMLElement} state.mirror
 * @return {Promise}
 * @private
 */
function removeMirrorID({ mirror, ...args }) {
  return withPromise((resolve) => {
    mirror.removeAttribute("id");
    delete mirror.id;
    resolve({ mirror, ...args });
  });
}

/**
 * Wraps functions in promise with potential animation frame option
 * @param {Function} callback
 * @param {Object} options
 * @param {Boolean} options.raf
 * @return {Promise}
 * @private
 */
function withPromise(callback, { raf = false } = {}) {
  return new Promise((resolve, reject) => {
    if (raf) {
      requestAnimationFrame(() => {
        callback(resolve, reject);
      });
    } else {
      callback(resolve, reject);
    }
  });
}

// BEGIN Kentico
/**
 * Returns background color of element's nearest ascendant element. The color must be set explicitly, ie. transparent color is ignored.
 * @param {HTMLElement} element Mirror element.
 * @return {String} Nearest background color or white color as default.
 * @private
 */
function getNearestExplicitBackgroundColor(element) {
  if (!element) {
    return;
  }

  let currentElement = element;
  let color = "white";

  while (currentElement) {
    const currentElementColor = getComputedStyle(currentElement).getPropertyValue("background-color");

    if ((currentElementColor !== "rgba(0, 0, 0, 0)") && (currentElementColor !== "transparent")) {
      color = currentElementColor;
      break;
    }

    currentElement = currentElement.parentElement;
  }

  return color;
}
// END Kentico
