namespace Exam_Test.Models
{
    public class Question
    {
        public int Id { get; set; }

        public string? QuestionText { get; set; }
        public string? ImagePath { get; set; }

        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }

        public string? CorrectAnswer { get; set; }

        public int ModuleId { get; set; }
        public Module? Module { get; set; }
    }
}