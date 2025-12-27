using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace FrameworkDriver_Api.Models
{
    public class DataContext
    {
        public string DefaultConnection { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
    }
}