using Microsoft.AspNetCore.Identity;

namespace VLO_BOARDS
{
    public class Internationalization
    {
        public class PolishIdentityErrorDescriber : IdentityErrorDescriber
        {
            public override IdentityError DefaultError()
            {
                return new IdentityError {Code = nameof(DefaultError), Description = $"Wystąpił nieznany błąd"};
            }

            public override IdentityError ConcurrencyFailure()
            {
                return new IdentityError
                {
                    Code = nameof(ConcurrencyFailure),
                    Description = "Błąd synchronizacji optymistycznej, zmodyfikowano obiekt"
                };
            }

            public override IdentityError PasswordMismatch()
            {
                return new IdentityError {Code = nameof(PasswordMismatch), Description = "Niepoprawne hasło"};
            }

            public override IdentityError InvalidToken()
            {
                return new IdentityError {Code = nameof(InvalidToken), Description = "Zły kod"};
            }

            public override IdentityError LoginAlreadyAssociated()
            {
                return new IdentityError
                {
                    Code = nameof(LoginAlreadyAssociated),
                    Description = "Użytkownik używający danej metody logowania już istnieje"
                };
            }

            public override IdentityError InvalidUserName(string userName)
            {
                return new IdentityError
                {
                    Code = nameof(InvalidUserName),
                    Description =
                        $"Nazwa użytkownika '{userName}' jest niepoprawna, może zawierać tylko znaki alfanumeryczne oraz wybrane znaki specjalne"
                };
            }

            public override IdentityError InvalidEmail(string email)
            {
                return new IdentityError
                    {Code = nameof(InvalidEmail), Description = $"Email '{email}' jest niepoprawny"};
            }

            public override IdentityError DuplicateUserName(string userName)
            {
                return new IdentityError
                {
                    Code = nameof(DuplicateUserName), Description = $"Nazwa użytkownika '{userName}' już jest zabrana"
                };
            }

            public override IdentityError DuplicateEmail(string email)
            {
                return new IdentityError
                    {Code = nameof(DuplicateEmail), Description = $"Email '{email}' jest już zajęty"};
            }

            public override IdentityError InvalidRoleName(string role)
            {
                return new IdentityError
                    {Code = nameof(InvalidRoleName), Description = $"Nazwa roli '{role}' jest niepoprawna"};
            }

            public override IdentityError DuplicateRoleName(string role)
            {
                return new IdentityError
                    {Code = nameof(DuplicateRoleName), Description = $"Nazwa roli '{role}' jest już zajęta"};
            }

            public override IdentityError UserAlreadyHasPassword()
            {
                return new IdentityError
                    {Code = nameof(UserAlreadyHasPassword), Description = "Użytkownik już ustawił swoje hasło"};
            }

            public override IdentityError UserLockoutNotEnabled()
            {
                return new IdentityError
                {
                    Code = nameof(UserLockoutNotEnabled),
                    Description = "Blokada konta nie jest włączona dla tego użytkownika"
                };
            }

            public override IdentityError UserAlreadyInRole(string role)
            {
                return new IdentityError
                    {Code = nameof(UserAlreadyInRole), Description = $"Użytkownik już posiada rolę '{role}'"};
            }

            public override IdentityError UserNotInRole(string role)
            {
                return new IdentityError
                    {Code = nameof(UserNotInRole), Description = $"Użytkownik nie ma roli '{role}'"};
            }

            public override IdentityError PasswordTooShort(int length)
            {
                return new IdentityError
                    {Code = nameof(PasswordTooShort), Description = $"Hasło musi mieć co najmniej {length} znaków"};
            }

            public override IdentityError PasswordRequiresNonAlphanumeric()
            {
                return new IdentityError
                {
                    Code = nameof(PasswordRequiresNonAlphanumeric),
                    Description = "Hasło musi zawierać co najmniej jeden znak alfanumeryczny"
                };
            }

            public override IdentityError PasswordRequiresDigit()
            {
                return new IdentityError
                {
                    Code = nameof(PasswordRequiresDigit),
                    Description = "Hasło musi zawierać co najmniej jedną cyfrę ('0'-'9')"
                };
            }

            public override IdentityError PasswordRequiresLower()
            {
                return new IdentityError
                {
                    Code = nameof(PasswordRequiresLower),
                    Description = "Hasło musi zawierać co najmniej jedną małą literę ('a'-'z')"
                };
            }

            public override IdentityError PasswordRequiresUpper()
            {
                return new IdentityError
                {
                    Code = nameof(PasswordRequiresUpper),
                    Description = "Hasło musi zawierać co najmniej jedną wielką literę ('A'-'Z')"
                };
            }

            public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
            {
                return new IdentityError
                {
                    Code = nameof(PasswordRequiresUniqueChars),
                    Description = $"Hasło musi zawierać co najmniej {uniqueChars} unikalnych znaków"
                };
            }

            public override IdentityError RecoveryCodeRedemptionFailed()
                {
                    return new IdentityError
                    {
                        Code = nameof(RecoveryCodeRedemptionFailed),
                        Description = $"Niepoprawny kod odzyskiwania, spróbuj ponownie lub skonsultuj się z su"
                    }; }
                }
            
    }
}