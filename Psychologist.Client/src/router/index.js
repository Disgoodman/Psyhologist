import { createRouter, createWebHistory } from 'vue-router'
import store from '@/store'

import HomeView from '../views/HomeView.vue'
import SignInView from '../views/SignInView.vue'
import { DateTime } from "luxon";

// Lazy loading | TODO: chunks not work with Vite
const SignUpView = () => import(/* webpackChunkName: "с" */ '../views/SignUpView.vue')
const ScheduleView = () => import(/* webpackChunkName: "с" */ '../views/ScheduleView.vue')
const ScheduleDayView = () => import(/* webpackChunkName: "с" */ '../views/ScheduleDayView.vue')
const ConsultationView = () => import(/* webpackChunkName: "с" */ '../views/ConsultationView.vue')
const VisitorsView = () => import(/* webpackChunkName: "с" */ '../views/VisitorsView.vue')
const VisitorView = () => import(/* webpackChunkName: "с" */ '../views/VisitorView.vue')

const isNotAuthenticated = (to, from) => {
    if (store.getters.isAuth)
        return '/';
}

const isAuthenticated = (to, from) => {
    if (!store.getters.isAuth && to.name !== 'Login') {
        return { name: 'login' }
    }
}

const isAdmin = (to, from) => isAuthenticated(to, from) ?? store.getters.isAdmin;

const routes = [
    {
        path: '/',
        name: 'home',
        component: HomeView,
        meta: { title: '' }
    },
    {
        path: '/login',
        name: 'login',
        component: SignInView,
        beforeEnter: isNotAuthenticated,
        meta: { title: 'Вход' }
    },
    {
        path: '/schedule',
        name: 'schedule',
        component: ScheduleView,
        beforeEnter: isAuthenticated,
        meta: { title: 'Расписание' }
    },
    {
        path: '/schedule/:date(\\d{4}-\\d{2}-\\d{2})',
        name: 'scheduleDay',
        component: ScheduleDayView,
        props: route => ({ date: DateTime.fromISO(route.params.date) }),
        beforeEnter: isAuthenticated,
        meta: { title: 'Расписание' }
    },
    {
        path: '/schedule/:date(\\d{4}-\\d{2}-\\d{2})/:time(\\d{2}:\\d{2})',
        name: 'consultation',
        component: ConsultationView,
        props: route => ({ datetime: DateTime.fromISO(route.params.date + 'T' + route.params.time) }),
        beforeEnter: isAuthenticated,
        meta: { title: 'Консультация' }
    },
    {
        path: '/visitors',
        name: 'visitors',
        component: VisitorsView,
        beforeEnter: isAuthenticated,
        meta: { title: 'Посетители' }
    },
    {
        path: '/visitors/:id(\\d)',
        name: 'visitor',
        component: VisitorView,
        props: route => ({ id: Number(route.params.id) }),
        beforeEnter: isAuthenticated,
        meta: { title: 'Посетитель' },
    },
    {
        path: '/register',
        name: 'register',
        component: SignUpView,
        beforeEnter: isNotAuthenticated,
        meta: { title: 'Регистрация' }
    },
    
    { path: '/:catchAll(.*)*', redirect: '/' }
]

const router = createRouter({
    history: createWebHistory(),
    routes
})

router.afterEach((to, from) => {
    if (to.meta?.title) document.title = to.meta.title;
})

export default router
