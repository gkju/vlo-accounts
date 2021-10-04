import {authoritySettings} from "../Config";
import {UserManager} from "oidc-client";
import Store from "../Redux/Store/Store";
import { setLoggedIn, setLoggedOut } from "../Redux/Slices/Auth";
import {EmptyObj} from "../Utils";

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
            await this.userManager.signinSilent();
            Store.dispatch(setLoggedIn({profile: (await this.userManager.getUser()).profile}));
        } catch(e) {
            console.error(e);
        }
    }

    async processSignInUrl(url: string): Promise<boolean> {
        try {
            await this.ensureUserManagerCreated();
            await this.userManager.signinCallback(url);
            Store.dispatch(setLoggedIn({profile: (await this.userManager.getUser()).profile}));
            return true;
        } catch (error) {
            return false;
        }
    }

    async GetToken() {
        await this.ensureUserManagerCreated();
        return (await this.userManager.getUser())?.access_token || "";
    }

    onUserLoaded = (user: any) => {
        Store.dispatch(setLoggedIn({profile: user.profile}));
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
}

const authService = new AuthService();

export default authService;
