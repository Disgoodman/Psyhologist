import { createStore } from 'vuex'
import auth from './modules/auth'
import common from './modules/common'
import schedule from './modules/schedule'

const store = createStore({
    strict: true,
    state: {},
    mutations: {},
    modules: {
        auth,
        common,
        schedule
    },
});

await store.dispatch('init');

export default store;
