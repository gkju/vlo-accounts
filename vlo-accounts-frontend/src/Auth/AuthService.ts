import {authoritySettings} from "../Config";
import {UserManager} from "oidc-client";
import Store from "../Redux/Store/Store";
import { setLoggedIn, setLoggedOut } from "../Redux/Slices/Auth";
import * as qs from "qs";

export class AuthService {
    private userManager: any = undefined;

    async ensureUserManagerCreated() {
        if(this.userManager === undefined) {
            this.userManager = new UserManager(authoritySettings);
            this.userManager.events.addUserLoaded(this.onUserLoaded);
            this.userManager.events.addSilentRenewError(this.onSilentRenewError);
            this.userManager.events.addAccessTokenExpired(this.onAccessTokenExpired);
            this.userManager.events.addAccessTokenExpiring(this.onAccessTokenExpiring);
            this.userManager.events.addUserUnloaded(this.onUserUnloaded);
            this.userManager.events.addUserSignedOut(this.onUserSignedOut);
        }
    }

    async signInSilent() {
        await this.ensureUserManagerCreated();
        try {
            console.log("attempting silent login");
            let user = await this.userManager.signinSilent();
            console.log("got user", user);
            Store.dispatch(setLoggedIn(user.profile));
        } catch(e) {
            console.error(e);
        }
    }

    async processSignInUrl(url: string): Promise<boolean> {
        try {
            await this.ensureUserManagerCreated();
            console.log("handling url", url);
            const user = await this.userManager.signinCallback(url);
            Store.dispatch(setLoggedIn(user.profile));
            return true;
        } catch (error) {
            //how did we get here
            console.log('it brokey');
            return false;
        }
    }

    onUserLoaded = (user: any) => {
        Store.dispatch(setLoggedIn(user.profile));
    }

    onSilentRenewError = (error: any) => {
        Store.dispatch(setLoggedOut());
    }

    onAccessTokenExpired = () => {
        Store.dispatch(setLoggedOut());
    }

    onUserUnloaded = () => {
        Store.dispatch(setLoggedOut());
    }

    onAccessTokenExpiring = () => {

    }

    onUserSignedOut = () => {
        Store.dispatch(setLoggedOut());
    }

    static get instance() { return authService }

    public static getRedirectUrl(): string {
        const redirectUrl = String(qs.parse(window.location.search.substr(1))["redirectUrl"]);
        if (redirectUrl && !redirectUrl.startsWith(`${window.location.origin}/`)) {
            throw new Error("Zły redirect url, potencjalnie open redirect attack")
        }
        return redirectUrl || `${window.location.origin}/`;
    }
}

const authService = new AuthService();

export default authService;
