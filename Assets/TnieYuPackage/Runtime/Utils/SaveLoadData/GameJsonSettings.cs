using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TnieYuPackage.Utils
{
    public class GameJsonSettings
    {
        public static JsonSerializerSettings Create()
        {
            return new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new Vector2IntConverter(),
                    new StringEnumConverter()
                },

                // tránh loop Unity
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                

                // optional
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
            };
        }

        public static JsonSerializer GetJsonSerializer()
        {
            return JsonSerializer.Create(Create());
        }
    }
}