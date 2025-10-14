using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Payload.Response.OpenAI
{
    public class QuestionMarkResponse
    {
        public string IsCorrect { get; set; }
        public string Comment { get; set; }
    }
}
