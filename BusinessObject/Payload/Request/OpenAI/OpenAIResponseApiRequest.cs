using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Payload.Request.OpenAI
{
    public class OpenAIResponseApiRequest
    {
        public List<QuestionMarkRequest> Questions { get; set; }
    }
}
