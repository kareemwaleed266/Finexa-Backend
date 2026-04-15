using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Finexa.Integration.AI.STT
{
    public interface ISpeechToTextService
    {
        Task<string> ConvertToTextAsync(IFormFile file);
    }
}
