using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Dto;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace FrameworkDriver_Api.src.Models
{
    public class SessionModel : NavDataDto
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;
        [BsonRepresentation(BsonType.ObjectId)]
        public string IdCompany { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        // public string Theme { get; set; } = "light";
    }
}