using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FrameworkDriver_Api.Models
{
    // observaciones del tecnico / analisis a los equipos del cliente
    public class ObservationModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string IdRegister { get; set; } // referencia al registro dato unico del servicio
        public ObservationType Type { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string IdUser { get; set; } // nombre del tecnico que creo la observacion
        public List<string> Photos { get; set; } // lista de urls de fotos asociadas a la observacion (hallasgos)

    }

    // tipo de observacion
    public enum ObservationType
    {
        Info,
        Warning,
        Error,
        Solution
    }
}