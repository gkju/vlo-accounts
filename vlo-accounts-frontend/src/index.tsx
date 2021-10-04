import React from 'react';
import ReactDOM from 'react-dom';
import OIDCLogon from './OIDCLogon';
import reportWebVitals from './reportWebVitals';
import "./index.css";
import {BrowserRouter, Routes, Route} from "react-router-dom";
import {LoginCallback} from "./Auth/LoginCallback";
import Store from "./Redux/Store/Store";
import {Provider} from "react-redux";
import {CaptchaConfig} from "./Config";
import {GoogleReCaptchaProvider} from "react-google-recaptcha-v3";

ReactDOM.render(
    <React.StrictMode>
        <Provider store={Store}>
            <GoogleReCaptchaProvider reCaptchaKey={String(CaptchaConfig.CaptchaKey)}>
                <BrowserRouter>
                    <Routes>
                        <Route path="/login-callback" element={<LoginCallback />} />
                    </Routes>
                    {window === window.parent &&
                        <OIDCLogon/>
                    }
                </BrowserRouter>
            </GoogleReCaptchaProvider>
        </Provider>
    </React.StrictMode>,
  document.getElementById('root')
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
