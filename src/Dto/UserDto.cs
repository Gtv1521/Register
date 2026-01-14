using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Models;
using Microsoft.AspNetCore.Antiforgery;

namespace FrameworkDriver_Api.src.Dto
{
    public class UserDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public Role Rol { get; set; }
    }
}