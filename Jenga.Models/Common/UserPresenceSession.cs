using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.Sistem;

namespace Jenga.Models.Common
{
    public class UserPresenceSession : BaseModel
    {
        public int PersonelId { get; set; }

        public string? UserName { get; set; }

        public string? DisplayName { get; set; }

        public string CircuitId { get; set; } = string.Empty;

        public DateTime ConnectedAt { get; set; }

        public DateTime LastSeen { get; set; }

        public DateTime? DisconnectedAt { get; set; }

        public string? UserAgent { get; set; }

        public string? RemoteIp { get; set; }
    }
}
