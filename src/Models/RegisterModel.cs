using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace FrameworkDriver_Api.Models
{
    public class RegisterModel
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))] //
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string RegistroNumber { get; set; } = string.Empty; // Número incremental formateado (ej: REG-000001)

        [BsonRepresentation(BsonType.ObjectId)]
        public string IdClient { get; set; } = null!; // referencia al cliente
        [BsonRepresentation(BsonType.ObjectId)]
        public string IdCompany { get; set; } = null!; // referencia a la empresa
        [BsonRepresentation(BsonType.ObjectId)]
        public string IdUser { get; set; } = null!; // referencia al usuario que crea el registro
        public string Tecnico { get; set; } = "Desconocido"; // nombre del tecnico asignado
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Status StatusRegister { get; set; } = Status.EnProgreso;// estado del registro
        public string UrlQr { get; set; } = string.Empty;
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Antisipo { get; set; } = 0; // total a pagar por el cliente

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalPagar { get; set; } = 0; // total a pagar por el cliente

        public string IdQr { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } // fecha de creacion el dia que se recive el/los equipos
    }

    public enum Status
    {
        [Display(Name = "Pendiente")]
        Pendiente,
        [Display(Name = "En Progreso")]
        EnProgreso,
        [Display(Name = "Completado")]
        Completado,
        [Display(Name = "Entregado")]
        Entregado,
        [Display(Name = "Cancelado")]
        Cancelado
    }
}