using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Interfaces
{
    public interface IIndexInitializer
    {
        Task InitializeIndexesAsync();
    }
}