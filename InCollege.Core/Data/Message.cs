using InCollege.Core.Data.Base;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Core.Data
{
    public class Message : DBRecord
    {
        [Column("FromID")]
        public virtual Account From { get; set; }
        [Column("ToID")]
        public virtual Account To { get; set; }
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
