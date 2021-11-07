import {FunctionComponent, useState} from "react";
import styled from "styled-components";
import {InputSize, inputWrapperProps, textInputProps} from "../Constants";
import {useField} from "formik";
import { motion } from "framer-motion";

export const TextInput: FunctionComponent<textInputProps> = (props: textInputProps ) => {
    const [focused, setFocus] = useState(false);
    const [show, setShow] = useState(!!props.password ? false :true);

    const inputType = !show ? "password" : "text";

    const [field, meta, helpers] = useField({name: props.name, type: inputType});

    const handleUp = (e: any) => {
        if(!!props.password) {
            if(e.key === "Alt") {
                e.preventDefault();
                setShow(false);
            }
        }
    }

    const handleDown = (e: any) => {
        if(!!props.password) {
            if(e.altKey) {
                e.preventDefault();
                setShow(true);
            }
        }
    }

    const handleBlur = (e: any) => {
        setFocus(false);
        helpers.setTouched(true);
        field.onBlur(e);
        field.onChange(e);
        setShow(false);
    }

    const showFeedback = (focused && field.value.trim().length > 3) || meta.touched;

    return (
        <Wrapper>
        <InputWrapper size={props.size}>
            <NeumorphTextInput {...field} error={showFeedback && !!meta.error} onKeyDown={handleDown} onKeyUp={handleUp} hasValue={!!field.value} size={props.size} onFocus={() => setFocus(true)} onBlur={handleBlur} type={inputType} />
            <Label size={props.size} focused={(Boolean) (focused || field.value)}>
                <InnerLabel>
                    {props.placeholder}
                </InnerLabel>
            </Label>
        </InputWrapper>
            <ErrorSpan>
                <motion.span animate={{opacity: showFeedback && !!meta.error ? 1 : 0}}>{meta.error}</motion.span>
            </ErrorSpan>
        </Wrapper>
    )
}

const InputWrapper = styled("div")<inputWrapperProps>`
  background: #1D1D28;
  box-shadow: inset 0 0 30px 5px rgba(0, 0, 0, 0.5);
  border-radius: 30px;
  display: flex;
  justify-content: center;
  position: relative;
  width: ${props => props?.size === InputSize.Big ? 580 : props?.size === InputSize.Medium ? 435 : 290}px;
  height: ${props => props?.size === InputSize.Big ? 120 : props?.size === InputSize.Medium ? 90 : 60}px;
`

const Label = styled("div")<{focused?: boolean, size?: InputSize}>`
  position: absolute;
  font-family: Raleway, serif;
  font-style: normal;
  font-weight: bold;
  text-align: center;
  color: ${props => !props.focused ? "rgba(161,161,161,0.4)" : "rgba(0,0,0,0)"};
  transition: all 0.2s linear;
  width: 100%;
  height: 100%;
  display: flex;
  justify-content: center;
  align-content: center;
  font-size: ${props => props?.size === InputSize.Big ? 90 : props?.size === InputSize.Medium ? 40 : 25}px;
  pointer-events: none;
`

const InnerLabel = styled.span`
    margin: auto;
`

const NeumorphTextInput = styled.input<{hasValue: Boolean, size?: InputSize, error?: Boolean}>`
  font-family: Raleway, serif;
  font-style: normal;
  font-weight: bold;
  background: transparent;
  border: none;
  width: 100%;
  font-size: ${props => props?.size === InputSize.Big ? 90 : props?.size === InputSize.Medium ? 40 : 25}px;
  padding: 10px 20px;
  color: ${props => props.hasValue ? "rgba(180,180,180,0.6)" : "rgba(161,161,161,0.4)"};
  transition: all 0.2s linear;
  text-align: center;
  border-radius: 30px;
  box-shadow: ${props => props?.error ? "0px 0px 5px 5px #f44336" : ""};
  &:focus {
    outline: none;
    color: white;
    box-shadow: ${props => !props?.error && "0px 0px 5px 5px #6D5DD3"};
  }
`

const Wrapper = styled.div`
  position: relative;
`

const ErrorSpan = styled.div`
  font-family: Raleway, serif;
  position: absolute;
  width: 100%;
  font-style: normal;
  font-weight: bold;
  color: #f44336;
  text-align: center;
  margin-top: 15px;
`
