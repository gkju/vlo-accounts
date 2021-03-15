import {FunctionComponent} from "react";
import styled from "styled-components";
import vlobg from "./vlobg.png";
import {Logo} from "../Logo";
import {TextInput} from "../Components/Inputs/TextInput";
import {InputSize} from "../Components/Inputs/Constants";
import {Form, FormikProvider, useFormik} from "formik";
import * as Yup from 'yup';
import {Button} from "../Components/Inputs/Button";

export const Login: FunctionComponent = (props) => {
    const Formik = useFormik(        {
        initialValues: {username: '', password: ''},
        onSubmit: async (values) => console.log(values),
        validationSchema: Yup.object({
            username: Yup.string()
                .required("Podaj nazwę użytkownika"),
            password: Yup.string()
                .required("Podaj hasło")
                .min(8, "Hasło musi mieć co najmniej 8 znaków")
                .matches(/\W/, "Hasło musi zawierać znak specjalny")
                .matches(/\d/, "Hasło musi zawierać cyfrę")
        })
    });

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
                                <TextInput name="password" password={true} placeholder={"Hasło"} size={InputSize.Medium} />
                            </InputWrapper>
                            <InputWrapper>
                            <Button type="submit" size={InputSize.Medium}>Zaloguj się</Button>
                            </InputWrapper>
                        </Form>
                    </FormikProvider>
                </section>

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
  grid-template-rows: 30vh 50vh 20vh;
  width: 100%;
`

const Bg = styled.div`
  width: 100%;
  background-image: Url("${vlobg}");
  background-position-y: top;
  background-position-x: center;
  background-size: cover;
`

const InputWrapper = styled.div`
  display: flex;
  justify-content: center;
  align-content: center;
  margin: 45px 0;
`
