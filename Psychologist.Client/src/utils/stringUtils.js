/**
 * Delete a character at the ends of a string
 * @param {string} str
 * @param {string} char
 * @returns {string}
 */
export function trim(str, char) {
    let start = 0,
        end = str.length;

    while (start < end && str[start] === char)
        ++start;

    while (end > start && str[end - 1] === char)
        --end;

    return (start > 0 || end < str.length) ? str.substring(start, end) : str;
}