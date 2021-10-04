import {FunctionComponent, useState} from "react";
import {Logo} from "../Logo";
import {TextInput} from "../Components/Inputs/TextInput";
import {InputSize} from "../Components/Constants";
import {Form, FormikProvider, FormikValues, useFormik} from "formik";
import * as Yup from 'yup';
import {Button} from "../Components/Inputs/Button";
import {motion} from "framer-motion";
import {Modal} from "../Components/Modal";
import {useMount} from "react-use";
import {useGoogleReCaptcha} from "react-google-recaptcha-v3";
import qs from "qs";
import {NavigateToReturnUrl} from "./ReturnUrlUtils";
import {ReactComponent as GoogleLogo } from "./glogo.svg";
import {Layout, ErrorSpan, InputWrapper, Container, Bg} from "./SharedStyledComponents";
import {LoginApi} from "vlo-accounts-client";
import {apiLocation, OpenApiSettings} from "../Config";
import {GetReturnUrl} from "../Utils";

const navigateGoogle = () => {
    window.location.href = apiLocation + "/Auth/ExternalLogin?provider=Google" + (window.location.search.length > 1 ? "&" + window.location.search.substr(1) : "");
}

export const Login: FunctionComponent = (props) => {
    const { executeRecaptcha } = useGoogleReCaptcha();

    const returnUrl = GetReturnUrl(window.location.search);
    let error: string = String(qs.parse(window.location.search.substr(1))["error"]);

    if(error === "undefined") {
        error = "";
    }

    const handleSubmit = async (values: FormikValues) => {
        setLoginError("");
        const captchaResponse = executeRecaptcha ? await executeRecaptcha("login") : "";
        try {
            let loginApi = new LoginApi(OpenApiSettings);
            let response = await loginApi.apiAuthLoginPost(GetReturnUrl(window.location.search), {username: values.username, password: values.password, rememberMe: true, captchaResponse});

            if(response.status === 200) {
                await NavigateToReturnUrl(returnUrl);
            } else {
                throw response;
            }
        } catch (res: any) {
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
                                    <motion.span animate={{opacity: loginError !== "" ? 1 : 0}}>{loginError}</motion.span>
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
