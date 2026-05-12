namespace Exam_Test.Models
{
    public class Result
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int ModuleId { get; set; }
        public int Correct { get; set; }
        public int Wrong { get; set; }
        public DateTime ExamDate { get; set; }
        public int? SessionId { get; set; }
    }
}