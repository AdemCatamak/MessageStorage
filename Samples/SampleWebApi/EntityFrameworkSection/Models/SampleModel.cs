using System;

namespace SampleWebApi.EntityFrameworkSection.Models
{
    public class SampleModel
    {
        public long Id { get; private set; }
        public string Text { get; private set; }
        public DateTime CreatedOn { get; private set; }

        public SampleModel(string text)
            : this(default, text, DateTime.UtcNow)
        {
        }

        public SampleModel(long id, string text, DateTime createdOn)
        {
            Id = id;
            Text = text;
            CreatedOn = createdOn;
        }
    }
}