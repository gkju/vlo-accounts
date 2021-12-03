using System;
using System.Runtime.InteropServices;
using System.Text;

namespace VLO_BOARDS.Auth
{
    //libsodium implementation with the help of twelve21.io, currently there are no full lib sodium implementations in .net
    public class SodiumLib
    {
        private const string Name = "libsodium";
        public const int crypto_pwhash_argon2id_ALG_ARGON2ID13 = 2;
        public const long crypto_pwhash_argon2id_OPSLIMIT = 4;
        public const int crypto_pwhash_argon2id_MEMLIMIT_BYTES = 273741824;
        
        static SodiumLib()
        {
            sodium_init();
        }
        
        public byte[] CreateSalt()
        {
            var buffer = new byte[32];
            SodiumLib.randombytes_buf(buffer, buffer.Length);
            return buffer;
        }
        
        public byte[] HashPassword(string password, byte[] salt)
        {
            var hash = new byte[32];

            var result = SodiumLib.crypto_pwhash(
                hash,
                hash.Length,
                Encoding.UTF8.GetBytes(password),
                password.Length,
                salt,
                SodiumLib.crypto_pwhash_argon2id_OPSLIMIT,
                SodiumLib.crypto_pwhash_argon2id_MEMLIMIT_BYTES,
                SodiumLib.crypto_pwhash_argon2id_ALG_ARGON2ID13
            );

            if (result != 0)
                throw new Exception("An unexpected error has occurred.");

            return hash;
        }

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void sodium_init();

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void randombytes_buf(byte[] buffer, int size);

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int crypto_pwhash(byte[] buffer, long bufferLen, byte[] password, long passwordLen, byte[] salt, long opsLimit, int memLimit, int alg);

        [DllImport(Name, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int crypto_pwhash_str_verify(string str, byte[] passwd, int passwdlen);
    }
}