namespace VLO_BOARDS.Areas.Auth;

public static class Constants
{
    public static string AccountError = "Account";
    public static string LockedOutStatus = "Konto zablokowane";
    public static string UnconfirmedOrNonexistentStatus = "Niepotwierdzone lub usunięte konto";
    
    public static string TwoFaError = "2FaError";
    public static string InvalidRecoveryCodeStatus = "Niepoprawny kod odzyskiwania.";
    public static string InvalidAuthenticatorCodeStatus = "Niepoprawny kod";
    public static string TwoFaRequiredStatus = "Wymagane jest uwierzytelnienie za pomocą MFA";
    public static string TwoFaNotEnabled = "Nie włączono mfa";

    public static string UsernameOrPasswordError = "UsernameOrPassword";
    public static string InvalidUsernameOrPasswordStatus = "Niepoprawna nazwa użytkownika lub hasło";
    public static string HasPasswordStatus = "Użytkownik posiada już hasło";

    public static string ExternalError = "ExternalError";
    public static string ExternalErrorStatus = "Wystąpił błąd zewnętrzny";
    public static string IneligibleForLoginRemoval = "Nie można usunąć zewnętrznego loginu";
    public static string MultiAccountError = "Spróbowałeś powiązać jedno konto zewnętrzne z wieloma kontami";

    public static string InvalidCodeError = "Code";
    public static string InvalidCodeStatus = "Niepoprawny kod";
}