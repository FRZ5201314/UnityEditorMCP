using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using UnityMcp.Models;

namespace UnityMcp.Utils
{
    public static class JsonUtil
    {
        public static T FromJson<T>(string json)
        {
            var value = MiniJson.Deserialize(json);
            if (typeof(T) == typeof(McpCommandRequest))
            {
                return (T)(object)ToCommandRequest(value as Dictionary<string, object>);
            }

            throw new NotSupportedException("Unsupported JSON target type: " + typeof(T).FullName);
        }

        public static string ToJson(object value)
        {
            return MiniJson.Serialize(value);
        }

        private static McpCommandRequest ToCommandRequest(Dictionary<string, object> dictionary)
        {
            if (dictionary == null)
            {
                return null;
            }

            object raw;
            return new McpCommandRequest
            {
                id = dictionary.TryGetValue("id", out raw) && raw != null ? Convert.ToString(raw) : null,
                command = dictionary.TryGetValue("command", out raw) && raw != null ? Convert.ToString(raw) : null,
                @params = dictionary.TryGetValue("params", out raw) ? raw as Dictionary<string, object> : null
            };
        }

        private static class MiniJson
        {
            public static object Deserialize(string json)
            {
                if (json == null)
                {
                    return null;
                }

                return new Parser(json).ParseValue();
            }

            public static string Serialize(object value)
            {
                var builder = new StringBuilder();
                SerializeValue(builder, value);
                return builder.ToString();
            }

            private sealed class Parser
            {
                private readonly string _json;
                private int _index;

                public Parser(string json)
                {
                    _json = json;
                }

                public object ParseValue()
                {
                    SkipWhitespace();
                    if (_index >= _json.Length)
                    {
                        return null;
                    }

                    var c = _json[_index];
                    if (c == '{')
                    {
                        return ParseObject();
                    }

                    if (c == '[')
                    {
                        return ParseArray();
                    }

                    if (c == '"')
                    {
                        return ParseString();
                    }

                    if (c == 't' || c == 'f')
                    {
                        return ParseBool();
                    }

                    if (c == 'n')
                    {
                        Expect("null");
                        return null;
                    }

                    return ParseNumber();
                }

                private Dictionary<string, object> ParseObject()
                {
                    var result = new Dictionary<string, object>();
                    _index++;
                    while (true)
                    {
                        SkipWhitespace();
                        if (Peek('}'))
                        {
                            _index++;
                            return result;
                        }

                        var key = ParseString();
                        SkipWhitespace();
                        Expect(":");
                        result[key] = ParseValue();
                        SkipWhitespace();
                        if (Peek(','))
                        {
                            _index++;
                            continue;
                        }

                        Expect("}");
                        return result;
                    }
                }

                private List<object> ParseArray()
                {
                    var result = new List<object>();
                    _index++;
                    while (true)
                    {
                        SkipWhitespace();
                        if (Peek(']'))
                        {
                            _index++;
                            return result;
                        }

                        result.Add(ParseValue());
                        SkipWhitespace();
                        if (Peek(','))
                        {
                            _index++;
                            continue;
                        }

                        Expect("]");
                        return result;
                    }
                }

                private string ParseString()
                {
                    Expect("\"");
                    var builder = new StringBuilder();
                    while (_index < _json.Length)
                    {
                        var c = _json[_index++];
                        if (c == '"')
                        {
                            return builder.ToString();
                        }

                        if (c != '\\')
                        {
                            builder.Append(c);
                            continue;
                        }

                        if (_index >= _json.Length)
                        {
                            break;
                        }

                        c = _json[_index++];
                        switch (c)
                        {
                            case '"':
                            case '\\':
                            case '/':
                                builder.Append(c);
                                break;
                            case 'b':
                                builder.Append('\b');
                                break;
                            case 'f':
                                builder.Append('\f');
                                break;
                            case 'n':
                                builder.Append('\n');
                                break;
                            case 'r':
                                builder.Append('\r');
                                break;
                            case 't':
                                builder.Append('\t');
                                break;
                            case 'u':
                                builder.Append((char)int.Parse(_json.Substring(_index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                                _index += 4;
                                break;
                        }
                    }

                    throw new FormatException("Invalid JSON string.");
                }

                private bool ParseBool()
                {
                    if (Peek('t'))
                    {
                        Expect("true");
                        return true;
                    }

                    Expect("false");
                    return false;
                }

                private object ParseNumber()
                {
                    var start = _index;
                    while (_index < _json.Length && "-+0123456789.eE".IndexOf(_json[_index]) >= 0)
                    {
                        _index++;
                    }

                    var text = _json.Substring(start, _index - start);
                    if (text.IndexOf('.') >= 0 || text.IndexOf('e') >= 0 || text.IndexOf('E') >= 0)
                    {
                        return double.Parse(text, CultureInfo.InvariantCulture);
                    }

                    long longValue;
                    if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out longValue))
                    {
                        return longValue >= int.MinValue && longValue <= int.MaxValue ? (object)(int)longValue : longValue;
                    }

                    return 0;
                }

                private void SkipWhitespace()
                {
                    while (_index < _json.Length && char.IsWhiteSpace(_json[_index]))
                    {
                        _index++;
                    }
                }

                private bool Peek(char c)
                {
                    return _index < _json.Length && _json[_index] == c;
                }

                private void Expect(string text)
                {
                    if (_index + text.Length > _json.Length || string.CompareOrdinal(_json, _index, text, 0, text.Length) != 0)
                    {
                        throw new FormatException("Invalid JSON near index " + _index + ".");
                    }

                    _index += text.Length;
                }
            }

