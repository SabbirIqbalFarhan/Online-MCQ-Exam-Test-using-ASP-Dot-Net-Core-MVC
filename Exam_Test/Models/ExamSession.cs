namespace Exam_Test.Models
{
    public class ExamSession
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}