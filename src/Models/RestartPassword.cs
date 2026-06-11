using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace FrameworkDriver_Api.src.Models
{
    public class RestartPassword
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))] //
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreateAd { get; set; } = DateTime.UtcNow;
        public DateTime Expired => CreateAd.AddHours(1);
    }
}