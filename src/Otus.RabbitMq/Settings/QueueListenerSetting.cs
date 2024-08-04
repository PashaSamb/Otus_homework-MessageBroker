using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus.RabbitMq.Settings
{
    public class QueueListenerSetting
    {
        public bool Enabled { get; set; } = true;
        public int Period { get; set; } = (int)TimeSpan.FromSeconds(15).TotalMilliseconds;
    }
}
