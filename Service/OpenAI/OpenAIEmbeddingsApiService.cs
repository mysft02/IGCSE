using BusinessObject.Model;
using BusinessObject.Payload.Request.OpenAI;
using BusinessObject.Payload.Response.OpenAI;
using Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service.OpenAI
{
    public class OpenAIEmbeddingsApiService
    {
        private readonly OpenAIApiService _openAIApiService;
        private readonly string OpenAIEmbeddingsApiBaseUrl = "https://api.openai.com/v1/embeddings";
        public OpenAIEmbeddingsApiService(OpenAIApiService openAIApiService)
        {
            _openAIApiService = openAIApiService;
        }

        public async Task<OpenAIEmbeddingsApiResponse> EmbedData(Course course)
        {
            try
            {
                var inputObject = CommonUtils.ObjectToString(course);

                if (inputObject.Length > 8000)
                {
                    inputObject = inputObject.Substring(0, 8000) + "...";
                }

                var body = new OpenAIEmbeddingsBody
                {
                    Model = "text-embedding-3-small",
                    Input = inputObject,
                    EncodingFormat = "float"
                };

                var request = OpenApiRequest.Builder()
                    .CallUrl(OpenAIEmbeddingsApiBaseUrl)
                    .Body(body)
                    .Build();

                var response = await _openAIApiService.PostAsync<object, OpenAIEmbeddingsApiResponse>(request, body);

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi EmbedData: {ex.Message}");
            }
        }
    }
}
