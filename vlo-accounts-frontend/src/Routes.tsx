import {Fragment, FunctionComponent, ReactPropTypes} from "react";
import {Route} from "react-router-dom";
import {Login} from "./Pages/Login";

type RoutesProps = {

}

export const Routes: FunctionComponent<RoutesProps> = (props) => {

    return (
        <Fragment>
            <Route path="/login" component={Login} />
        </Fragment>
    )
}
