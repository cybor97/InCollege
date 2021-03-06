﻿using InCollege.Core.Data.Base;
using SQLite;

namespace InCollege.Core.Data
{
    [Table("Configuration")]
    public class ConfigurationParameter :DBRecord
    {
        public string ParameterName { get; set; }
        public string ParameterValue { get; set; }
        public string UIName { get; set; }
        public string UITip { get; set; }
    }
}
