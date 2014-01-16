using System;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNet.Identity;
using NUnit.Framework;

namespace Malt.PasswordHasher.Test
{
    [TestFixture]
    public class PasswordHasherTests
    {
        [Test]
        public void HashedPasswordIsInCorrectFormat()
        {
            var hasher = new PasswordHasher<HMACSHA512>();
            var hash = hasher.HashPassword("Cats");
            var parts = hash.Split('.');
            Assert.That(parts.Length, Is.EqualTo(2));
            Assert.That(parts[0], Is.EqualTo("25000"));
            var saltAndHash = Convert.FromBase64String(parts[1]);
            Assert.That(saltAndHash.Length, Is.EqualTo(256/8 + 512/8));
        }

        [Test]
        public void CryptSharpProducesSameResultAsRfc2898DeriveBytesForHmacSha1()
        {
            var salt = Crypto.GenerateSalt();
            //cryptsharp
            var customHash = Crypto.HashWithPbfdk2<HMACSHA1>("Cats", salt, 1000);

            //ms
            byte[] msHash;
            using (var derived = new Rfc2898DeriveBytes("Cats", salt, 1000))
            {
                msHash = derived.GetBytes(160/8);
            }
            Assert.IsTrue(customHash.SequenceEqual(msHash));
        }

        [Test]
        public void CustomIterationsShouldBeStoredAtStartOfHash()
        {
            var hasher = new PasswordHasher<HMACSHA512>(1000);
            var hash = hasher.HashPassword("TestsAreAw3s0m3");
            var parts = hash.Split('.');
            Assert.That(parts.Length, Is.EqualTo(2));
            Assert.That(parts[0], Is.EqualTo("1000"));
        }

        [Test]
        public void VerifyPasswordShouldReturnSuccessIfPasswordIsCorrect()
        {
            var hasher = new PasswordHasher<HMACSHA512>(1000);
            var hash = hasher.HashPassword("Cats");
            var result = hasher.VerifyHashedPassword(hash, "Cats");
            Assert.That(result, Is.EqualTo(PasswordVerificationResult.Success));
        }

        [Test]
        public void VerifyPasswordShouldReturnFailedIfDifferent()
        {
            var hasher = new PasswordHasher<HMACSHA512>(1000);
            var hash = hasher.HashPassword("Cats");
            var result = hasher.VerifyHashedPassword(hash, "Dogs");
            Assert.That(result, Is.EqualTo(PasswordVerificationResult.Failed));
        }

        [Test]
        public void VerifyPasswordShouldReturnFailedIfIterationsComponentInvalid()
        {
            var hasher = new PasswordHasher<HMACSHA512>(1000);
            var hash = hasher.HashPassword("Cats");
            var parts = hash.Split('.');
            var invalid = string.Join(".", "Invalid", parts[1]);
            var result = hasher.VerifyHashedPassword(invalid, "Cats");
            Assert.That(result, Is.EqualTo(PasswordVerificationResult.Failed));
        }

        [Test]
        public void VerifyPasswordShouldReturnFailedIfIterationsComponentMissing()
        {
            var hasher = new PasswordHasher<HMACSHA512>(1000);
            var hash = hasher.HashPassword("Cats");
            var parts = hash.Split('.');
            var invalid = parts[1];
            var result = hasher.VerifyHashedPassword(invalid, "Cats");
            Assert.That(result, Is.EqualTo(PasswordVerificationResult.Failed));
        }

        [Test]
        public void VerifyPasswordShouldReturnSuccessRehashRequiredIfIterationsAreDifferent()
        {
            var hasher = new PasswordHasher<HMACSHA512>(1000);
            var hash = hasher.HashPassword("Cats");
            var betterHasher = new PasswordHasher<HMACSHA512>(10000);
            var result = betterHasher.VerifyHashedPassword(hash, "Cats");
            Assert.That(result, Is.EqualTo(PasswordVerificationResult.SuccessRehashNeeded));
        }


        [Test]
        public void VerifyPasswordShouldThrowArgumentNullIfPasswordNull()
        {
            var hasher = new PasswordHasher<HMACSHA512>(1000);
            var hash = hasher.HashPassword("Cats");
            Assert.Throws<ArgumentNullException>(() => hasher.VerifyHashedPassword(hash, null));
        }

        [Test]
        public void VerifyPasswordShouldThrowArgumentNullIfHashNull()
        {
            var hasher = new PasswordHasher<HMACSHA512>(1000);
            var hash = hasher.HashPassword("Cats");
            Assert.Throws<ArgumentNullException>(() => hasher.VerifyHashedPassword(null, "Cats"));
        }

        [Test]
        public void HashPasswordShouldThrowArgumentNullIfPasswordNull()
        {
            var hasher = new PasswordHasher<HMACSHA512>(1000);
            Assert.Throws<ArgumentNullException>(() => hasher.HashPassword(null));
        }

    }
}
