namespace PANOPA.Models
{
    public class Delay
    {
        public int Id { get; set; }
        public string? ProcessId { get; set; }
        public string? ProjectName { get; set; }
        public string? PanoNo { get; set; }
        public string? NameSurname { get; set; }
        public string? ProcessName { get; set; }
        public string? UserName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? DelayTime { get; set; }
        public string? PausedReason { get; set;}
    }
}
