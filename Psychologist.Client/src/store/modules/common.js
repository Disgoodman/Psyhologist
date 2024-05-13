import { callGet, callPost } from '@/services/api';
import { DateTime } from "luxon";
import { dateTimeOptions } from "@/utils/timeUtils.js";
import { isString } from "@/utils/commonUtils.js";

export const parseArticle = a => ({
    ...a,
    time: DateTime.fromISO(a.time, dateTimeOptions),
});

export const parseSchedule = s => ({
    ...s,
    date: DateTime.fromISO(s.date, dateTimeOptions),
    startTime: DateTime.fromISO(s.startTime, dateTimeOptions),
    endTime: DateTime.fromISO(s.endTime, dateTimeOptions),
    breakTime: DateTime.fromISO(s.breakTime, dateTimeOptions)
});

export const parseVisitor = c => ({
    ...c,
    birthday: DateTime.fromISO(c.birthday, dateTimeOptions),
});

export const parseConsultation = c => ({
    ...c,
    scheduleDate: DateTime.fromISO(c.scheduleDate, dateTimeOptions),
    time: DateTime.fromISO(c.time, dateTimeOptions),
    visitor: c.visitor ? parseVisitor(c.visitor) : c.visitor
});


export default {
    state: {
        articles: null,
        schedule: null,
        visitors: null
    },
    getters: {
        articlesLoaded: state => !!state.articles,
        scheduleLoaded: state => !!state.schedule,
        visitorsLoaded: state => !!state.visitors,
    },
    mutations: {
        setArticles(state, articles) {
            state.articles = articles.map(parseArticle);
        },
        addArticle(state, article) {
            state.articles.push(parseArticle(article));
        },
        updateArticle(state, article) {
            let index = state.articles.findIndex(e => e.id === article.id);
            if (index >= 0) state.articles.splice(index, 1, parseArticle(article));
            else state.articles.push(parseArticle(article));
        },
        deleteArticle(state, articleId) {
            let index = state.articles.findIndex(e => e.id === articleId);
            if (index >= 0) state.articles.splice(index, 1);
        },
        
        setSchedule(state, schedule) {
            state.schedule = schedule.map(parseSchedule);
        },
        addSchedule(state, schedule) {
            if (Array.isArray(schedule)) {
                state.schedule.push(...schedule.map(parseSchedule));
            } else {
                state.schedule.push(parseSchedule(schedule));
            }
        },
        updateSchedule(state, schedule) {
            schedule = parseSchedule(schedule);
            let index = state.schedule.findIndex(e => +e.date === +schedule.date);
            if (index >= 0) state.schedule.splice(index, 1, schedule);
            else state.schedule.push(schedule);
        },
        deleteSchedule(state, scheduleDate) {
            if (isString(scheduleDate)) scheduleDate = DateTime.fromISO(scheduleDate, dateTimeOptions);
            let index = state.schedule.findIndex(e => +e.date === +scheduleDate);
            if (index >= 0) state.schedule.splice(index, 1);
        },
        deleteScheduleRange(state, {startDate, endDate}) {
            if (isString(startDate)) startDate = DateTime.fromISO(startDate, dateTimeOptions);
            if (isString(endDate)) endDate = DateTime.fromISO(endDate, dateTimeOptions);
            state.schedule = state.schedule.filter(s => !(s.date >= startDate && s.date <= endDate));
        },

        setVisitors(state, visitors) {
            state.visitors = visitors.map(parseVisitor);
        },
        addVisitor(state, visitor) {
            state.visitors?.push(parseVisitor(visitor));
        },
        updateVisitor(state, visitor) {
            let index = state.visitors?.findIndex(e => e.id === visitor.id);
            if (index >= 0) state.visitors.splice(index, 1, parseVisitor(visitor));
            else if (state.visitors) state.visitors.push(parseVisitor(visitor));
        },
        deleteVisitor(state, visitorId) {
            let index = state.visitors?.findIndex(e => e.id === visitorId);
            if (index >= 0) state.visitors.splice(index, 1);
        },
    },
    actions: {
        async loadArticles(ctx) {
            let articles = await callGet('/api/articles');
            console.debug('articles: ', articles);
            ctx.commit('setArticles', articles);
        },
        async loadSchedule(ctx) {
            let schedule = await callGet('/api/schedule');
            console.debug('schedule: ', schedule);
            ctx.commit('setSchedule', schedule);
        },
        async loadVisitors(ctx) {
            let visitors = await callGet('/api/visitors');
            console.debug('visitors: ', visitors);
            ctx.commit('setVisitors', visitors);
        },
    }
}
