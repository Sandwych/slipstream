using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Newtonsoft.Json;

namespace ObjectServer.Json
{
    public static class PlainJsonConvert
    {
        public static string Generate(object value)
        {
            var debug = Platform.Configuration.Debug;
            var fmt = Formatting.None;
            if (debug)
            {
                fmt = Formatting.Indented;
            }
            var str = Newtonsoft.Json.JsonConvert.SerializeObject(value, fmt);

            return str;
        }

        public static object Parse(string json)
        {
            using (var ss = new StringReader(json))
            {
                return Parse(ss);
            }
        }

        public static object Parse(TextReader tr)
        {
            using (var jreader = new JsonTextReader(tr))
            {
                return ParseInternal(jreader);
            }
        }

        public static object Parse(Stream ins)
        {
            using (var tr = new StreamReader(ins, Encoding.UTF8))
            {
                return Parse(tr);
            }
        }

        public static object Parse(byte[] utf8Buffer)
        {
            using (var ms = new MemoryStream(utf8Buffer, false))
            {
                return Parse(ms);
            }
        }

        private static object ParseInternal(JsonReader reader)
        {
            reader.Read();
            return ReadToken(reader);
        }

        private static object ReadToken(JsonReader reader)
        {
            object result = null;

            switch (reader.TokenType)
            {
                //跳过注释
                case JsonToken.Comment:
                    SkipComment(reader);
                    break;

                case JsonToken.StartObject:
                    result = ReadObject(reader);
                    break;

                case JsonToken.StartArray:
                    result = ReadArray(reader);
                    break;

                //标量
                case JsonToken.Boolean:
                case JsonToken.Bytes:
                case JsonToken.Date:
                case JsonToken.Float:
                case JsonToken.Integer:
                case JsonToken.String:
                    result = reader.Value;
                    break;

                case JsonToken.Null:
                    result = null;
                    break;

                case JsonToken.Undefined:
                case JsonToken.None:
                default:
                    throw new NotSupportedException(
                        "Unsupported JSON token type: " + reader.TokenType.ToString());
            }

            return result;
        }

        private static void SkipComment(JsonReader reader)
        {
            while (reader.Read() && reader.TokenType != JsonToken.Comment)
            {
                //do nothing
            }
        }


        private static Dictionary<string, object> ReadObject(JsonReader reader)
        {
            Dictionary<string, object> propBag = new Dictionary<string, object>();

            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var key = (string)reader.Value;
                    reader.Read();
                    object e = ReadToken(reader);
                    propBag[key] = e;
                    continue;
                }
            }

            return propBag;
        }

        private static object[] ReadArray(JsonReader reader)
        {
            var list = new List<object>();

            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
            {
                object e = ReadToken(reader);
                list.Add(e);
            }

            return list.ToArray();
        }

    }
}