            private static void SerializeValue(StringBuilder builder, object value)
            {
                if (value == null)
                {
                    builder.Append("null");
                    return;
                }

                var text = value as string;
                if (text != null)
                {
                    SerializeString(builder, text);
                    return;
                }

                if (value is bool)
                {
                    builder.Append((bool)value ? "true" : "false");
                    return;
                }

                if (IsNumber(value))
                {
                    builder.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
                    return;
                }

                var dictionary = value as IDictionary;
                if (dictionary != null)
                {
                    SerializeDictionary(builder, dictionary);
                    return;
                }

                var enumerable = value as IEnumerable;
                if (enumerable != null)
                {
                    SerializeArray(builder, enumerable);
                    return;
                }

                SerializeObject(builder, value);
            }

            private static void SerializeDictionary(StringBuilder builder, IDictionary dictionary)
            {
                var first = true;
                builder.Append('{');
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (!first)
                    {
                        builder.Append(',');
                    }

                    SerializeString(builder, Convert.ToString(entry.Key));
                    builder.Append(':');
                    SerializeValue(builder, entry.Value);
                    first = false;
                }

                builder.Append('}');
            }

            private static void SerializeArray(StringBuilder builder, IEnumerable array)
            {
                var first = true;
                builder.Append('[');
                foreach (var item in array)
                {
                    if (!first)
                    {
                        builder.Append(',');
                    }

                    SerializeValue(builder, item);
                    first = false;
                }

                builder.Append(']');
            }

            private static void SerializeObject(StringBuilder builder, object value)
            {
                var first = true;
                builder.Append('{');
                foreach (var field in value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (!first)
                    {
                        builder.Append(',');
                    }

                    SerializeString(builder, field.Name);
                    builder.Append(':');
                    SerializeValue(builder, field.GetValue(value));
                    first = false;
                }

                foreach (var property in value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (!property.CanRead || property.GetIndexParameters().Length > 0)
                    {
                        continue;
                    }

                    if (!first)
                    {
                        builder.Append(',');
                    }

                    SerializeString(builder, property.Name);
                    builder.Append(':');
                    SerializeValue(builder, property.GetValue(value, null));
                    first = false;
                }

                builder.Append('}');
            }

            private static void SerializeString(StringBuilder builder, string value)
            {
                builder.Append('"');
                foreach (var c in value)
                {
                    switch (c)
                    {
                        case '"':
                            builder.Append("\\\"");
                            break;
                        case '\\':
                            builder.Append("\\\\");
                            break;
                        case '\b':
                            builder.Append("\\b");
                            break;
                        case '\f':
                            builder.Append("\\f");
                            break;
                        case '\n':
                            builder.Append("\\n");
                            break;
                        case '\r':
                            builder.Append("\\r");
                            break;
                        case '\t':
                            builder.Append("\\t");
                            break;
                        default:
                            builder.Append(c);
                            break;
                    }
                }

                builder.Append('"');
            }

            private static bool IsNumber(object value)
            {
                return value is byte || value is sbyte ||
                       value is short || value is ushort ||
                       value is int || value is uint ||
                       value is long || value is ulong ||
                       value is float || value is double ||
                       value is decimal;
            }
        }
    }
}
