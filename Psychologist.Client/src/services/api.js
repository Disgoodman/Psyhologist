import { RequestError } from '@/exceptions';
import store from '@/store'
import TokenService from '@/services/tokenService';
import { Mutex } from 'async-mutex';
import { trim } from '@/utils/stringUtils';
import { isEmpty } from '@/utils/commonUtils'
import { postRaw as unauthorizedPost } from '@/services/fetch'

async function refreshToken() {
    if (!TokenService.checkTokens()) {
        await store.dispatch('logout');
        return false;
    }
    const tokens = TokenService.getTokens();
    
    const release = await refreshTokenMutex.acquire();
    try {
        const response = await unauthorizedPost('/api/refresh', {
            refreshToken: tokens.refreshToken 
        });

        if (!response.ok) {
            const text = await response.text();
            if (text.length === 0) {
                console.error('Refresh token request error.\n', response);
            } else {
                const json = JSON.parse(text);
                console.error('Refresh token update error.\n', response, '\n', json);
            }
            await store.dispatch('logout');
            return false;
        }

        const json = await response.json();
        TokenService.updateTokens(json);
        console.debug('Refresh token update: ', json);

        return true;
    } finally { release(); }
}

export const getAuthHeader = () => ({ 'Authorization': `Bearer ${ TokenService.getTokens().accessToken }` });
const getContentType = data => typeof data === 'object' ? 'application/json' : 'text/plain';
const getBody = data => typeof data === 'object' ? JSON.stringify(data) : String(data);
const getRequestParams = data => ({
    headers: {
        'Content-Type': getContentType(data),
        ...getAuthHeader()
    },
    body: getBody(data)
})

export async function authorizedGet(url = '', data = {}) {
    return await fetch(url + (isEmpty(data) ? '' : '?' + new URLSearchParams(data)), {
        method: 'GET',
        headers: getAuthHeader()
    });
}

export async function authorizedPost(url = '', data = {}) {
    return await fetch(url, {
        method: 'POST',
        ...getRequestParams(data)
    });
}

export async function authorizedDelete(url = '', data = {}) {
    return await fetch(url, {
        method: 'DELETE',
        ...getRequestParams(data)
    });
}

export async function authorizedPatch(url = '', data = {}) {
    return await fetch(url, {
        method: 'PATCH',
        ...getRequestParams(data)
    });
}

export async function authorizedPut(url = '', data = {}) {
    return await fetch(url, {
        method: 'PUT',
        ...getRequestParams(data)
    });
}

let methods = {
    'get': authorizedGet,
    'post': authorizedPost,
    'delete': authorizedDelete,
    'patch': authorizedPatch,
    'put': authorizedPut,
}

// To avoid multiple refresh-token calls.
const refreshTokenMutex = new Mutex();

/**
 * Call the API method and return the parsed JSON
 * @param {string} url
 * @param {string} method GET | POST | DELETE | PATCH
 * @param {object|string} data
 * @returns {Promise<object>}
 * @throws {RequestError}
 */
export async function call(url = '', method = 'GET', data = {}) {
    /*if (store.getters.jwtData.exp < Date.now() / 1000 && !refreshTokenMutex.isLocked()) {
        let res = await refreshToken();
        if (!res) throw new Error('Refresh token update error');
    }*/
    await refreshTokenMutex.waitForUnlock();

    let func = methods[method.toLowerCase()];
    if (!func) throw new Error('Unknown method');

    let response = await func(url, data);

    if (response.status === 401) {
        if (!refreshTokenMutex.isLocked()) {
            let res = await refreshToken();
            //if (!res) throw new Error('Refresh token update error');
            if (!res) throw new RequestError(response, 'Refresh token update error');
        } else await refreshTokenMutex.waitForUnlock();
        response = await func(url, data);
    }
    if (!response.ok) {
        console.error('Request execution error\n', response);
        if (response.headers.get('content-type')?.includes('application/problem+json')) {
            let json = await response.json();
            console.info('Request response: ', json)

            throw new RequestError(response, json);
        }
        throw new RequestError(response);
    }

    let text = await response.text();
    return text.length > 0 ? JSON.parse(text) : text;
}

