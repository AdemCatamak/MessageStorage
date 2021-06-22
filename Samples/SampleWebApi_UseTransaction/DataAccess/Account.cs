using System;

namespace SampleWebApi_UseTransaction.DataAccess
{
    public class Account
    {
        public Guid AccountId { get; private set; }
        public string Email { get; private set; }
        public DateTime CreatedOn { get; private set; }


        public Account(string email)
            : this(Guid.NewGuid(), email, DateTime.UtcNow)
        {
        }

        public Account(Guid accountId, string email, DateTime createdOn)
        {
            AccountId = accountId;
            Email = email;
            CreatedOn = createdOn;
        }
    }
}