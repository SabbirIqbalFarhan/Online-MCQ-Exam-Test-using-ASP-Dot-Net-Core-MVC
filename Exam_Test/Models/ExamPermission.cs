namespace Exam_Test.Models
{
    public class ExamPermission
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public bool IsPermitted { get; set; }
        public DateTime GrantedAt { get; set; }
    }
}