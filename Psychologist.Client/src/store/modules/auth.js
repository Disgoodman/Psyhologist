import { get, post } from '@/services/fetch'
import { authorizedPost, callGet, callPost } from '@/services/api';
import TokenService from '@/services/tokenService';

export default {
    state: {
        user: null
    },
    getters: {
        isAuth: state => !!state.user,
        isAdmin: state => !!state.user?.roles?.find(r => r.toLowerCase() === 'admin'),
        isSpecialist: state => !!state.user?.roles?.find(r => r.toLowerCase() === 'employee'),
        isVisitor: state => !!state.user?.roles?.find(r => r.toLowerCase() === 'visitor'),
    },
    mutations: {
        setAuthorizedUser(state, value) {
            state.user = value;
        }
    },
    actions: {
        async init(ctx) {
            if (!TokenService.checkTokens()) {
                console.log('no auth');
                return;
            } 
            try {
                let user = await callGet('/api/me');
                console.warn('user: ', user);
                ctx.commit('setAuthorizedUser', user);
            } 
            catch (e) {
                TokenService.removeTokens();
            }
        },
        async login(ctx, { email, password }) {
            let res = await post('/api/login', {
                email: email,
                password: password
            });

            TokenService.updateTokens(res);

            let user = await callGet('/api/me');
            console.warn('user: ', user);
            ctx.commit('setAuthorizedUser', user);
        },
        async logout({ commit }) {
            console.log('logout')
            if (TokenService.checkTokens()) {
                await authorizedPost('/api/logout');
                TokenService.removeTokens();
            }
            commit('setAuthorizedUser', null);
        }
    }
}
