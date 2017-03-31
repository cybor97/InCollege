using InCollege.Core.Data.Base;
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
        public virtual string MessageAttachment { get; set; }
        public virtual DateTime MessageDate { get; set; }
    }
}
