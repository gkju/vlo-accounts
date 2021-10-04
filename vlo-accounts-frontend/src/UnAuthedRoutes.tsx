import {Fragment, FunctionComponent, ReactPropTypes} from "react";
import {Routes, Route, Navigate} from "react-router-dom";
import {Login} from "./Pages/Login";
import {RegisterExternalLogin} from "./Pages/RegisterExternalLogin";

type RoutesProps = {

}

export const UnAuthedRoutes: FunctionComponent<RoutesProps> = (props) => {

    return (
        <Routes>
            <Route path="/Login" element={<Login />} />
            <Route path="/RegisterExternalLogin" element={<RegisterExternalLogin />} />
            <Route path="*" element={<Navigate to="/Login" />} />
        </Routes>
    )
}
