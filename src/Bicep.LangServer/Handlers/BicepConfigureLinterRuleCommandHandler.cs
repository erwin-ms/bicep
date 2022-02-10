// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bicep.Core;
using Bicep.Core.Configuration;
using Bicep.LanguageServer.Configuration;
using Bicep.LanguageServer.Telemetry;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Bicep.LanguageServer.Handlers
{
    public class BicepConfigureLinterRuleCommandHandler : ExecuteTypedCommandHandlerBase<DocumentUri, string, string>
    {
        private readonly string DefaultBicepConfig;
        private readonly ILanguageServerFacade server;
        private readonly ITelemetryProvider telemetryProvider;

        public BicepConfigureLinterRuleCommandHandler(ISerializer serializer, ILanguageServerFacade server, ITelemetryProvider telemetryProvider)
            : base(LanguageConstants.ConfigureLinterRuleCommandName, serializer)
        {
            DefaultBicepConfig = DefaultBicepConfigHelper.GetDefaultBicepConfig();
            this.server = server;
            this.telemetryProvider = telemetryProvider;
        }

        //asdfg why is bicepConfigFilePath allowed to be empty?
        public override async Task<Unit> Handle(DocumentUri documentUri, string ruleCode, string bicepConfigFilePath, CancellationToken cancellationToken)
        {
            //(string updatedBicepConfigFilePath, string bicepConfigContents) = GetBicepConfigFilePathAndContents(documentUri, code, bicepConfigFilePath);

            //File.WriteAllText(updatedBicepConfigFilePath, bicepConfigContents);

            if (string.IsNullOrEmpty(bicepConfigFilePath))
            {
                var directoryContainingSourceFile = Path.GetDirectoryName(documentUri.GetFileSystemPath()) ??
                    throw new ArgumentException("Unable to find directory information");
                bicepConfigFilePath = Path.Combine(directoryContainingSourceFile, LanguageConstants.BicepConfigurationFileName);
            }

            if (!File.Exists(bicepConfigFilePath))
            {
                // use server  workspace.workspaceEdit?
                File.WriteAllText(bicepConfigFilePath, DefaultBicepConfig); //asdfg error handling
            }

            await SetCursor(ruleCode, bicepConfigFilePath);
            //bicepConfigContents = ConfigureLinterRule(string.Empty, code);
            //asdfg where create?
            //var directoryContainingSourceFile = Path.GetDirectoryName(documentUri.GetFileSystemPath()) ??
            //    throw new ArgumentException("Unable to find directory information");

            //bicepConfigFilePath = Path.Combine(directoryContainingSourceFile, LanguageConstants.BicepConfigurationFileName);
            //File.WriteAllText(bicepConfigFilePath, bicepConfigContents);
            //return (0, 0, 0); //(bicepConfigFilePath, ConfigureLinterRule(string.Empty, code));


            //{
            //    bicepConfigContents = File.ReadAllText(bicepConfigFilePath); //asdfg errors?
            //}
            //else


            //await SetCursor(ruleCode, bicepConfigFilePath);
            return Unit.Value;
        }

        //public (int, int, int) GetBicepConfigFilePathAndContents(DocumentUri documentUri, string code, string bicepConfigFilePath)
        //{
        //    if (File.Exists(bicepConfigFilePath))
        //    {
        //        var bicepConfigContents = File.ReadAllText(bicepConfigFilePath); //asdfg errors?

        //        string jsonString = bicepConfigContents;

        //        // Convert the JSON string to a JObject:
        //        TextReader textReader = File.OpenText(bicepConfigFilePath);
        //        JsonReader jsonReader = new JsonTextReader(textReader);
        //        // LineInfoHandling.Load ensures line info is saved for all tokens while parsing (requires some additional memory).
        //        var jObject = JObject.Load(jsonReader, new JsonLoadSettings { LineInfoHandling = LineInfoHandling.Load });

        //        // Select a nested property using a single string:
        //        JToken? jToken = jObject?.SelectToken($"analyzers.core.rules.{code}.level");
        //        if (jObject is not null && jToken is not null)
        //        {
        //            var a = jToken as IJsonLineInfo;
        //            return (a.LineNumber, a.LinePosition - jToken.ToString().Length, jToken.ToString().Length); //asdfg

        //            // Update the value of the property: 
        //            // jToken.Replace("myNewPassword123");
        //            // // Convert the JObject back to a string:
        //            // string updatedJsonString = jObject.ToString();
        //            // File.WriteAllText(bicepConfigFilePath, updatedJsonString);
        //        }

        //        return (0, 0, 0); //(bicepConfigFilePath, ConfigureLinterRule(bicepConfigContents, code));
        //    }
        //    else
        //    {
        //        //asdfg where create?
        //        var directoryContainingSourceFile = Path.GetDirectoryName(documentUri.GetFileSystemPath()) ??
        //            throw new ArgumentException("Unable to find directory information");

        //        bicepConfigFilePath = Path.Combine(directoryContainingSourceFile, LanguageConstants.BicepConfigurationFileName);
        //        return (0, 0, 0); //(bicepConfigFilePath, ConfigureLinterRule(string.Empty, code));
        //    }
        //}

        public async Task SetCursor(/*DocumentUri documentUri, */string code, string bicepConfigFilePath)
        {
            await server.Window.ShowDocument(new ShowDocumentParams() //asdfg return?failure?
            {
                Uri = DocumentUri.File(bicepConfigFilePath),
                // Selection = new Range(line - 1, column - 1, line - 1, column - 1 + length),
                TakeFocus = true
            });


            //if (File.Exists(bicepConfigFilePath))
            //{
            //var bicepConfigContents = File.ReadAllText(bicepConfigFilePath); //asdfg errors?

            //string jsonString = bicepConfigContents;

            // Convert the JSON string to a JObject:
            TextReader textReader = File.OpenText(bicepConfigFilePath); // open properly
            JsonReader jsonReader = new JsonTextReader(textReader);
            //while (jsonReader.Read())
            //{
            //    var a = jsonReader.Value;
            //}
            //jsonReader = new JsonTextReader(textReader);
            // LineInfoHandling.Load ensures line info is saved for all tokens while parsing (requires some additional memory).
            var jObject = JObject.Load(jsonReader, new JsonLoadSettings
            {
                LineInfoHandling = LineInfoHandling.Load,
                CommentHandling = CommentHandling.Load,
                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Ignore,
            });

            // Select a nested property using a single string:
            JToken? jToken = jObject?.SelectToken($"analyzers.core.rules.{code}.level");
            if (jObject is not null && jToken is not null)
            {
                var a = jToken as IJsonLineInfo;
                // return (a.LineNumber, a.LinePosition - jToken.ToString().Length, jToken.ToString().Length); //asdfg
                int line = a.LineNumber;
                int column = a.LinePosition - jToken.ToString().Length;
                int length = jToken.ToString().Length;
                // GetBicepConfigFilePathAndContents(documentUri, code, bicepConfigFilePath);
                await server.Window.ShowDocument(new ShowDocumentParams() //asdfg return?failure?
                {
                    Uri = DocumentUri.File(bicepConfigFilePath),
                    Selection = new Range(line - 1, column - 1, line - 1, column - 1 + length),
                    TakeFocus = true
                });

                // Update the value of the property: 
                // jToken.Replace("myNewPassword123");
                // // Convert the JObject back to a string:
                // string updatedJsonString = jObject.ToString();
                // File.WriteAllText(bicepConfigFilePath, updatedJsonString);
            }
            else
            {//asdfg
                string json = File.ReadAllText(bicepConfigFilePath);
                (int line, int column, string text)? insertion = new JsonEditor(json).InsertIfNotExist(
                    new string[] { "analyzers", "core", "rules", code },
                    new { level = "warning" });

                if (insertion.HasValue)
                {
                    var (line, column, insertText) = insertion.Value;
                    await server.Workspace.ApplyWorkspaceEdit(
                        new ApplyWorkspaceEditParams()
                        {
                            Label = "asdfg",
                            Edit = new WorkspaceEdit()
                            {
                                DocumentChanges = new Container<WorkspaceEditDocumentChange>(
                                    new WorkspaceEditDocumentChange(
                                        new TextDocumentEdit()
                                        {
                                            TextDocument = new OptionalVersionedTextDocumentIdentifier() { Uri = DocumentUri.File(bicepConfigFilePath) },
                                            Edits = new TextEditContainer(
                                                new TextEdit()
                                                {
                                                    Range = new Range()
                                                    {
                                                        Start = new Position(line, column),
                                                        End = new Position(line, column)
                                                    },
                                                    NewText = insertText
                                                }
                                            )
                                        }
                                    )
                                )
                            }
                        }
                    );
                }
            }
        }

        public string ConfigureLinterRule(string bicepConfigContents, string code)
        {
            try
            {
                TextReader textReader = new StringReader(bicepConfigContents);
                JsonReader jsonReader = new JsonTextReader(textReader);
                // LineInfoHandling.Load ensures line info is saved for all tokens while parsing (requires some additional memory).
                var jObject = JObject.Load(jsonReader, new JsonLoadSettings
                {
                    LineInfoHandling = LineInfoHandling.Load,
                    CommentHandling = CommentHandling.Load,
                    DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Ignore,
                });
                if (jObject is JObject root &&
                    root["analyzers"] is JObject analyzers &&
                    analyzers["core"] is JObject core)
                {
                    var a = root.Children()[0];
                    var b = a.Values();

                    if (core["rules"] is JObject rules)
                    {
                        if (rules[code] is JObject rule)
                        {
                            if (rule.ContainsKey("level"))
                            {
                                rule["level"] = "warning";//asdfg
                            }
                            else
                            {
                                rule.Add("level", "warning"); //asdfg
                            }
                        }
                        else
                        {
                            SetRuleLevelToDefault(rules, code);
                        }
                    }
                    else
                    {
                        JObject rule = new JObject();
                        SetRuleLevelToDefault(rule, code);

                        core.Add("rules", rule);
                    }

                    return root.ToString(Formatting.Indented);
                }

                if (JsonConvert.DeserializeObject(DefaultBicepConfig) is JObject defaultBicepConfigRoot &&
                    defaultBicepConfigRoot["analyzers"]?["core"]?["rules"] is JObject defaultRules)
                {
                    SetRuleLevelToDefault(defaultRules, code);

                    return defaultBicepConfigRoot.ToString();
                }

                return string.Empty;
            }
            catch (Exception)
            {
                //asdfg show full path, better error
                throw new Exception("File bicepconfig.json already exists and is invalid. If overwriting the file is intended, delete it manually and retry disable linter rule lightBulb option again");
            }
        }

        private void SetRuleLevelToDefault(JObject jObject, string code)
        {
            jObject.Add(code, JToken.Parse(@"{
  ""level"": ""warning""
}"));
        }
    }
}
