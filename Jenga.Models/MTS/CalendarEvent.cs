namespace Jenga.Models.MTS
{
    public class CalendarEvent
    {
        public int id { get; set; }
        public string title { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string color { get; set; }
        public string textColor { get; set; }
        public string backgroundColor { get; set; }
        public string borderColor { get; set; }
        public bool allDay { get; set; }
        public string purpose { get; set; }
        public string url { get; set; }
        public string state { get; set; }
        public bool startEditable { get; set; }
        public string className { get; set; } = "mtsEvent";
        public string description { get; set; }
    }
}
