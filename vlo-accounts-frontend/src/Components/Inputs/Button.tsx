import {FunctionComponent, useState} from "react";
import {ButtonProps, InputSize} from "./Constants";
import {AnimatePresence, motion} from "framer-motion";
import styled, {keyframes} from "styled-components";

type Ripple = {
    key: number,
    styles: any
}

export const Button: FunctionComponent<ButtonProps> = (props) => {
    let arr: Ripple[] = [];
    const [ripples, setRipples] = useState(arr);
    const [lastKey, setLastKey] = useState(0);

    const handleDown = (e: any) => {
        const target = e.currentTarget;
        const maxDim = Math.max(target.clientWidth, target.clientHeight);
        const styles: any = {};
        styles.left = `${e.clientX - target.offsetLeft - maxDim/2}px`;
        styles.top = `${e.clientY - target.offsetTop - maxDim/2}px`;
        styles.width = styles.height = maxDim;
        const key = lastKey + 1;
        setLastKey(key + 1);
        setRipples([...(ripples ?? []), {key, styles} as Ripple]);
    }

    const handleUpOrLeave = (e: any) => {
        let ripples2: Ripple[] = [...ripples];
        if(ripples2.length > 0) {
            ripples2.shift();
        }
        setRipples(ripples2);
    }

    return (
        <StyledButton onMouseDown={handleDown} onMouseUp={handleUpOrLeave} onMouseLeave={handleUpOrLeave} {...props} type={props.type}>
            <TextWrapper>
            {props.children}
            </TextWrapper>
            <AnimatePresence>
                {ripples.map((ripple) => (
                    <motion.div key={ripple.key} initial={{opacity: 1}} exit={{opacity: 0}}>
                        <RippleCircle style={ripple.styles} />
                    </motion.div>
                ))}
            </AnimatePresence>

        </StyledButton>
    )
}

const StyledButton = styled("button")<ButtonProps>`
  outline: none;
  border: none;
  border-radius: 20px;
  background: ${props => !!props.primaryColor ? props.primaryColor : "#6D5DD3"};
  color: ${props => !!props.secondaryColor ? props.secondaryColor : "#FFFFFF"};
  width:  ${props => props?.size == InputSize.Big ? 600 : props?.size == InputSize.Medium ? 435 : 200}px;
  font-family: Raleway, serif;
  font-style: normal;
  font-weight: bold;
  cursor: pointer;
  font-size: ${props => props?.size == InputSize.Big ? 40 : props?.size == InputSize.Medium ? 36 : 20}px;
  padding: 30px 15px;
  position: relative;
  overflow: hidden;
  transition: all 200ms ease;
  
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

const rippleKeyframes = keyframes`
  to {
    transform: scale(4);
  }
`

const RippleCircle = styled.span`
  position: absolute;
  border-radius: 50%;
  transform: scale(0);
  animation: ${rippleKeyframes} 700ms ease-in-out forwards;
  background-color: rgba(255, 255, 255, 0.4);
`

const TextWrapper = styled.span`
  z-index: 1;
`
