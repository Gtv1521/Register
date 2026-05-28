using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;

namespace FrameworkDriver_Api.src.Utils
{
    public class HashPassword : IHashPass<UserDto>
    {
        private readonly PasswordHasher<UserDto> hasher = new();
        public string Hash(UserDto user, int password)
        {
            return hasher.HashPassword(user, password.ToString());
        }

        public bool Verify(UserDto user, int password, int hashedPassword)
        {
            var result = hasher.VerifyHashedPassword(user, hashedPassword.ToString(), password.ToString());
            return result == PasswordVerificationResult.Success;
        }
    }
}