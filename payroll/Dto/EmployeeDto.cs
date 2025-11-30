namespace payroll.Dto
{
    public class EmployeeDto
    {
        public EmployeeDto() { }
        public string lastName { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public DateOnly dateOfBirth { get; set; }
        public decimal dailyRate { get; set; }
        public string workingDays { get; set; }
    }
}
