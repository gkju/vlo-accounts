import {NavigateToReturnUrl} from "./ReturnUrlUtils";
import {Logo} from "../Logo";
import {TextInput} from "../Components/Inputs/TextInput";
import {InputSize} from "../Components/Constants";
import {Button} from "../Components/Inputs/Button";
import {motion} from "framer-motion";
import {useMount} from "react-use";
import {Form, FormikProvider, FormikValues, useFormik} from "formik";
import {useGoogleReCaptcha} from "react-google-recaptcha-v3";
import axios from "axios";
import {FunctionComponent, useState} from "react";
import {Layout, ErrorSpan, InputWrapper, Container, Bg} from "./SharedStyledComponents";
import qs from "qs";
import * as Yup from 'yup';

export const RegisterExternalLogin: FunctionComponent = (props) => {
    const returnUrl = String(qs.parse(window.location.search.substr(1))["returnUrl"]);

    const handleSubmit = async (values: FormikValues) => {
        setRegisterError("");

        try {
            let response = await axios.post("/Auth/ExternalLogin/CreateAccount" + window.location.search, {username: values.username, email: values.email});
            if(response.status === 200) {
                await NavigateToReturnUrl(returnUrl);
            }
        } catch (res) {
            let response = res.response;
            if(response.status === 400) {
                setRegisterError(response.data[""][0]);
            }
        }

    }

    const Formik = useFormik(        {
        initialValues: {username: qs.parse(window.location.search.substr(1))["username"] || "", email: qs.parse(window.location.search.substr(1))["email"] || ""},
        onSubmit: handleSubmit,
        validationSchema: Yup.object({
            username: Yup.string()
                .required("Podaj nazwę użytkownika"),
            email: Yup.string()
                .required("Podaj adres email")
                .email("Podaj poprawny adres email")
        })
    });

    const [registerError, setRegisterError] = useState("");

    useMount(() => {

    })

    return (
        <Layout>
            <Container>
                <Logo />
                <section>
                    <FormikProvider value={Formik}>
                        <Form>
                            <InputWrapper>
                                <TextInput name="username" placeholder={"Nazwa użytkownika"} size={InputSize.Medium} />
                            </InputWrapper>
                            <InputWrapper>
                                <TextInput name="email" placeholder={"Email"} size={InputSize.Medium} />
                            </InputWrapper>
                            <InputWrapper style={{marginBottom: "0"}}>
                                <Button type="submit" size={InputSize.Medium}>Zarejestruj się</Button>
                            </InputWrapper>
                        </Form>
                    </FormikProvider>
                    <InputWrapper style={{zIndex: 1, margin: "0 0"}}>
                        <ErrorSpan>
                            <motion.span animate={{opacity: !!registerError ? 1 : 0}}>{registerError}</motion.span>
                        </ErrorSpan>
                    </InputWrapper>
                </section>
            </Container>
            <Bg/>
        </Layout>
    )
}
