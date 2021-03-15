import React from 'react';
import ReactDOM from 'react-dom';
import OIDCLogon from './OIDCLogon';
import reportWebVitals from './reportWebVitals';
import "./index.css";
import { Provider } from 'react-redux';
import Store from "./Redux/Store/Store";

ReactDOM.render(
  <React.StrictMode>
      <Provider store={Store}>
        <OIDCLogon />
      </Provider>
  </React.StrictMode>,
  document.getElementById('root')
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
