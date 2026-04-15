using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finexa.Application.Modules.AI.STT.Interfaces;
using Finexa.Integration.AI.STT;
using Microsoft.AspNetCore.Http;

namespace Finexa.Application.Modules.AI.STT.Services
{
    public class SpeechAppService : ISpeechAppService
    {
        private readonly ISpeechToTextService _speechService;

        public SpeechAppService(ISpeechToTextService speechService)
        {
            _speechService = speechService;
        }

        public async Task<string> ConvertSpeechAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("Invalid audio file");

            return await _speechService.ConvertToTextAsync(file);
        }
    }
}
