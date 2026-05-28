using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.Models;
using MongoDB.Bson;

namespace FrameworkDriver_Api.src.Projections
{
    public class ListRegistersProjection : ClientModel
    {
        public  List<RegisterModel>? Registers { get; set; }
    }

}