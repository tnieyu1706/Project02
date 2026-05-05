// using System;
// using System.Reflection;
// using Newtonsoft.Json;
// using Newtonsoft.Json.Linq;
//
// namespace TnieYuPackage.Utils
// {
//     public class GamePropertyConverter : JsonConverter
//     {
//         public override bool CanConvert(Type objectType)
//         {
//             return true; // apply cho mọi type (có thể filter nếu muốn)
//         }
//
//         public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//         {
//             if (value == null)
//             {
//                 writer.WriteNull();
//                 return;
//             }
//
//             var type = value.GetType();
//             var obj = new JObject();
//
//             // Fields
//             var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//             foreach (var field in fields)
//             {
//                 if (!field.IsDefined(typeof(GamePropertyAttribute), true)) continue;
//
//                 var fieldValue = field.GetValue(value);
//                 obj[field.Name] = fieldValue != null
//                     ? JToken.FromObject(fieldValue, serializer)
//                     : JValue.CreateNull();
//             }
//
//             // Properties
//             var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//             foreach (var prop in props)
//             {
//                 if (!prop.CanRead) continue;
//                 if (!prop.IsDefined(typeof(GamePropertyAttribute), true)) continue;
//
//                 var propValue = prop.GetValue(value);
//                 obj[prop.Name] = propValue != null
//                     ? JToken.FromObject(propValue, serializer)
//                     : JValue.CreateNull();
//             }
//
//             obj.WriteTo(writer);
//         }
//
//         public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
//             JsonSerializer serializer)
//         {
//             var obj = JObject.Load(reader);
//
//             // tạo instance (support subclass / interface nếu bạn truyền type cụ thể vào Deserialize)
//             var instance = existingValue ?? Activator.CreateInstance(objectType);
//
//             var type = instance.GetType();
//
//             // Fields
//             var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//             foreach (var field in fields)
//             {
//                 if (!field.IsDefined(typeof(GamePropertyAttribute), true)) continue;
//
//                 if (!obj.TryGetValue(field.Name, out var token)) continue;
//
//                 var value = token.ToObject(field.FieldType, serializer);
//                 field.SetValue(instance, value);
//             }
//
//             // Properties
//             var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//             foreach (var prop in props)
//             {
//                 if (!prop.CanWrite) continue;
//                 if (!prop.IsDefined(typeof(GamePropertyAttribute), true)) continue;
//
//                 if (!obj.TryGetValue(prop.Name, out var token)) continue;
//
//                 var value = token.ToObject(prop.PropertyType, serializer);
//                 prop.SetValue(instance, value);
//             }
//
//             return instance;
//         }
//     }
// }