using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.Models;

namespace FrameworkDriver_Api.src.Projections
{
    public class RegisterObsCliProjection : RegisterModel
    {
        public List<ClientModel>? Clients { get; set; }
        public ObservationModel? Observation { get; set; }
    }
}