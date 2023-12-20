import { ActionTypeKeys, ActionTypes } from './actions';

export type State = {
    readonly counter: number;
};

export function counterReducer(s: State, action: ActionTypes) {
    switch (action.type) {
        case ActionTypeKeys.INCREMENT:
            return { counter: s.counter + action.by };
        case ActionTypeKeys.DECREMENT:
            return { counter: s.counter - action.by };
        default:
            return s;
    }
}