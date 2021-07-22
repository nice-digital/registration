import { fetchWrapper } from '../lib/fetch-wrapper';

export const registrationService = {
    getRegistrations,
    createRegistration    
};

const baseUrl = `${process.env.REGISTRATION_API}/AddRegistration`;


function getRegistrations() {
    return fetchWrapper.get(baseUrl);
}

function createRegistration(params:any) {
    return fetchWrapper.post(baseUrl, params);
}

export {}