import {FunctionComponent, ReactPropTypes} from "react";
import styled from "styled-components";

export const Logo: FunctionComponent = (props) => {
    return (
        <LogoSection>
            <BigLogoPart>VLO</BigLogoPart>
            <SubLogoPart>Accounts</SubLogoPart>
        </LogoSection>
    )
}


const LogoSection = styled.div`
  width: 100%;
  display: grid;
  grid-template-rows: 10fr 10fr;
`

const BigLogoPart = styled.div`
  font-family: Raleway, serif;
  font-style: normal;
  font-weight: Bold;
  font-size: 230px;
  text-align: center;
  color: white;
  width: 100%;
  letter-spacing: -0.1em;
`

const SubLogoPart = styled.div`
  font-family: Raleway, serif;
  font-style: normal;
  font-weight: 300;
  font-size: 48px;
  color: white;
  text-align: center;
  width: 100%;
  margin-top: -45px;
`
