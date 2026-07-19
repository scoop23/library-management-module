namespace LibraryManagementSystem.Models
{
    public class ClearanceRecord
    {
        public string StudentId { get; set; }
        public string Status { get; set; } = "Not Cleared";
        public DateTime LastChecked { get; set; }
    }
}
