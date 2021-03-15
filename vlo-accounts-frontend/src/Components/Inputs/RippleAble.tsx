import {FunctionComponent, useState} from "react";
import {ButtonProps, InputSize, RippleAbleProps} from "../Constants";
import {AnimatePresence, motion} from "framer-motion";
import styled, {keyframes} from "styled-components";

type Ripple = {
    key: number,
    styles: any
}


//finally rippleable component that works :DDDDD, took 1h
export const RippleAble: FunctionComponent<RippleAbleProps> = (props) => {
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
        <Wrapper onMouseDown={handleDown} onMouseUp={handleUpOrLeave} onMouseLeave={handleUpOrLeave}>
            {props.children}
            <WrapperInner style={props.style}>
            <AnimatePresence>
                {ripples.map((ripple) => (
                    <motion.div key={ripple.key} initial={{opacity: 1}} exit={{opacity: 0}}>
                        <RippleCircle style={ripple.styles} />
                    </motion.div>
                ))}
            </AnimatePresence>
            </WrapperInner>
        </Wrapper>
    )
}

const Wrapper = styled.div`
  position: relative;
  display: block;
`

const WrapperInner = styled.div`
  position: absolute;
  overflow: hidden;
  width: 100%;
  height: 100%;
  transform: translateY(-100%);
  pointer-events: none;
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
