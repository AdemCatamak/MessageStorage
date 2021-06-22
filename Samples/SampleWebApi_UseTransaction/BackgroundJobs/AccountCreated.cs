using System;

namespace SampleWebApi_UseTransaction.BackgroundJobs
{
    public class AccountCreated
    {
        public Guid AccountId { get; private set; }
        public string Email { get; private set; }
        public DateTime CreatedOn { get; private set; }

        public AccountCreated(Guid accountId, string email, DateTime createdOn)
        {
            AccountId = accountId;
            Email = email;
            CreatedOn = createdOn;
        }
    }
}