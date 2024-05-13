import { DateTime } from "luxon";

export const dateTimeOptions = { locale: 'ru-RU', zone: 'Europe/Moscow' }

export const getCurrentDateTime = (includeSeconds = false) =>
    DateTime.now().toFormat("yyyy-MM-dd'T'hh:mm" + (includeSeconds ? ":ss" : ""))
export const getCurrentDate = () =>
    DateTime.now().toFormat("yyyy-MM-dd")
export const getCurrentTime = (includeSeconds = false) =>
    DateTime.now().toFormat("hh:mm" + (includeSeconds ? ":ss" : ""))