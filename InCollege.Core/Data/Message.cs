using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using SQLite;
using System;

namespace InCollege.Core.Data
{
    public class Message : DBRecord
    {
        public int FromID { get; set; }
        public int ToID { get; set; }
        public string MessageText { get; set; }
        /// <summary>
        /// JSON-encoded representation.
        /// First parameter(type) shows data type, second(data) - Base64-encoded data
        /// {
        /// "Type":"Text"
        /// "Data":"SOME_ENCODED_MESS"
        /// }
        /// </summary>
        public string MessageAttachment { get; set; }
        public DateTime MessageDate { get; set; }

        [Ignore]
        [JsonIgnore]
        public string SenderName { get => Sender?.FullName; }

        [Ignore]
        [JsonIgnore]
        public Account Sender
        {
            get => _sender;
            set
            {
                _sender = value;
                FromID = _sender?.ID ?? -1;
            }
        }
        private Account _sender;

        [Ignore]
        [JsonIgnore]
        public string ReceiverName { get => Receiver?.FullName; }

        [Ignore]
        [JsonIgnore]
        public Account Receiver
        {
            get => _receiver;
            set
            {
                _receiver = value;
                ToID = _receiver?.ID ?? -1;
            }
        }
        private Account _receiver;

    }
}
