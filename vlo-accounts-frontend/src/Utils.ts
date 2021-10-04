import qs from "qs";

export const GetReturnUrl = (search: string) => {
    const queryParams = qs.parse(window.location.search.substr(1));
    return String(queryParams?.returnUrl);
};

export const EmptyObj = (obj: any)  => {
    return obj && Object.keys(obj).length === 0
        && Object.getPrototypeOf(obj) === Object.prototype;
}