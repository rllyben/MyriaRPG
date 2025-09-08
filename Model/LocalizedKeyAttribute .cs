using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyriaRPG.Model
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class LocalizedKeyAttribute : Attribute
    {
        public string Key { get; }
        public LocalizedKeyAttribute(string key) => Key = key;
    }

}
