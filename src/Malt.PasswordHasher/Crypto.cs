//Contains modified and as is code from Microsoft
//
//Copyright (c) Microsoft Open Technologies, Inc.  All rights reserved.
//Microsoft Open Technologies would like to thank its contributors, a list
//of whom are at http://aspnetwebstack.codeplex.com/wikipage?title=Contributors.
//
//Licensed under the Apache License, Version 2.0 (the "License"); you
//may not use this file except in compliance with the License. You may
//obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

//See http://aspnetwebstack.codeplex.com/SourceControl/latest#src/System.Web.Helpers/Crypto.cs

using System;
using System.Security.Cryptography;
using System.Text;
using CryptSharp.Utility;

namespace Malt.PasswordHasher
{
    public static class Crypto
    {
        private const int SaltSize = 256 / 8;

        public static string HashPassword<T>(string password, int iterations) where T : KeyedHashAlgorithm, new()
        {
            var saltBytes = GenerateSalt();
            var hashBytes = HashWithPbfdk2<T>(password, saltBytes, iterations);
            var combinedBytes = new byte[hashBytes.Length + saltBytes.Length];
            Buffer.BlockCopy(saltBytes, 0, combinedBytes, 0, saltBytes.Length);
            Buffer.BlockCopy(hashBytes, 0, combinedBytes, saltBytes.Length, hashBytes.Length);
            var saltAndHash = Convert.ToBase64String(combinedBytes);
            return saltAndHash;
        }

        public static byte[] HashWithPbfdk2<T>(string password, byte[] saltBytes, int iterations) where T : KeyedHashAlgorithm, new()
        {
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            
            var hashSize = new T().HashSize/8;
            var hashedPassword = new byte[hashSize];

            Pbkdf2.ComputeKey(passwordBytes, saltBytes, iterations, Pbkdf2.CallbackFromHmac<T>(), hashSize, hashedPassword);
            return hashedPassword;
        }

        public static byte[] GenerateSalt()
        {
            var salt = new byte[SaltSize];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        public static bool VerifyHashedPassword<T>(string hashedPassword, string password, int iterations) where T : KeyedHashAlgorithm, new()
        {
            if (hashedPassword == null)
            {
                throw new ArgumentNullException("hashedPassword");
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            var hashedPasswordBytes = Convert.FromBase64String(hashedPassword);
            var hashSize = new T().HashSize/8;
            if (hashedPasswordBytes.Length != (SaltSize + hashSize))
            {
                return false;
            }

            var salt = new byte[SaltSize];
            Buffer.BlockCopy(hashedPasswordBytes, 0, salt, 0, SaltSize);
            var storedSubkey = new byte[hashSize];
            Buffer.BlockCopy(hashedPasswordBytes, SaltSize, storedSubkey, 0, hashSize);

            var generatedSubkey = HashWithPbfdk2<T>(password, salt, iterations);
            return ByteArraysEqual(storedSubkey, generatedSubkey);
        }

        /// <summary>
        /// Compares two byte arrays without short circuiting.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            bool areSame = true;
            for (int i = 0; i < a.Length; i++)
            {
                areSame &= (a[i] == b[i]);
            }
            return areSame;
        }
    }
}
