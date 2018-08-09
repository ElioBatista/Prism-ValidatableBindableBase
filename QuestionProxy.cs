using System;

namespace Apollo.Survey.Views
{
    public class QuestionProxy
    {
        public Guid Id { get; set; }
        public long QuestionNo { get; set; }
        public string QuestionText { get; set; }
        public string QuestionCategory { get; set; }
        public string QuestionType { get; set; }
        public string Frequency { get; set; }
        public bool Notification { get; set; }
        public bool Compliance { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public string LastModifiedUser { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }
}
