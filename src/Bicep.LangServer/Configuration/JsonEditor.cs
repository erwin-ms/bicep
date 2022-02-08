// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Bicep.Core.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bicep.LanguageServer.Configuration
{
    public class JsonEditor
    {
        private string _json;

        public JsonEditor(string json)
        {
            _json = json;
        }

        // If the insertion path already exists, or can't be added (eg array instead of object exists on the path asdfg), returns null
        public (int line, int column, string text)? GetValueInsertionIfNotExist(string[] propertyPaths, object defaultValue)
        {
            if (propertyPaths.Length == 0)
            {
                throw new ArgumentException($"{nameof(propertyPaths)} must not be empty");
            }

            TextReader textReader = new StringReader(_json); //asdfg
            JsonReader jsonReader = new JsonTextReader(textReader);
            // LineInfoHandling.Load ensures line info is saved for all tokens while parsing (requires some additional memory).
            var jObject = JObject.Load(jsonReader, new JsonLoadSettings
            {
                LineInfoHandling = LineInfoHandling.Load,
                CommentHandling = CommentHandling.Load,
                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Ignore,
            });
            if (jObject is null)
            {
                throw new Exception("asdfg");
            }

            JObject? currentObject = jObject;
            List<string> remainingPaths = new(propertyPaths);
            while (remainingPaths.Count > 0)
            {
                string path = remainingPaths[0];
                remainingPaths.RemoveAt(0);
                JToken? nextLevel = currentObject[path];
                if (nextLevel is null)
                {
                    int line = ((IJsonLineInfo)currentObject).LineNumber;
                    int column = ((IJsonLineInfo)currentObject).LinePosition;
                    object insertionValue = defaultValue;
                    remainingPaths.Reverse();
                    foreach (string propertyName in remainingPaths)
                    {
                        dynamic newObject = new { };
                        newObject[propertyName] = insertionValue;
                        insertionValue = newObject;
                    }
                    string newPath = string.Join('.', remainingPaths);
                    string insertionValueAsString = SerializeJsonToString(insertionValue, 1/*asdfg*/);
                    var propertyName2 = "asdfg";
                    return (line, column, $"{propertyName2}:\n{insertionValue}");
                }
                else if (nextLevel is JObject nextObject)
                {
                    currentObject = nextObject;
                }
                else
                {
                    throw new Exception("asdfg");
                }
            }

            return null; //asdfg?
        }

        public static string ApplyInsertion(string text, (int line, int column, string text) insertion)
        {
            var lineStarts = TextCoordinateConverter.GetLineStarts(text);
            int offset = TextCoordinateConverter.GetOffset(lineStarts, insertion.line, insertion.column);
            return text.Substring(0, offset) + insertion.text + text.Substring(offset);
        }

        private string SerializeJsonToString(object value, int indent)
        {
            StringBuilder sb = new StringBuilder();
            TextWriter textWriter = new StringWriter(sb);
            JsonTextWriter jsonWriter = new(textWriter);
            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.IndentChar = ' ';
            jsonWriter.Indentation = 4; //asdfg

            new JsonSerializer().Serialize(jsonWriter, value);

            return sb.ToString();
        }
    }
}

