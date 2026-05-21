using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Exam_Test.Models
{
    public class UserAnswer
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }

        public int QuestionId { get; set; }

        [NotMapped]
        public Question? Question { get; set; }

        public string? SelectedAnswer { get; set; }

        public bool IsCorrect { get; set; }

        public int ModuleId { get; set; }
    }
}