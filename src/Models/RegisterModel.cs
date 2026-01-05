using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace FrameworkDriver_Api.Models
{
    public class RegisterModel
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        [BsonRepresentation(BsonType.ObjectId)]
        public string IdClient { get; set; } = null!; // referencia al cliente
        public Status StatusRegister { get; set; } // estado del registro
        public DateTime CreatedAt { get; set; } // fecha de creacion el dia que se recive el/los equipos

    }

    public enum Status
    {
        [Display(Name = "Pendiente")]
        Pending,
        [Display(Name = "En Progreso")]
        InProgress,
        [Display(Name = "Completado")]
        Completed,
        [Display(Name = "Cancelado")]
        Cancelled
    }
}