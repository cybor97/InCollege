using Newtonsoft.Json;
using SQLite;

namespace InCollege.Core.Data.Base
{
    public abstract class DBRecord
    {
        [PrimaryKey]
        public int ID { get; set; } = -1;
        [NotNull]
        public bool IsLocal { get; set; } = true;
        [NotNull]
        public bool Modified { get; set; } = true;

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);//TODO:Remove this debug staff.
        }

        public static DBRecord DeSerialize<T>(string json) where T : DBRecord
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
