import vlobg from "./vlobg.png";
import styled from "styled-components";

export const Layout = styled.div`
  display: grid;
  grid-template-columns: 500px 1fr;
`

export const Container = styled.div`
  background: #1D1D28;
  display: grid;
  grid-template-rows: 30vh 50vh 20vh;
  width: 100%;
`

export const Bg = styled.div`
  width: 100%;
  background-image: Url("${vlobg}");
  background-position-y: top;
  background-position-x: center;
  background-size: cover;
`

export const InputWrapper = styled.div`
  display: flex;
  justify-content: center;
  align-content: center;
  margin: 45px 0;
`

export const ErrorSpan = styled.div`
  font-family: Raleway, serif;
  position: absolute;
  width: 100%;
  font-style: normal;
  font-weight: bold;
  color: #f44336;
  text-align: center;
  margin-top: 15px;
`
