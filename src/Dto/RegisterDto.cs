using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FrameworkDriver_Api.src.Dto
{
    public class RegisterDto
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string IdClient { get; set; } = string.Empty; // referencia al cliente
        public Status StatusRegister { get; set; } // estado del registro 
    }
}