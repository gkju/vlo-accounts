import { configureStore, combineReducers } from '@reduxjs/toolkit';
import {authSlice} from "../Slices/Auth";

const rootReducer = combineReducers({auth: authSlice.reducer});

export default configureStore({reducer: rootReducer});

export type RootState = ReturnType<typeof rootReducer>
