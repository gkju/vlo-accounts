import {FunctionComponent} from "react";
import authService from "./AuthService";
import {useMount} from "react-use";
import {useSelector} from "react-redux";
import {selectLastKnownUrl} from "../Redux/Slices/Auth";

export const LoginCallback: FunctionComponent = (props) => {

    useMount(async () => {
        const res = await authService.processSignInUrl(window.location.href);
        if(res) {
            window.location.replace("/");
        }
    })

    return (
        <div>processing login</div>
    )
}
