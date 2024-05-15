import { DateTime } from "luxon";

export const dateTimeOptions = { locale: 'ru-RU', zone: 'Europe/Moscow' }

export const getCurrentDateTime = (includeSeconds = false) =>
    DateTime.now().toFormat("yyyy-MM-dd'T'hh:mm" + (includeSeconds ? ":ss" : ""))
export const getCurrentDate = () =>
    DateTime.now().toFormat("yyyy-MM-dd")
export const getCurrentTime = (includeSeconds = false) =>
    DateTime.now().toFormat("hh:mm" + (includeSeconds ? ":ss" : ""))

/**
 * @param {DateTime} date
 * @param {DateTime} time
 * @return {DateTime}
 */
export const concatDateAndTime = (date, time) => {
    if (!date || !time) return null;
    const s = date.toISODate() + 'T' + time.toISOTime();
    return DateTime.fromISO(s)
} 