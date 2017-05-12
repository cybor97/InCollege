using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
        FullNameEmpty,
        FullNameIncorrectFormat
    }

    public class Account : DBRecord
    {
        private const decimal DaysInAYear = 365.242M;
        private const int MaxAge = 150;
        public const int OnlineTimeoutSeconds = 10;

        public int GroupID { get; set; } = -1;
        public string UserName { get; set; }
        public string Password { get; set; }
        public AccountType AccountType { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime? LastAction { get; set; }
        public byte[] ProfileImage { get; set; }
        public string FullName { get; set; }
        public bool Approved { get; set; }

        [Ignore]
        [JsonIgnore]
        public bool IsOnline => LastAction != null && DateTime.Now.Subtract(LastAction.Value).Seconds < OnlineTimeoutSeconds;

        [Ignore]
        [JsonIgnore]
        public string AccountTypeString
        {
            get
            {
                if (Enum.IsDefined(typeof(AccountType), AccountType))
                    return StatementTypeStrings[AccountType];
                else return "Не определено";
            }
        }

        static readonly Dictionary<AccountType, string> StatementTypeStrings = new Dictionary<AccountType, string>()
        {
            { AccountType.Guest, "Гость" },
            { AccountType.Student, "Студент" },
            { AccountType.Professor, "Преподаватель" },
            { AccountType.DepartmentHead, "Заведующий отделением" },
            { AccountType.Admin, "Администратор" },
        };

        public static AccountValidationResult Validate(string userName, string password, DateTime birthDate, string fullName)
        {
            TimeSpan age = TimeSpan.Zero;
            if (birthDate != null)
                age = DateTime.Now.Subtract(birthDate);
            return
                string.IsNullOrWhiteSpace(fullName) ? AccountValidationResult.FullNameEmpty :
                !Regex.IsMatch(fullName, "[A-я].* [A-я].* [A-я].*") ? AccountValidationResult.FullNameIncorrectFormat :
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
