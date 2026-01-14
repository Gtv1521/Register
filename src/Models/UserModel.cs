using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace FrameworkDriver_Api.src.Models
{
    public class UserModel
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty; 
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;// pin de inicio de sesion
        public Role Rol { get; set; }
    }


// tipos de rol de usuriros
    public enum Role
    {
        [Display(Name = "Administrador")]
        Administrador,
        [Display(Name = "Usuario")]
        Usuario,
        [Display(Name = "Cliente")]
        Cliente        
    }
}