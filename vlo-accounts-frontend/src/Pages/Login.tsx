import {FunctionComponent, useState} from "react";
import styled from "styled-components";
import vlobg from "./vlobg.png";
import {Logo} from "../Logo";
import {TextInput} from "../Components/Inputs/TextInput";
import {InputSize} from "../Components/Constants";
import {Form, FormikProvider, FormikValues, useFormik} from "formik";
import * as Yup from 'yup';
import {Button} from "../Components/Inputs/Button";
import {motion} from "framer-motion";
import {Modal} from "../Components/Modal";
import {useMount} from "react-use";
import axios from "axios";
import {GoogleReCaptchaProvider, useGoogleReCaptcha} from "react-google-recaptcha-v3";
import qs from "qs";
import {NavigateToReturnUrl} from "./ReturnUrlUtils";
import {ReactComponent as GoogleLogo } from "./glogo.svg";
import {Layout, ErrorSpan, InputWrapper, Container, Bg} from "./SharedStyledComponents";

const navigateGoogle = () => {
    const queryParams = qs.parse(window.location.search.substr(1));
    window.location.href = "/Auth/ExternalLogin?provider=Google&" + queryParams;
}

export const Login: FunctionComponent = (props) => {
    const {executeRecaptcha} = useGoogleReCaptcha();

    const returnUrl = String(qs.parse(window.location.search.substr(1))["returnUrl"]);
    const error = String(qs.parse(window.location.search.substr(1))["error"] || "");

    const handleSubmit = async (values: FormikValues) => {
        setLoginError("");
        const captchaResponse = await executeRecaptcha("login");
        try {
            let response = await axios.post("/Auth/Login" + window.location.search, {userName: values.username, password: values.password, rememberMe: true, captchaResponse});
            if(response.status === 200) {
                await NavigateToReturnUrl(returnUrl);
            }
        } catch (res) {
            let response = res.response;
            if(response.status === 400) {
                setLoginError(response.data[""][0]);
            } else if(response.status === 422) {
                //handle 2fa
            }  else if(response.status === 423) {
                setLoginError("Konto zostało zablokowane na 5 minut po ostatniej nieudanej próbie logowania");
            }
        }

    }

    const Formik = useFormik(        {
        initialValues: {username: '', password: ''},
        onSubmit: handleSubmit,
        validationSchema: Yup.object({
            username: Yup.string()
                .required("Podaj nazwę użytkownika"),
            password: Yup.string()
                .required("Podaj hasło")
                .min(8, "Hasło musi mieć co najmniej 8 znaków")
                .matches(/\W/, "Hasło musi zawierać znak specjalny")
                .matches(/\d/, "Hasło musi zawierać cyfrę")
                .matches(/(.*[a-z].*)/, "Hasło musi zawierać co najmniej jedną małą literę")
                .matches(/(.*[A-Z].*)/, "Hasło musi zawierać co najmniej jedną dużą literę")
        })
    });

    const [modal, setModal] = useState(false);
    const [loginError, setLoginError] = useState("");

    useMount(() => {
        if(error !== "") {
            setLoginError(error);
        }
    })

    return (
        <Layout>
            <Container>
                <Logo />
                <section>
                    <FormikProvider value={Formik}>
                        <Form autoComplete="off">
                            <InputWrapper>
                                <TextInput name="username" placeholder={"Nazwa użytkownika"} size={InputSize.Medium} />
                            </InputWrapper>
                            <InputWrapper>
                                <TextInput name="password" password={true} placeholder={"Hasło"} size={InputSize.Medium} />
                            </InputWrapper>
                            <InputWrapper style={{marginBottom: "0"}}>
                            <Button type="submit" size={InputSize.Medium}>Zaloguj się</Button>
                            </InputWrapper>
                            <InputWrapper style={{marginTop: "5px"}}>
                                <ErrorSpan style={{zIndex: 1, margin: "0 0", maxWidth: "400px"}}>
                                    <motion.span animate={{opacity: !!loginError ? 1 : 0}}>{loginError}</motion.span>
                                </ErrorSpan>
                            </InputWrapper>
                        </Form>
                    </FormikProvider>
                    <Modal open={modal} close={() => setModal(false)}>helo</Modal>
                    <InputWrapper style={{marginTop: "0"}}>
                        <Button onClick={navigateGoogle} type="submit" primaryColor="#FFFFFF" secondaryColor="#595959" size={InputSize.Medium} style={{display: "flex", justifyContent: "center", alignContent: "center"}}>Zaloguj się z <GoogleLogo style={{width: "42px", marginBottom: "-8px", marginLeft: "20px", transform: "scale(1.7)"}}/></Button>
                    </InputWrapper>
                </section>
            </Container>
            <Bg/>
        </Layout>
    )
}
