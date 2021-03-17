import {User, UserManager, WebStorageStateStore} from "oidc-client";

export const CaptchaConfig = {
  CaptchaKey: "6LfvfoAaAAAAAArJt9n55Z-7WbwCJccw2QAGNOCS"
}

let otherAuthSettings = {
  WebStoragePrefix: "VLO_BOARDS_AUTH"
}

export let authoritySettings = {
  authority: "https://localhost:44328",
  client_id: "VLO_BOARDS",
  redirect_uri: "https://localhost:44328/login-callback",
  response_type: "code",
  scope:"openid profile VLO_BOARDS",
  post_logout_redirect_uri : "https://localhost:44328/logout-callback",
  userStore: new WebStorageStateStore({
    prefix: otherAuthSettings.WebStoragePrefix
  }),
  includeIdTokenInSilentRenew: true,
  automaticSilentRenew: true
};
