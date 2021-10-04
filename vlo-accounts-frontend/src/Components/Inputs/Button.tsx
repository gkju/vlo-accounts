import {FunctionComponent} from "react";
import {ButtonProps, InputSize} from "../Constants";
import styled from "styled-components";
import {RippleAble} from "./RippleAble";

export const Button: FunctionComponent<ButtonProps> = (props) => {

    return (
        <RippleAble style={{borderRadius: "20px"}}>
            <StyledButton {...props} type={props.type}>
                <TextWrapper>
                    {props.children}
                </TextWrapper>
            </StyledButton>
        </RippleAble>
    )
}

const StyledButton = styled("button")<ButtonProps>`
  outline: none;
  border: none;
  border-radius: 20px;
  background: ${props => !!props.primaryColor ? props.primaryColor : "#6D5DD3"};
  color: ${props => !!props.secondaryColor ? props.secondaryColor : "#FFFFFF"};
  width:  ${props => props?.size === InputSize.Big ? 600 : props?.size === InputSize.Medium ? 435 : 200}px;
  font-family: Raleway, serif;
  font-style: normal;
  font-weight: bold;
  cursor: pointer;
  font-size: ${props => props?.size === InputSize.Big ? 40 : props?.size === InputSize.Medium ? 36 : 20}px;
  padding: 30px 15px;
  position: relative;
  overflow: hidden;
  transition: all 200ms ease;
  -webkit-tap-highlight-color: transparent;

  &:hover {
    outline: none;
    border: none;
    filter: brightness(90%);
  }

  &:active {
    outline: none;
    border: none;
  }
`

const TextWrapper = styled.span`
  z-index: 1;
`
