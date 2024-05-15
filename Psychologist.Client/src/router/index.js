import { createRouter, createWebHistory } from 'vue-router'
import store from '@/store'

import HomeView from '../views/HomeView.vue'
import SignInView from '../views/SignInView.vue'
import { DateTime } from "luxon";

// Lazy loading | TODO: chunks not work with Vite
const SignUpView = () => import('../views/SignUpView.vue')
const ChangePasswordView = () => import('../views/ChangePasswordView.vue')
const ScheduleView = () => import('../views/ScheduleView.vue')
const SpecialistScheduleView = () => import('../views/SpecialistScheduleView.vue')
const ScheduleDayView = () => import('../views/ScheduleDayView.vue')
const ConsultationView = () => import('../views/ConsultationView.vue')
const VisitorsView = () => import('../views/VisitorsView.vue')
const VisitorView = () => import('../views/VisitorView.vue')
const VisitorConsultationsView = () => import('../views/VisitorConsultationsView.vue')
const SpecialistsView = () => import('../views/SpecialistsView.vue')
const SpecialistView = () => import('../views/SpecialistView.vue')
const AppointmentView = () => import('../views/AppointmentView.vue')

const isNotAuthenticated = (to, from) => {
    if (store.getters.isAuth)
        return '/';
}

const isAuthenticated = (to, from) => {
    if (!store.getters.isAuth && to.name !== 'Login') {
        return { name: 'login' }
    }
}

const isAdminOrSpecialist = (to, from) => isAuthenticated(to, from) ?? (store.getters.isAdmin || store.getters.isSpecialist);
const isAdmin = (to, from) => isAuthenticated(to, from) ?? store.getters.isAdmin;
const isVisitor = (to, from) => isAuthenticated(to, from) ?? store.getters.isVisitor;
const isSpecialist = (to, from) => isAuthenticated(to, from) ?? store.getters.isSpecialist;

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
        path: '/change-password',
        name: 'changePassword',
        component: ChangePasswordView,
        beforeEnter: isAuthenticated,
        meta: { title: 'Изменение пароля' }
    },
    {
        path: '/schedule',
        name: 'schedule',
        component: ScheduleView,
        beforeEnter: isAdmin,
        meta: { title: 'Расписание' }
    },
    {
        path: '/schedule/:specialistId(\\d+)/:date(\\d{4}-\\d{2}-\\d{2})',
        name: 'scheduleDay',
        component: ScheduleDayView,
        props: route => ({
            specialistId: Number(route.params.specialistId),
            date: DateTime.fromISO(route.params.date)
        }),
        beforeEnter: isAdminOrSpecialist,
        meta: { title: 'Расписание' }
    },
    {
        path: '/schedule/:specialistId(\\d+)/:date(\\d{4}-\\d{2}-\\d{2})/:time(\\d{2}:\\d{2})',
        name: 'consultation',
        component: ConsultationView,
        props: route => ({
            specialistId: Number(route.params.specialistId),
            datetime: DateTime.fromISO(route.params.date + 'T' + route.params.time)
        }),
        beforeEnter: isAdminOrSpecialist,
        meta: { title: 'Консультация' }
    },
    {
        path: '/visitors',
        name: 'visitors',
        component: VisitorsView,
        beforeEnter: isAdmin,
        meta: { title: 'Посетители' }
    },
    {
        path: '/visitors/:id(\\d)',
        name: 'visitor',
        component: VisitorView,
        props: route => ({ id: Number(route.params.id) }),
        beforeEnter: isAdmin,
        meta: { title: 'Посетитель' },
    },
    {
        path: '/visitor-consultations',
        name: 'visitorConsultations',
        component: VisitorConsultationsView,
        beforeEnter: isVisitor,
        meta: { title: 'консультации' },
    },
    {
        path: '/specialists',
        name: 'specialists',
        component: SpecialistsView,
        beforeEnter: isAdmin,
        meta: { title: 'Специалисты' }
    },
    {
        path: '/specialists/:id(\\d)',
        name: 'specialist',
        component: SpecialistView,
        props: route => ({ id: Number(route.params.id) }),
        beforeEnter: isAdmin,
        meta: { title: 'Специалист' },
    },
    {
        path: '/specialist-schedule',
        name: 'specialistSchedule',
        component: SpecialistScheduleView,
        beforeEnter: isSpecialist,
        meta: { title: 'Записи' },
    },
    {
        path: '/appointment',
        name: 'appointment',
        component: AppointmentView,
        beforeEnter: isVisitor,
        meta: { title: 'Расписание' }
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
