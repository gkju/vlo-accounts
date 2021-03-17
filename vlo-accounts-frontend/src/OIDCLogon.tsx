import React, {PropsWithChildren, Fragment, FunctionComponent, useState} from "react";
import {BrowserRouter, Switch, Route, Redirect } from "react-router-dom";
import {UnAuthedRoutes} from "./UnAuthedRoutes";
import {AuthedRoutes} from "./AuthedRoutes";
import {GoogleReCaptchaProvider} from "react-google-recaptcha-v3";
import {authoritySettings, CaptchaConfig} from "./Config";
import {useMount, useUnmount} from "react-use";
import {selectLoggedIn} from "./Redux/Slices/Auth";
import {useSelector} from "react-redux";
import {UserManager} from "oidc-client";
import authService from "./Auth/AuthService";

type RoutesState = {
    oidcFinished: Boolean,
    user?: object,
    loggedIn: boolean
}

type OIDCLogonProps = {
}

export const OIDCLogon: FunctionComponent<OIDCLogonProps> = (props) => {
    let loggedIn = useSelector(selectLoggedIn);
    const [ready, setReady] = useState(false);

    useMount(async () => {
        if(!loggedIn) {
            await authService.signInSilent();
            setReady(true);
        } else {
            setReady(true);
        }
    })

    return (
        <GoogleReCaptchaProvider reCaptchaKey={String(CaptchaConfig.CaptchaKey)}>
            {ready ? (<Fragment>
                {!loggedIn && <UnAuthedRoutes />}
                <AuthedRoutes />
                <Route path="/">
                    {!loggedIn && <Redirect to="/Login" />}
                    {loggedIn && <Redirect to="/AccountManagement" />}
                </Route>
            </Fragment>) : <div>not ready</div>}
        </GoogleReCaptchaProvider>
    )
}

export default OIDCLogon;
