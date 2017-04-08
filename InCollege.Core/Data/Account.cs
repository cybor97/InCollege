using InCollege.Core.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InCollege.Core.Data
{
    public enum AccountValidationResult : byte
    {
        OK,
        UserNameEmpty,
        UserNameTooShort,
        PasswordEmpty,
        PasswordTooShort,
        BirthDateUndefined,
        BirthDateAfterNow,
        AgeTooBig,
    }

    public class Account : DBRecord
    {
        private const decimal DaysInAYear = 365.242M;
        private const int MaxAge = 150;

        public virtual int AccountDataID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public AccountType AccountType { get; set; }
        public virtual DateTime BirthDate { get; set; }
        public virtual byte[] ProfileImage { get; set; }
        public string FullName { get; set; }

        public static AccountValidationResult Validate(string userName, string password, DateTime birthDate, string fullName)
        {
            TimeSpan age = TimeSpan.Zero;
            if (birthDate != null)
                age = DateTime.Now.Subtract(birthDate);
            return
                string.IsNullOrWhiteSpace(userName) ? AccountValidationResult.UserNameEmpty :
                userName.Length < 5 ? AccountValidationResult.UserNameTooShort :
                string.IsNullOrWhiteSpace(password) ? AccountValidationResult.PasswordEmpty :
                password.Length < 5 ? AccountValidationResult.PasswordTooShort :
                birthDate == null ? AccountValidationResult.BirthDateUndefined :
                age.Equals(TimeSpan.Zero) ? AccountValidationResult.BirthDateAfterNow :
                (decimal)age.TotalDays / DaysInAYear > MaxAge ? AccountValidationResult.AgeTooBig :
                AccountValidationResult.OK;
        }

        public IEnumerable<Message> GetOutgoing(Message[] messages)
        {
            return messages.Where(m => m.FromID == ID);
        }

        public IEnumerable<Message> Inbox(Message[] messages)
        {
            return messages.Where(m => m.ToID == ID);
        }
    }
}
