using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace FrameworkDriver_Api.src.Models
{
    public class AdvertenciaModel
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        [BsonRepresentation(BsonType.ObjectId)]
        public string IdCompany { get; set; } = string.Empty;
        public string Advertencia { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
    }
}