using Newtonsoft.Json;
using SQLite;

namespace InCollege.Core.Data.Base
{
    public abstract class DBRecord
    {
        [PrimaryKey]
        [AutoIncrement]
        public int ID { get; set; } = -1;
        // [NotNull]
        public bool IsLocal { get; set; } = true;
        // [NotNull]
        public bool Modified { get; set; } = true;
        // [NotNull]
        public bool Removed { get; set; } = false;

        [Ignore]
        [JsonIgnore]
        public string StateIconURI
        {
            get
            {
                return Removed ? "RemovedIcon" : IsLocal || Modified ? "ModifiedIcon" : "OkIcon";
            }
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);//TODO:Remove this debug staff.
        }

        public static T DeSerialize<T>(string json) where T : DBRecord
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
