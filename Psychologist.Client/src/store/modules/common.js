import { callGet, callPost } from '@/services/api';
import { DateTime } from "luxon";
import { dateTimeOptions } from "@/utils/timeUtils.js";

export const parseArticle = a => ({
    ...a,
    time: DateTime.fromISO(a.time, dateTimeOptions),
});

export const parseVisitor = c => ({
    ...c,
    birthday: DateTime.fromISO(c.birthday, dateTimeOptions),
});

export const parseSpecialist = c => c;

export const parseConsultation = c => ({
    ...c,
    scheduleDate: DateTime.fromISO(c.scheduleDate, dateTimeOptions),
    time: DateTime.fromISO(c.time, dateTimeOptions),
    visitor: c.visitor ? parseVisitor(c.visitor) : c.visitor,
    specialist: c.specialist ? parseSpecialist(c.specialist) : c.specialist,
    dateTime: DateTime.fromISO(c.scheduleDate + 'T' + c.time, dateTimeOptions),
});


export default {
    state: {
        articles: null,
        visitors: null,
        specialists: null
    },
    getters: {
        articlesLoaded: state => !!state.articles,
        visitorsLoaded: state => !!state.visitors,
        specialistsLoaded: state => !!state.specialists,
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

        setSpecialists(state, specialists) {
            state.specialists = specialists.map(parseSpecialist);
        },
        addSpecialist(state, specialist) {
            state.specialists?.push(parseSpecialist(specialist));
        },
        updateSpecialist(state, specialist) {
            let index = state.specialists?.findIndex(e => e.id === specialist.id);
            if (index >= 0) state.specialists.splice(index, 1, parseSpecialist(specialist));
            else if (state.specialists) state.specialists.push(parseSpecialist(specialist));
        },
        deleteSpecialist(state, specialistId) {
            let index = state.specialists?.findIndex(e => e.id === specialistId);
            if (index >= 0) state.specialists.splice(index, 1);
        },
        
    },
    actions: {
        async loadArticles(ctx) {
            let articles = await callGet('/api/articles');
            console.debug('articles: ', articles);
            ctx.commit('setArticles', articles);
        },
        async loadVisitors(ctx) {
            let visitors = await callGet('/api/visitors');
            console.debug('visitors: ', visitors);
            ctx.commit('setVisitors', visitors);
        },
        async loadSpecialists(ctx) {
            let specialists = await callGet('/api/specialists');
            console.debug('specialists: ', specialists);
            ctx.commit('setSpecialists', specialists);
        },
    }
}
