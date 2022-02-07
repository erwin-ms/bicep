// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;
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

        // If the insertion path already exists, null is returned
        public (int line, int column, string text)? GetValueInsertionIfNotExist(string paths, string propertyName, object defaultValue)
        {
            TextReader textReader = new StringReader(_json); //asdfg
            JsonReader jsonReader = new JsonTextReader(textReader);
            // LineInfoHandling.Load ensures line info is saved for all tokens while parsing (requires some additional memory).
            var jObject = JObject.Load(jsonReader, new JsonLoadSettings
            {
                LineInfoHandling = LineInfoHandling.Load,
                CommentHandling = CommentHandling.Load,
                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Ignore,
            });


            //JToken? jToken = jObject?.SelectToken($"analyzers.core.rules.{code}.level");
            JToken? jToken = jObject?.SelectToken($"analyzers.core.rules");
            if (jToken is null)
            {
                int line = ((IJsonLineInfo)jToken).LineNumber;
                int column = ((IJsonLineInfo)jToken).LinePosition;
                string valueJson = SerializeJsonToString(value, 1/*asdfg*/);
                return (line, column, $"{propertyName}:\n{valueJson}");
            }
            else
            {
                return null;
            }
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

