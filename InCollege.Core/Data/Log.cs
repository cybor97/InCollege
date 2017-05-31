using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using SQLite;
using System;

namespace InCollege.Core.Data
{
    public class Log : DBRecord
    {
        public virtual int AccountID { get; set; }
        public virtual DateTime LogDate { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }

        [Ignore]
        [JsonIgnore]
        public string UserView => _user == null || (string.IsNullOrEmpty(_user.FullNameInitials) || string.IsNullOrEmpty(_user.UserName)) ? "?" : $"{_user.FullNameInitials}({_user.UserName})";

        [Ignore]
        [JsonIgnore]
        public Account User
        {
            get => _user;
            set
            {
                _user = value;
                AccountID = value?.ID ?? -1;
            }
        }
        private Account _user;
    }
}
