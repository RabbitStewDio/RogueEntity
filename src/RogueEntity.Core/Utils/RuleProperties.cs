using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RogueEntity.Core.Utils
{
    /// <summary>
    ///  A primitive Key-Value store of single value properties.
    /// </summary>
    public class RuleProperties
    {
        readonly Dictionary<string, List<string>> data;

        public RuleProperties()
        {
            data = new Dictionary<string, List<string>>();
        }

        RuleProperties(Dictionary<string, List<string>> data)
        {
            this.data = data;
        }

        public void DefineProperty(string key, float value)
        {
            var val = Convert.ToString(value, CultureInfo.InvariantCulture);
            DefineProperty(key, val);
        }

        public void DefineProperty(string key, string value)
        {
            if (!data.TryGetValue(key, out var list))
            {
                list = new List<string>();
                data[key] = list;
            }

            list.Clear();
            list.Add(value);
        }

        public void DefineProperty(string key, int value)
        {
            var val = Convert.ToString(value, CultureInfo.InvariantCulture);
            DefineProperty(key, val);
        }

        public void DefineProperty(string key, bool value = true)
        {
            var val = Convert.ToString(value, CultureInfo.InvariantCulture);
            DefineProperty(key, val);
        }

        public void AddProperty(string key, float value)
        {
            var val = Convert.ToString(value, CultureInfo.InvariantCulture);
            AddProperty(key, val);
        }

        public void AddProperty(string key, string value)
        {
            if (!data.TryGetValue(key, out var list))
            {
                list = new List<string>();
                data[key] = list;
            }

            list.Add(value);
        }

        public void AddProperty(string key, int value)
        {
            var val = Convert.ToString(value, CultureInfo.InvariantCulture);
            AddProperty(key, val);
        }

        public void AddProperty(string key, bool value = true)
        {
            var val = Convert.ToString(value, CultureInfo.InvariantCulture);
            AddProperty(key, val);
        }

        public bool TryGetValue(string key, out string value)
        {
            if (data.TryGetValue(key, out var list) &&
                list.Count > 0)
            {
                value = list[0];
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetValue(string key, out bool value)
        {
            if (data.TryGetValue(key, out var list) &&
                list.Count > 0)
            {
                value = Convert.ToBoolean(list[0], CultureInfo.InvariantCulture);
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetValue(string key, out int value)
        {
            if (data.TryGetValue(key, out var list) &&
                list.Count > 0)
            {
                value = Convert.ToInt32(list[0], CultureInfo.InvariantCulture);
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetValue(string key, out float value)
        {
            if (data.TryGetValue(key, out var list) &&
                list.Count > 0)
            {
                value = Convert.ToSingle(list[0], CultureInfo.InvariantCulture);
                return true;
            }

            value = default;
            return false;
        }

        public ReadOnlyListWrapper<string> TryGetValues(string key)
        {
            if (data.TryGetValue(key, out var list) &&
                list.Count > 0)
            {
                return list;
            }

            return new ReadOnlyListWrapper<string>(new List<string>());
        }

        public List<float> TryGetValues(string key, List<float> valueReceiver)
        {
            valueReceiver.Clear();
            if (data.TryGetValue(key, out var list) &&
                list.Count > 0)
            {
                foreach (var l in list)
                {
                    valueReceiver.Add(Convert.ToSingle(l, CultureInfo.InvariantCulture));
                }
            }

            return valueReceiver;
        }

        public List<int> TryGetValues(string key, List<int> valueReceiver)
        {
            valueReceiver.Clear();
            if (data.TryGetValue(key, out var list) &&
                list.Count > 0)
            {
                foreach (var l in list)
                {
                    valueReceiver.Add(Convert.ToInt32(l, CultureInfo.InvariantCulture));
                }
            }

            return valueReceiver;
        }

        public List<bool> TryGetValues(string key, List<bool> valueReceiver)
        {
            valueReceiver.Clear();
            if (data.TryGetValue(key, out var list) &&
                list.Count > 0)
            {
                foreach (var l in list)
                {
                    valueReceiver.Add(Convert.ToBoolean(l, CultureInfo.InvariantCulture));
                }
            }

            return valueReceiver;
        }

        public bool HasFlag(string key, bool v = default)
        {
            if (TryGetValue(key, out bool val))
            {
                return val;
            }

            return v;
        }

        public RuleProperties Copy()
        {
            var dataCopy = new Dictionary<string, List<string>>(data.Count);
            foreach (var e in data)
            {
                dataCopy.Add(e.Key, e.Value.ToList());
            }
            return new RuleProperties(dataCopy);
        }
    }
}