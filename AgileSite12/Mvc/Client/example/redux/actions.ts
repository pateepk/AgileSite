export enum ActionTypeKeys {
    INCREMENT = 'INCREMENT',
    DECREMENT = 'DECREMENT',
    OTHER_ACTION = "__any_other_action_type__"
}

export interface IncrementAction {
    readonly type: ActionTypeKeys.INCREMENT;
    by: number;
}

export interface DecrementAction {
    readonly type: ActionTypeKeys.DECREMENT;
    by: number;
}

export interface OtherAction {
    readonly type: ActionTypeKeys.OTHER_ACTION;
}

export const incrementCounter = (by: number): IncrementAction => ({
    type: ActionTypeKeys.INCREMENT,
    by
});

export type ActionTypes =
    | IncrementAction
    | DecrementAction
    | OtherAction;