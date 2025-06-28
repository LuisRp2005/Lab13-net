namespace Lab12.Request
{
    public class CreateEnrollmentRequest
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
