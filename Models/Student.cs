namespace LibraryManagementSystem.Models
{
    public class Student
    {
        public string StudentId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Sex { get; set; }
        public string BirthDate { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string CollegeId { get; set; }
        public string ProgramId { get; set; }
        public int YearLevel { get; set; }
        public string Status { get; set; } = "Active";
        public string DateRegistered { get; set; }
    }
}
