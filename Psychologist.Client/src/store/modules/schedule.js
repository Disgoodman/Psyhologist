import { callGet, callPost } from '@/services/api';
import { DateTime } from "luxon";
import { dateTimeOptions } from "@/utils/timeUtils.js";
import { isString } from "@/utils/commonUtils.js";

export const parseSchedule = s => ({
    ...s,
    date: DateTime.fromISO(s.date, dateTimeOptions),
    startTime: DateTime.fromISO(s.startTime, dateTimeOptions),
    endTime: DateTime.fromISO(s.endTime, dateTimeOptions),
    breakTime: DateTime.fromISO(s.breakTime, dateTimeOptions)
});

export default {
    state: {
        scheduleSpecialist: null,
        schedule: null,
    },
    getters: {
        scheduleLoaded: state => !!state.schedule,
    },
    mutations: {
        setSchedule(state, { schedule, specialist }) {
            state.schedule = schedule.map(parseSchedule);
            state.scheduleSpecialist = specialist;
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
        deleteScheduleRange(state, { startDate, endDate }) {
            if (isString(startDate)) startDate = DateTime.fromISO(startDate, dateTimeOptions);
            if (isString(endDate)) endDate = DateTime.fromISO(endDate, dateTimeOptions);
            state.schedule = state.schedule.filter(s => !(s.date >= startDate && s.date <= endDate));
        },
    },
    actions: {
        async loadSchedule(ctx, specialist) {
            let schedule = await callGet(`/api/schedule/${specialist.id}`);
            console.debug('schedule: ', schedule);
            ctx.commit('setSchedule', { schedule, specialist });
        },
    }
}
