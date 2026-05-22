using System.ComponentModel.DataAnnotations;

namespace Exam_Test.Models
{
    public class ExamSession
    {
        public int Id { get; set; }

        [Required]
        public string? Title { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}