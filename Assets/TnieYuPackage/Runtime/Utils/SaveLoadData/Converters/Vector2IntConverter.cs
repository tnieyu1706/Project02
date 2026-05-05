using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace TnieYuPackage.Utils
{
    public class Vector2IntConverter : JsonConverter<Vector2Int>
    {
        public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("x");
            writer.WriteValue(value.x);

            writer.WritePropertyName("y");
            writer.WriteValue(value.y);

            writer.WriteEndObject();
        }

        public override Vector2Int ReadJson(JsonReader reader, Type objectType, Vector2Int existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);

            int x = obj["x"].Value<int>();
            int y = obj["y"].Value<int>();

            return new Vector2Int(x, y);
        }
    }
}