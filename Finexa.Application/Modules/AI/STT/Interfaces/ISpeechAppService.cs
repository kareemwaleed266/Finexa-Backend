using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Finexa.Application.Modules.AI.STT.Interfaces
{
    public interface ISpeechAppService
    {
        Task<string> ConvertSpeechAsync(IFormFile file);
    }
}
