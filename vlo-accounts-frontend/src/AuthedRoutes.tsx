import {Fragment, FunctionComponent, ReactPropTypes} from "react";
import {Route} from "react-router-dom";
import authService from "./Auth/AuthService";

export const AuthedRoutes: FunctionComponent = (props) => {

    return (
        <Fragment>
            <Route path="/AccountManagement" component={comp} />
        </Fragment>
    )
}

const comp: FunctionComponent = () => {
    authService.GetToken().then(console.log);
    return <div>hello</div>
}
