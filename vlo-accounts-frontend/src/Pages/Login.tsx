import {FunctionComponent, ReactPropTypes} from "react";
import styled from "styled-components";
import vlobg from "./vlobg.png";

export const Login: FunctionComponent<ReactPropTypes> = (props: ReactPropTypes) => {
    return (
        <Layout>
            <Container>

            </Container>
            <Bg/>
        </Layout>
    )
}

const Layout = styled.div`
  display: grid;
  grid-template-columns: 500px 1fr;
`

const Container = styled.div`
  background: #1D1D28;
  display: grid;
  grid-template-rows: 30vh 40vh 30vh;
  width: 100%;
`

const Bg = styled.div`
  width: 100%;
  background-image: Url("${vlobg}");
  background-position-y: top;
  background-position-x: center;
  background-size: cover;
`
