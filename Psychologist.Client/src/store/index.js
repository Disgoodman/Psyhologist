import { createStore } from 'vuex'
import auth from './modules/auth'
import common from './modules/common'

const store = createStore({
    strict: true,
    state: {},
    mutations: {},
    modules: {
        auth,
        common
    },
});

await store.dispatch('init');

export default store;
