import { RequestError } from "@/exceptions";

export const sleep = ms => new Promise(resolve => setTimeout(resolve, ms));

export const isString = s => s?.constructor === String;

export const getVisitorFullname = visitor => !visitor ? null : visitor.firstName + ' ' + visitor.lastName + (visitor.patronymic ? ' ' + visitor.patronymic : '');

export const visitorTypes = [
    { name: 'student', title: 'Студент' },
    { name: 'parent', title: 'Родитель' },
    { name: 'specialist', title: 'Специалист' },
]
export const getVisitorTypeTitleByName = name => visitorTypes.find(t => t.name === name)?.title;

export const getVisitorLabel = c => `${getVisitorFullname(c)} (${getVisitorTypeTitleByName(c.type)}, ${c.birthday.toFormat('dd.MM.yyyy')})`;

export const consultationTypes = [
    { name: 'individualConsultation', title: 'Индивидуальная консультация' },
    { name: 'individualWork', title: 'Индивидуальная коррекционная/развивающая работа' },
    { name: 'diagnosticWork', title: 'Диагностическая работа' },
]

export const getConsultationTypeTitleByName = name => consultationTypes.find(t => t.name === name)?.title;

export const consultationTypeEndpoints = [
    { name: 'individualConsultation', endpoint: 'individual-consultation' },
    { name: 'individualWork', endpoint: 'individual-work' },
    { name: 'diagnosticWork', endpoint: 'diagnostic-work' },
]

export const getConsultationTypeEndpoint = name => consultationTypeEndpoints.find(t => t.name === name)?.endpoint;

export const requestCodes = [
    { name: 'О', title: 'Проблемы, связанные с обучением' },
    { name: 'В', title: 'Проблемы, связанные с воспитанием' },
    { name: 'П', title: 'Проблемы, связанные с поведением' },
    { name: 'Э', title: 'Эмоциональные проблемы' },
    { name: 'О', title: 'Определение уровняя развития' },
    { name: 'Р', title: 'Проблемы, связанные с развитием речи' },
    { name: 'ОМ', title: 'Определение маршрута обучения' },
    { name: 'Пф', title: 'Профориентация' },
]

/**
 *
 * @param {object} obj
 * @param {function} fn (value, key, index) => new value
 * @returns {object}
 */
export const objectMap = (obj, fn) =>
    Object.fromEntries(
        Object.entries(obj).map(
            ([ k, v ], i) => [ k, fn(v, k, i) ]
        )
    )

/**
 * Check if the object is empty
 * @param {object} obj
 * @returns {boolean}
 */
export function isEmpty(obj) {
    for (let i in obj) return false;
    return true;
}

/**
 * Check if the object is not null and empty
 * @param {object} obj
 * @returns {boolean}
 */
export function isNotEmpty(obj) {
    if (!(obj instanceof Object)) return false;
    for (let i in obj) return true;
    return false;
}


/**
 * Convert RFC7807 to text representation of the error
 * @param {RequestError} err
 * @returns {string}
 */
export function errorToText(err) {
    if (!(err instanceof RequestError)) return 'Неизвестная ошибка';
    if (!err.rfc7807) return 'Ошибка запроса';
    let errors = err.response.errors;
    console.log(errors);
    if (errors === undefined || isEmpty(errors)) return err.title;
    let result = err.title;
    for (let prop in errors) {
        console.log(errors[prop]);
        result += '\n';
        result += errors[prop].join("\n");
    }
    return result;
}
