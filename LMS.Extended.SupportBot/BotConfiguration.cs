using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Extended.SupportBot
{
    public class BotConfiguration
    {
        public string BotToken { get; init; } = default!;

        public long AdminId { get; init; }
    }
}
