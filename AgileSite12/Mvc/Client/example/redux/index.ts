import { createStore } from 'redux';
import { counterReducer } from './reducers';

export default function configureStore() {
    return createStore(counterReducer);
}