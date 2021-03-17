import {Fragment, FunctionComponent, ReactPropTypes} from "react";
import {Route} from "react-router-dom";
import {Login} from "./Pages/Login";
import {RegisterExternalLogin} from "./Pages/RegisterExternalLogin";

type RoutesProps = {

}

export const UnAuthedRoutes: FunctionComponent<RoutesProps> = (props) => {

    return (
        <Fragment>
            <Route path="/Login" component={Login} />
            <Route path="/RegisterExternalLogin" component={RegisterExternalLogin} />
        </Fragment>
    )
}
