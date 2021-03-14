import React, {ReactPropTypes} from 'react';
import {BrowserRouter, Switch } from 'react-router-dom';
import {Routes} from "./Routes";

type RoutesState = {
    oidcFinished: Boolean
}

class OIDCLogon extends React.Component<{}, RoutesState> {
  constructor(props: ReactPropTypes) {
    super(props);
    this.state = {oidcFinished: false};
  }

  componentDidMount() {
      //dummy
      this.setState({oidcFinished: true});
  }

  render() {
    return (
        <BrowserRouter>
            {this.state.oidcFinished &&
                <Routes />
            }
        </BrowserRouter>
    )
  }
}

export default OIDCLogon;
