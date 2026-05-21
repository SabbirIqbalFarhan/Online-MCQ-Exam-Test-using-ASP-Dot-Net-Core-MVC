using System.ComponentModel.DataAnnotations;

namespace Exam_Test.Models
{
    public class Module
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }
    }
}