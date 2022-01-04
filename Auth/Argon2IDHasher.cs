using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Options;

namespace VLO_BOARDS.Auth
{
    public class Argon2IDHasher<TUser> : PasswordHasher<TUser> where TUser : class
    {
        public override PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
        {
            if (hashedPassword == null)
            {
                throw new ArgumentNullException(nameof(hashedPassword));
            }
            if (providedPassword == null)
            {
                throw new ArgumentNullException(nameof(providedPassword));
            }

            byte[] decodedHashedPassword = Convert.FromBase64String(hashedPassword);

            // read the format marker from the hashed password
            if (decodedHashedPassword.Length == 0)
            {
                return PasswordVerificationResult.Failed;
            }

            if (decodedHashedPassword[0] == 0xFF)
            {
                byte[] unMarkedHashedPassword = new byte[decodedHashedPassword.Length - 1];
                Array.Copy(decodedHashedPassword, 1, unMarkedHashedPassword, 0, decodedHashedPassword.Length - 1);
                
                if(VerifyHashedPasswordArgon2Id(unMarkedHashedPassword, providedPassword))
                {
                    return PasswordVerificationResult.Success;
                } else
                {
                    return PasswordVerificationResult.Failed;
                }
            }

            var result = base.VerifyHashedPassword(user, hashedPassword, providedPassword);
            
            if (result == PasswordVerificationResult.Success ||
                result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                return PasswordVerificationResult.SuccessRehashNeeded;
            }
            else
            {
                return PasswordVerificationResult.Failed;
            }
        }
        
        public override string HashPassword(TUser user, string password)
        {
            int saltSize = 32;
            
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            byte[] salt = new byte[saltSize];
            salt = SodiumLib.CreateSalt();

            byte[] hashedPassword = SodiumLib.HashPassword(password, salt);

            byte[] resultMarked = new byte[hashedPassword.Length + salt.Length+ 1];
            resultMarked[0] = 0xFF;
            Array.Copy(salt, 0, resultMarked, 1, salt.Length);
            Array.Copy(hashedPassword, 0, resultMarked, 1 + salt.Length, hashedPassword.Length);
            
            return Convert.ToBase64String(resultMarked);
        }
        
        private bool VerifyHashedPasswordArgon2Id(byte[] hashedPassword, string password)
        {
            try
            {
                int saltLength = 32;

                if (hashedPassword.Length - saltLength <= 0)
                {
                    return false;
                }
                
                byte[] salt = new byte[saltLength];
                Buffer.BlockCopy(hashedPassword, 0, salt, 0, salt.Length);

                byte[] expectedPassword = new byte[hashedPassword.Length - saltLength];
                Buffer.BlockCopy(hashedPassword, salt.Length, expectedPassword, 0, hashedPassword.Length - saltLength);

                byte[] resultPassword = SodiumLib.HashPassword(password, salt);

                return CryptographicOperations.FixedTimeEquals(expectedPassword, resultPassword);
            }
            catch
            {
                // This should never occur except in the case of a malformed payload, where
                // we might go off the end of the array. Regardless, a malformed payload
                // implies verification failed.
                return false;
            }
        }
    }
}