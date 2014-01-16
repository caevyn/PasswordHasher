PasswordHasher
==============

##An implementation of Asp.Net Identity IPasswordHasher

The default IPasswordHasher does use PBKDF2 but is limited to using HMACSHA1 as the PRF and 1000 iterations.
This implementation uses the [CryptSharp] library.
CryptSharp's PBKDF2 implementation works with any HMAC.
You can specify the PRF and number of iterations when you create an instance of PasswordHasher.

```sh
//E.g. To use with HMACSHA512 as the PRF and 30000 iterations
userManager.PasswordHasher = new PasswordHasher<HMACSHA512>(30000);
```
The Crypto class borrows heavily from Microsoft's [System.Web.Helpers.Crypto] and reuses as much as possible.

If you don't *have* to use Asp.Net Identity check out [MembershipReboot]

License: Apache 2.0

[CryptSharp]:https://github.com/ChrisMcKee/cryptsharp
[System.Web.Helpers.Crypto]:http://aspnetwebstack.codeplex.com/SourceControl/latest#src/System.Web.Helpers/Crypto.cs
[MembershipReboot]:https://github.com/brockallen/BrockAllen.MembershipReboot