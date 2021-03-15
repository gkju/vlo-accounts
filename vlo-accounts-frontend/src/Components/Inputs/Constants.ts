export interface textInputProps {
    size?: InputSize,
    password?: boolean,
    error?: boolean,
    errors?: string,
    id?: string,
    name: string,
    placeholder: string
}

export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
    size?: InputSize,
    primaryColor?: string,
    secondaryColor?: string
}

export interface inputWrapperProps {
    size?: InputSize,
}

export enum InputSize {
    Big,Medium, Small
}
