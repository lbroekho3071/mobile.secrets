using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mobile.Secrets.SecretCollectorStrategies
{
    public class AppSettingsCollectorStrategy : ICollectorStrategy<JObject?>
    {
        private readonly string _path;

        public AppSettingsCollectorStrategy(string path)
        {
            _path = path;
        }

        public Dictionary<string, object> CollectAndMapSecrets()
        {
            var jsonObject = CollectSecrets();

            return MapSecrets(jsonObject);
        }

        public JObject? CollectSecrets()
        {
            var fileStream = File.Open(_path, FileMode.Open);
            using var reader = new StreamReader(fileStream);

            var fileSecrets = reader.ReadToEnd();

            var result = JsonConvert.DeserializeObject(fileSecrets);

            if (!(result is JObject jsonObject))
                return null;

            return jsonObject;
        }

        public Dictionary<string, object> MapSecrets(JObject? collectedSecrets)
        {
            var dictionary = new Dictionary<string, object>();

            if (collectedSecrets == null)
                return dictionary;

            return MapSecretsObject(collectedSecrets, dictionary);
        }

        private Dictionary<string, object> MapSecretsObject(JObject jsonObject, Dictionary<string, object> dictionary)
        {
            foreach (var child in jsonObject)
            {
                var value = child.Value;

                switch (value)
                {
                    case null:
                        continue;
                    case JValue jsonValue:
                        dictionary.Add(FormatPath(jsonValue.Path), jsonValue.Value);
                        break;
                    case JObject childJsonObject:
                        MapSecretsObject(childJsonObject, dictionary);
                        break;
                    case JArray childJsonArray:
                    {
                        var array = MapArray(childJsonArray);
                        dictionary.Add(FormatPath(childJsonArray.Path), array);
                        break;
                    }
                }
            }

            return dictionary;
        }

        private string FormatPath(string path)
            => path.Replace('.', '_');

        private object MapArray(JArray array)
        {
            var count = array.Count;
            var result = new object[count];

            if (count <= 0)
                return result;

            Type? arrayType = null;

            for (int i = 0; i < count; i++)
            {
                var child = array[i];

                if (!(child is JValue jValue))
                    throw new NotSupportedException("Only primary data types are supported as array data type");

                var value = jValue.Value;
                arrayType ??= value?.GetType();

                if (value?.GetType() != arrayType)
                    throw new NotSupportedException("Every element in the supplied array should be of the same type.");

                result[i] = value;
            }

            if (arrayType == null)
                return result;

            var typedArray = Array.CreateInstance(arrayType, count);
            Array.Copy(result, typedArray, count);

            return result;
        }
    }
}