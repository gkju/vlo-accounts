import {FunctionComponent, useState, Fragment} from "react";
import {ModalProps} from "./Constants";
import {AnimatePresence, motion} from "framer-motion";
import styled from "styled-components";
import ReactDOM from "react-dom";

export const Modal: FunctionComponent<ModalProps> = (props) => {
    const open = props.open;

    if(open) {
        let html = document.querySelector("html");

        if(html !== null) {
            html.classList.add("locked");
        }
    }

    const shouldUnlockScroll = () => {
        const modals = document.querySelectorAll(".Modal");
        let html = document.querySelector("html");

        if(modals.length == 0 && html !== null) {
            html.classList.remove("locked");
        }
    }

    const backDropVariants = {
        exit: {opacity: 0},
        open: {opacity: 1},
        initial: {opacity: 0}
    }

    const modalVariants = {
        exit: {top: "150%", transition: {type: "string"}},
        open: {top: "50%"},
        initial: {top: "-50%", transition: {type: "string", velocity: 2}}
    }


    // @ts-ignore
    const domNode: Element = document.getElementById("modalRoot");

    return ReactDOM.createPortal(
        <AnimatePresence onExitComplete={shouldUnlockScroll}>
            {open && (
                <Fragment>
                    <ModalDiv variants={modalVariants} initial="initial" exit="exit" animate="open" key="modal" className="Modal">{props.children}</ModalDiv>
                    <Backdrop onMouseDown={props.close} variants={backDropVariants} initial="initial" exit="exit" animate="open" key="backdrop"/>
                </Fragment>
            )}
        </AnimatePresence>
    , domNode);
}

const ModalDiv = styled(motion.div)`
  position: absolute;
  z-index: 1;
  top: 50%;
  left: 50%;
  background: #1D1D28;
  min-width: 60vw;
  min-height: 50vh;
  padding: 40px 20px;
  border-radius: 20px;
  transform: translate(-50%, -50%);
  
  font-family: Raleway, serif;
  font-style: normal;
  font-weight: bold;
  color: white;
  font-size: 40px;
  
  text-align: center;
`

const Backdrop = styled(motion.div)`
  position: absolute;
  z-index: 0;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.6);
`
