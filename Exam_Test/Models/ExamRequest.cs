namespace Exam_Test.Models
{
    public class ExamRequest
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public DateTime RequestedAt { get; set; }
        public string? Status { get; set; } // "Pending", "Approved", "Rejected"
    }
}