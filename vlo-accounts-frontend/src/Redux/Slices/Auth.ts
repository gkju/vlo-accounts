import {User} from "oidc-client";
import {createSlice, PayloadAction} from "@reduxjs/toolkit";
import {RootState} from "../Store/Store";


interface authState {
    loggedIn: boolean,
    profile?: {},
    lastKnownUrl: string
}

const initialState = {loggedIn: false, profile: {}, lastKnownUrl: "/"} as authState;

export const authSlice = createSlice({
    name: "auth",
    initialState,
    reducers: {
        setLoggedIn(state, action: PayloadAction<{profile: {}}>) {
            state.loggedIn = true;
            state.profile = action.payload.profile;
        },
        setLoggedOut(state) {
            state.loggedIn = false;
            state.profile = undefined;
        },
        setLastKnownUrl(state, action: PayloadAction<{url: string}>) {
            state.lastKnownUrl = action.payload.url;
        }
    }
})

export const selectProfile = (state: RootState) => state.auth.profile;
export const selectLoggedIn = (state: RootState) => state.auth.loggedIn;
export const selectLastKnownUrl = (state: RootState) => state.auth.lastKnownUrl;

export const {setLoggedIn, setLoggedOut, setLastKnownUrl} = authSlice.actions;
