import { isString } from "@/utils/commonUtils.js";

export class RequestError extends Error {
    constructor(response, jsonOrMessage) {
        if (isString(jsonOrMessage)) {
            super(jsonOrMessage);
            this.rfc7807 = false;
        } else {
            super(jsonOrMessage?.title);
            this.rfc7807 = !!jsonOrMessage;
            this.response = jsonOrMessage;
        }
        this.name = 'RequestError';
        
        this.title = jsonOrMessage?.title;
        this.status = response.status;
    }
}
