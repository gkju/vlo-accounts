import axios from "axios";

export const NavigateToReturnUrl = async (returnUrl: string) => {
    try {
        let res = await axios.post("/auth/returnUrlInfo", {returnUrl});
        if(Boolean(res.data?.validReturnUrl)) {
            window.location.href = returnUrl;
        } else {
            window.location.href = "/";
        }
    } catch (response) {
        window.location.href = "/";
    }

}
