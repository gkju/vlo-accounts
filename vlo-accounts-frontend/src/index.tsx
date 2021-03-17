import React from 'react';
import ReactDOM from 'react-dom';
import OIDCLogon from './OIDCLogon';
import reportWebVitals from './reportWebVitals';
import "./index.css";
import {BrowserRouter, Switch, Route} from "react-router-dom";
import {LoginCallback} from "./Auth/LoginCallback";
import Store from "./Redux/Store/Store";
import {Provider} from "react-redux";

ReactDOM.render(
    <React.StrictMode>
        <Provider store={Store}>
        <BrowserRouter>
            <Switch>
                <Route path="/login-callback" component={LoginCallback} />
                <Route path="/">
                    <OIDCLogon />
                </Route>
            </Switch>
        </BrowserRouter>
        </Provider>
    </React.StrictMode>,
  document.getElementById('root')
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