/**
 * Call the API GET method and return the parsed JSON
 * @param {string} url
 * @param {object|string} data
 * @returns {Promise<object>}
 * @throws {RequestError}
 */
export const callGet = (url = '', data = {}) => call(url, 'get', data);

/**
 * Call the API POST method and return the parsed JSON
 * @param {string} url
 * @param {object|string} data
 * @returns {Promise<object>}
 * @throws {RequestError}
 */
export const callPost = (url = '', data = {}) => call(url, 'post', data);

/**
 * Call the API DELETE method and return the parsed JSON
 * @param {string} url
 * @param {object|string} data
 * @returns {Promise<object>}
 * @throws {RequestError}
 */
export const callDelete = (url = '', data = {}) => call(url, 'delete', data);

/**
 * Call the API PATCH method and return the parsed JSON
 * @param {string} url
 * @param {object|string} data
 * @returns {Promise<object>}
 * @throws {RequestError}
 */
export const callPatch = (url = '', data = {}) => call(url, 'patch', data);

/**
 * Call the API PUT method and return the parsed JSON
 * @param {string} url
 * @param {object|string} data
 * @returns {Promise<object>}
 * @throws {RequestError}
 */
export const callPut = (url = '', data = {}) => call(url, 'put', data);

function extractFilename(contentDisposition, defaultFilename) {
    if (!contentDisposition) return defaultFilename;

    contentDisposition = contentDisposition.split(';').map(s => s.trim().split('='));
    let filenameUtf8 = contentDisposition.find(e => e[0] === "filename*");
    if (filenameUtf8) {
        filenameUtf8 = trim(filenameUtf8[1], '"');
        return decodeURI(filenameUtf8.startsWith("UTF-8''") ? filenameUtf8.substring(7) : filenameUtf8);
    }
    let filename = contentDisposition.find(e => e[0] === "filename");
    if (filename) {
        return trim(filename[1], '"');
    }
    return defaultFilename;
}

/**
 * Call the API GET method and download received file
 * @param {string} url
 * @param {object|string} data
 * @param {string|null} defaultFileName
 * @returns {Promise<object>}
 * @throws {RequestError}
 */
export async function downloadFile(url, data = {}, defaultFileName = null) {
    /*if (store.getters.jwtData.exp < Date.now() / 1000 && !refreshTokenMutex.isLocked()) {
        let res = await refreshToken();
        if (!res) throw new Error('Refresh token update error');
    }*/
    await refreshTokenMutex.waitForUnlock();

    const fetchFile = () => fetch(url + (isEmpty(data) ? '' : '?' + new URLSearchParams(data)), {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${ store.state.auth.jwt }`,
        }
    });
    let response = await fetchFile();

    if (response.status === 401) {
        if (!refreshTokenMutex.isLocked()) {
            let res = await refreshToken();
            //if (!res) throw new Error('Refresh token update error');
            if (!res) throw new RequestError(response, 'Refresh token update error');
        } else await refreshTokenMutex.waitForUnlock();
        response = await fetchFile();
    }
    if (!response.ok) {
        console.error('Request execution error\n', response);
        if (response.headers.get('content-type')?.includes('application/problem+json')) {
            let json = await response.json();
            console.info('Request response: ', json)

            throw new RequestError(response, json);
        }
        throw new RequestError(response);
    }

    let blob = await response.blob();
    let objectUrl = window.URL.createObjectURL(blob);

    let filename = extractFilename(response.headers.get('Content-Disposition'), defaultFileName);

    let anchor = document.createElement("a");
    document.body.appendChild(anchor);
    anchor.href = objectUrl;
    anchor.download = filename;
    anchor.click();

    window.URL.revokeObjectURL(objectUrl);
}
