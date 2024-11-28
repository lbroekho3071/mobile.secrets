using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mobile.Secrets.SecretCollectorStrategies
{

    public class EnvironmentCollectorStrategy : ICollectorStrategy<IEnumerable<KeyValuePair<string, string>>>
    {
        private readonly string _secretPrefix;

        public EnvironmentCollectorStrategy(string secretPrefix)
        {
            _secretPrefix = secretPrefix;
        }

        public Dictionary<string, object> CollectAndMapSecrets()
        {
            var prefixedSecrets = CollectSecrets();

            return MapSecrets(prefixedSecrets);
        }

        public IEnumerable<KeyValuePair<string, string>> CollectSecrets()
        {
            var environmentVariables = Environment.GetEnvironmentVariables();

            foreach (var environmentVariable in environmentVariables)
            {
                if (!(environmentVariable is DictionaryEntry entry))
                    continue;

                if (!(entry.Key is string entryKey) || !entryKey.StartsWith(_secretPrefix))
                    continue;

                yield return new KeyValuePair<string, string>(entryKey, entry.Value as string);
            }
        }

        public Dictionary<string, object> MapSecrets(IEnumerable<KeyValuePair<string, string>> collectedSecrets)
        {
            var prefixedSecrets = new Dictionary<string, object>();

            foreach (var entry in collectedSecrets)
            {
                var value = MapValue(entry.Value);
                var trimmedKey = entry.Key.TrimStart(_secretPrefix.ToCharArray());

                prefixedSecrets.Add(trimmedKey, value);
            }

            return prefixedSecrets;
        }

        private object MapValue(object? value)
        {
            if (!(value is string stringValue))
                return "";

            if (!stringValue.StartsWith("[") || !stringValue.EndsWith("]"))
                return MapPrimitiveValue(stringValue);

            var arrayValues = stringValue
                .Split(new []{',', '[', ']'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .ToArray();

            return MapArray(arrayValues);
        }

        private object MapPrimitiveValue(string stringValue)
        {
            if (Boolean.TryParse(stringValue, out var boolValue))
                return boolValue;

            if (long.TryParse(stringValue, out var longValue))
                return longValue;

            return stringValue;
        }

        // TODO: Figure out a way how to combine this method and the AppSettingsCollectorStrategy method
        private object MapArray(string[] array)
        {
            var count = array.Length;
            var result = new object[count];

            if (count <= 0)
                return result;

            Type? arrayType = null;

            for (int i = 0; i < count; i++)
            {
                var child = array[i];
                var mappedValue = MapPrimitiveValue(child);

                arrayType ??= mappedValue.GetType();

                if (mappedValue.GetType() != arrayType)
                    throw new NotSupportedException("Every element in the supplied array should be of the same type.");

                if (mappedValue is string stringValue)
                    mappedValue = stringValue.Trim('"');

                result[i] = mappedValue;
            }

            if (arrayType == null)
                return result;

            var typedArray = Array.CreateInstance(arrayType, count);
            Array.Copy(result, typedArray, count);

            return typedArray;
        }
    }
}