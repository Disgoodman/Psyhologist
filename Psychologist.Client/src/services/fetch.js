import { RequestError } from '@/exceptions';

/**
 * Unauthorized POST request to API
 * @param {string} url
 * @param {object} data
 * @returns {Promise<Response>}
 */
export async function postRaw(url = '', data = {}) {
    let response =  await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    });
    if (!response.ok) {
        console.log('Request execution error\n', response);
        if (response.headers.get('content-type')?.includes('application/problem+json'))
            throw new RequestError(response, await response.json());
        throw new RequestError(response);
    }
    return response;
}

/**
 * Unauthorized POST request to API with parsing response
 * @param {string} url
 * @param {object} data
 * @returns {Promise<object>}
 */
export async function post(url = '', data = {}) {
    const response = await postRaw(url, data);
    return await response.json();
}

/**
 * Unauthorized GET request to API
 * @param {string} url
 * @param {object} data query params
 * @returns {Promise<Response>}
 */
export async function getRaw(url = '', data = {}) {
    return await fetch(url + '?' + new URLSearchParams(data), { method: 'GET' });
}

/**
 * Unauthorized GET request to API with parsing response
 * @param {string} url
 * @param {object} data query params
 * @returns {Promise<Response>}
 */
export async function get(url = '', data = {}) { 
    return await (await getRaw(url, data)).json();
}
