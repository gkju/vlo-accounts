import {Fragment, FunctionComponent, ReactPropTypes} from "react";
import {Routes, Route, Navigate} from "react-router-dom";
import authService from "./Auth/AuthService";

export const AuthedRoutes: FunctionComponent = (props) => {

    return (
        <Routes>
            <Route path="/AccountManagement" element={<Comp />} />
            <Route path="*" element={<Navigate to="/AccountManagement" />} />
        </Routes>
    )
}

const Comp: FunctionComponent = () => {
    return <div>hello</div>
}
