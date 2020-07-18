using System;

namespace AccountWebApi.EntityFrameworkSection.Models
{
    public class AccountModel
    {
        public long Id { get; private set; }
        public string Email { get; private set; }
        public DateTime CreatedOn { get; private set; }

        public AccountModel(string email)
            : this(default, email, DateTime.UtcNow)
        {
        }

        public AccountModel(long id, string email, DateTime createdOn)
        {
            Id = id;
            Email = email;
            CreatedOn = createdOn;
        }
    }
}