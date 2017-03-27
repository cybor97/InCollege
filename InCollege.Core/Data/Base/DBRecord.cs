using Newtonsoft.Json;

namespace InCollege.Core.Data.Base
{
    public abstract class DBRecord
    {
        public int ID { get; set; }

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
