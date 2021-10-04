import {User, UserManager, WebStorageStateStore} from "oidc-client";
import {Configuration} from "vlo-accounts-client";
import authService from "./Auth/AuthService";

export const CaptchaConfig = {
  CaptchaKey: "6LfvfoAaAAAAAArJt9n55Z-7WbwCJccw2QAGNOCS"
}

const otherAuthSettings = {
  WebStoragePrefix: "VLO_BOARDS_AUTH"
}

export const apiLocation = "/api";

export const authoritySettings = {
  authority: "https://localhost:5001",
  client_id: "VLO_BOARDS",
  redirect_uri: "https://localhost:5001/login-callback",
  silent_redirect_uri: "https://localhost:5001/login-callback",
  response_type: "code",
  scope:"openid profile VLO_BOARDS",
  post_logout_redirect_uri : "https://localhost:5001/logout-callback",
  userStore: new WebStorageStateStore({
    prefix: otherAuthSettings.WebStoragePrefix
  }),
  includeIdTokenInSilentRenew: true,
  automaticSilentRenew: true
};

export const OpenApiSettings : Configuration = new Configuration({
  basePath: "https://localhost:5001",
  accessToken: authService.GetToken()
});