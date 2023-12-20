/**
 * Handles postMessage sending.
 * @module postMessage
 */
import { MessageTypes } from "@/builder/types";

/**
 * Sends postMessage to parent window.
 * @param messageType Type of message.
 * @param data Data to be sent to parent window.
 * @param targetOrigin Target origin of parent window.
 */
const postMessage = (messageType: MessageTypes, data: any, targetOrigin: string): void => {
    window.parent.postMessage({ msg: messageType, data }, targetOrigin);
};

export {
    postMessage,
};
