using BusinessObject.Payload.Request.OpenAI;
using BusinessObject.Payload.Request.OpenApi;
using BusinessObject.Payload.Response.OpenAI;
using System.Text.Json;

namespace Service.OpenAI
{
    public class TestService
    {
        private readonly OpenAIApiService _openAIApiService;
        private readonly string OpenAIResponseApiBaseUrl = "https://api.openai.com/v1/responses";

        public TestService(OpenAIApiService openAIApiService)
        {
            _openAIApiService = openAIApiService;
        }

        public async Task<List<GradedQuestion>> MarkTest(List<Question> questions)
        {
            try
            {
                var contentText = string.Join("\n", questions.Select(q =>
                $"Question: {q.QuestionText}\nAnswer: {q.Answer}\nRight answer: {q.RightAnswer}\n"));

                var input = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "input_text",
                                text = $"Chấm điểm danh sách câu hỏi sau và trả về JSON gồm(question, answer, rightAnswer, comment):\n{contentText}"
                            }
                        }
                    }
                };

                var body = new OpenAIBody
                {
                    Model = "gpt-4.1-mini",
                    Input = input,
                    Temperature = 0.5,
                    MaxTokens = 200
                };

                var request = OpenApiRequest.Builder()
                    .CallUrl(OpenAIResponseApiBaseUrl)
                    .Body(body)
                    .Build();

                var response = await _openAIApiService.PostAsync<OpenAIBody, OpenAIResponse>(request, body);

                if (response?.Output == null || response.Output.Count == 0)
                    return new List<GradedQuestion>();

                // 🧩 Ghép output text
                var outputText = string.Join("\n",
                    response.Output
                        .SelectMany(o => o.Content)
                        .Where(c => c.Type == "output_text")
                        .Select(c => c.Text));

                if (string.IsNullOrWhiteSpace(outputText))
                {
                    throw new Exception("OpenAI trả về rỗng (outputText empty)");
                }

                // 🧠 Parse JSON từ text output — cố gắng bóc đúng phần JSON nếu model có lẫn text
                string jsonCandidate = outputText.Trim();

                // Nếu chuỗi có thừa text, cắt lấy đoạn JSON đầu tiên (mảng hoặc object)
                int idxArrayStart = jsonCandidate.IndexOf('[');
                int idxObjStart = jsonCandidate.IndexOf('{');
                int start = -1;
                char closing = '\0';
                if (idxArrayStart >= 0 && (idxObjStart < 0 || idxArrayStart < idxObjStart))
                {
                    start = idxArrayStart;
                    closing = ']';
                }
                else if (idxObjStart >= 0)
                {
                    start = idxObjStart;
                    closing = '}';
                }

                if (start > 0)
                {
                    // Tìm vị trí đóng khớp
                    int depth = 0;
                    for (int i = start; i < jsonCandidate.Length; i++)
                    {
                        char ch = jsonCandidate[i];
                        if (ch == (closing == ']' ? '[' : '{')) depth++;
                        if (ch == closing)
                        {
                            depth--;
                            if (depth == 0)
                            {
                                jsonCandidate = jsonCandidate.Substring(start, i - start + 1);
                                break;
                            }
                        }
                    }
                }

                // 🧠 Parse JSON từ jsonCandidate
                var result = JsonSerializer.Deserialize<List<GradedQuestion>>(jsonCandidate,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result ?? new List<GradedQuestion>();
            }
            catch (JsonException jex)
            {
                throw new Exception($"OpenAI trả về JSON không hợp lệ: {jex.Message}");
            }
            catch (HttpRequestException hex)
            {
                throw new Exception($"Lỗi gọi OpenAI: {hex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi xử lý MarkTest: {ex.Message}");
            }
        }
    }
}
