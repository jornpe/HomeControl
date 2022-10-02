using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api.Services
{
    public interface IIotHubService
    {
        public Task<List<Twin>> GetTwinsAsync();
    }
}
