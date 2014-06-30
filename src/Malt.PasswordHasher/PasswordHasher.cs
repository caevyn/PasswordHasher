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

using System;
using System.Security.Cryptography;
using Microsoft.AspNet.Identity;

namespace Malt.PasswordHasher
{
    public class PasswordHasher<T> : IPasswordHasher where T : KeyedHashAlgorithm, new()
    {
        private readonly int _iterations;
        
        public PasswordHasher(int iterations = 25000)
        {
            _iterations = iterations;
        }
        
        /// <summary>
        /// Hash password with salt and return string to store in the format {Iterations}.{Salt}{Hash}
        /// </summary>
        /// <param name="password">The clear text password to hash</param>
        /// <returns>A string including the iterations, salt and hash to be stored in the database</returns>
        public string HashPassword(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
         
            var saltAndHash = Crypto.HashPassword<T>(password, _iterations);
            return string.Format("{0}.{1}", _iterations, saltAndHash);
        }

       

        /// <summary>
        /// Verify that the hash of the supplied password matches the stored hash for that user.
        /// </summary>
        /// <param name="hashedPassword">The hash stored in the database in the format {Iterations}.{Salt}{Hash}</param>
        /// <param name="providedPassword">The clear text password to verify</param>
        /// <returns>A PasswordVerificationResult. If the iterations have changed since the user last logged in SuccessRehashNeeded is returned.</returns>
        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            if (hashedPassword == null)
            {
                throw new ArgumentNullException("hashedPassword");
            }
            if (providedPassword == null)
            {
                throw new ArgumentNullException("providedPassword");
            }

            var parts = hashedPassword.Split('.');
            int iterations;
            if (parts.Length != 2 || !int.TryParse(parts[0], out iterations))
            {
                return PasswordVerificationResult.Failed;
            }
            var hashAndSalt = parts[1];
            bool isEqual = Crypto.VerifyHashedPassword<T>(hashAndSalt, providedPassword, iterations);
            if(!isEqual)
            {
                return PasswordVerificationResult.Failed;
            }
            return _iterations == iterations
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
