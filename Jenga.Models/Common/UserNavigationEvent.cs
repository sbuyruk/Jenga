using System;
using Jenga.Models.Sistem;

namespace Jenga.Models.Common
{
    public sealed class UserNavigationEvent : BaseModel
    {
        public int PersonelId { get; set; }

        public int? PresenceSessionId { get; set; }

        public string? Url { get; set; }

        public DateTime OccurredAt { get; set; }
    }
}
