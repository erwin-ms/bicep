// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Bicep.Core;
using Bicep.Core.UnitTests;
using Bicep.Core.UnitTests.Assertions;
using Bicep.Core.UnitTests.Utils;
using Bicep.LanguageServer.Handlers;
using Bicep.LanguageServer.Telemetry;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace Bicep.LangServer.UnitTests.Handlers
{
    [TestClass]
    public class BicepConfigureLinterRuleCommandHandlerTests
    {
        [NotNull]
        public TestContext? TestContext { get; set; }

        private static readonly MockRepository Repository = new(MockBehavior.Strict);
        private static readonly ISerializer Serializer = Repository.Create<ISerializer>().Object;
        private static readonly ITelemetryProvider TelemetryProvider = BicepTestConstants.CreateMockTelemetryProvider().Object;

        public static Mock<ITextDocumentLanguageServer> CreateMockDocument() //asdfg
        {
            var document = Repository.Create<ITextDocumentLanguageServer>();
            //document //asdfg
            //    .Setup(m => m.SendNotification(It.IsAny<MediatR.IRequest>()))
            //    .Callback<MediatR.IRequest>((p) => callback((PublishDiagnosticsParams)p))
            //    .Verifiable();

            return document;
        }


        [TestMethod]
        public void ConfigureLinterRule_WithInvalidBicepConfig_ShouldThrow()
        {
            string bicepConfig = @"{
              ""analyzers"": {
                ""core"": {
                  ""verbose"": false,
                  ""enabled"": true,
                  ""rules"": {
                    ""no-unused-params"": {
                      ""level"": ""warning""
            }";

            var document = CreateMockDocument();
            var server = BicepCompilationManagerHelper.CreateMockServer(document).Object;
            BicepConfigureLinterRuleCommandHandler BicepConfigureLinterRuleHandler = new(Serializer, server, TelemetryProvider);
            Action ConfigureLinterRule = () => BicepConfigureLinterRuleHandler.ConfigureLinterRule(bicepConfig, "no-unused-params");

            ConfigureLinterRule.Should().Throw<Exception>().WithMessage("File bicepconfig.json already exists and is invalid. If overwriting the file is intended, delete it manually and retry disable linter rule lightBulb option again");
        }

        [TestMethod]
        public void asdfg1()
        {
            string bicepConfig = @"{ // hi
  ""cloud"": { //there



                         ""currentProfile"": ""AzureCloud"",
    ""profiles"": {
                ""AzureCloud"": {
                    ""resourceManagerEndpoint"": ""https://management.azure.com"",
        ""activeDirectoryAuthority"": ""https://login.microsoftonline.com""
                },
      ""AzureChinaCloud"": {
                    ""resourceManagerEndpoint"": ""https://management.chinacloudapi.cn"",
        ""activeDirectoryAuthority"": ""https://login.chinacloudapi.cn""
      },
      ""AzureUSGovernment"": {
                    ""resourceManagerEndpoint"": ""https://management.usgovcloudapi.net"",
        ""activeDirectoryAuthority"": ""https://login.microsoftonline.us""
      }
            },
    ""credentialPrecedence"": [
      ""AzureCLI"",
      ""AzurePowerShell""
    ]
  },
  ""moduleAliases"": {
    ""ts"": {},
    ""br"": {}
  },
  ""analyzers"": {
    ""core"": {
      ""verbose"": false,
      ""enabled"": true,
      ""rules"": {
        // hello
        ""explicit-values-for-loc-params"": {
          ""level"": ""warning""
        },
        ""no-hardcoded-env-urls"": {
          ""level"": ""warning"",
          ""disallowedhosts"": [
            ""gallery.azure.com"",
            ""management.core.windows.net"",
            ""management.azure.com"",
            ""database.windows.net"",
            ""core.windows.net"",
            ""login.microsoftonline.com"",
            ""graph.windows.net"",
            ""trafficmanager.net"",
            ""datalake.azure.net"",
            ""azuredatalakestore.net"",
            ""azuredatalakeanalytics.net"",
            ""vault.azure.net"",
            ""api.loganalytics.io"",
            ""asazure.windows.net"",
            ""region.asazure.windows.net"",
            ""batch.core.windows.net""
          ],
          ""excludedhosts"": [
            ""schema.management.azure.com""
          ]
    }
}
    }
  }
}";
            TextReader textReader = new StringReader(bicepConfig);
            JsonReader jsonReader = new JsonTextReader(textReader);
            // LineInfoHandling.Load ensures line info is saved for all tokens while parsing (requires some additional memory).
            var jObject = JObject.Load(jsonReader, new JsonLoadSettings
            {
                LineInfoHandling = LineInfoHandling.Load,
                CommentHandling = CommentHandling.Load,
                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Ignore,
            });


            StringBuilder sb = new StringBuilder();
            TextWriter textWriter = new StringWriter(sb);
            MyWriter myWriter = new(textWriter);

            var s = JsonSerializer.Create(new JsonSerializerSettings()
            {

            });
            s.Serialize(myWriter, jObject);

            //myWriter.WriteValue(jObject);
#pragma warning disable RS0030 // Do not used banned APIs
            Console.WriteLine(sb.ToString());
#pragma warning restore RS0030 // Do not used banned APIs
        }

        class MyWriter : JsonTextWriter
        {
            public MyWriter(TextWriter textWriter) :
                base(textWriter)
            {
            }
        }

        [TestMethod]
        public void asdfg2()
        {
            string bicepConfig = @"{
  ""analyzers"": { // comment1
    ""core"": { // comment2
      ""verbose"": false, // comment3
      ""enabled"": true, // comment3
      ""rules"": { // comment4
        ""no-unused-params"": {
          ""level"": ""warning""
        }
      }
    }
  }
}";

            var document = CreateMockDocument();
            var server = BicepCompilationManagerHelper.CreateMockServer(document).Object;
            BicepConfigureLinterRuleCommandHandler BicepConfigureLinterRuleHandler = new(Serializer, server, TelemetryProvider);
            Action ConfigureLinterRule = () => BicepConfigureLinterRuleHandler.ConfigureLinterRule(bicepConfig, "no-unused-params");

            ConfigureLinterRule.Should().Throw<Exception>().WithMessage("File bicepconfig.json already exists and is invalid. If overwriting the file is intended, delete it manually and retry disable linter rule lightBulb option again");
        }

#if false //asdfg
        [TestMethod]
        public void ConfigureLinterRule_WithRuleEnabledInBicepConfig_ShouldTurnOffRule()
        {
            string bicepConfigFileContents = @"{
  ""analyzers"": {
    ""core"": {
      ""verbose"": false,
      ""enabled"": true,
      ""rules"": {
        ""no-unused-params"": {
          ""level"": ""warning""
        }
      }
    }
  }
}";
            string actual = BicepConfigureLinterRuleHandler.ConfigureLinterRule(bicepConfigFileContents, "no-unused-params");

            actual.Should().BeEquivalentToIgnoringNewlines(@"{
  ""analyzers"": {
    ""core"": {
      ""verbose"": false,
      ""enabled"": true,
      ""rules"": {
        ""no-unused-params"": {
          ""level"": ""off""
        }
      }
    }
  }
}");
        }

        [TestMethod]
        public void ConfigureLinterRule_WithRuleDisabledInBicepConfig_DoesNothing()
        {
            string bicepConfigFileContents = @"{
  ""analyzers"": {
    ""core"": {
      ""verbose"": false,
      ""enabled"": true,
      ""rules"": {
        ""no-unused-params"": {
          ""level"": ""off""
        }
      }
    }
  }
}";
            string actual = BicepConfigureLinterRuleHandler.ConfigureLinterRule(bicepConfigFileContents, "no-unused-params");

            actual.Should().BeEquivalentToIgnoringNewlines(@"{
  ""analyzers"": {
    ""core"": {
      ""verbose"": false,
      ""enabled"": true,
      ""rules"": {
        ""no-unused-params"": {
          ""level"": ""off""
        }
      }
    }
  }
}");
        }

        [TestMethod]
        public void ConfigureLinterRule_WithNoRuleInBicepConfig_ShouldAddAnEntryInBicepConfig()
        {
            string bicepConfigFileContents = @"{
  ""analyzers"": {
    ""core"": {
      ""verbose"": false,
      ""enabled"": true,
      ""rules"": {
      }
    }
  }
}";
            string actual = BicepConfigureLinterRuleHandler.ConfigureLinterRule(bicepConfigFileContents, "no-unused-params");

            actual.Should().BeEquivalentToIgnoringNewlines(@"{
  ""analyzers"": {
    ""core"": {
      ""verbose"": false,
      ""enabled"": true,
      ""rules"": {
        ""no-unused-params"": {
          ""level"": ""off""
        }
      }
    }
  }
}");
        }

        [TestMethod]
        public void ConfigureLinterRule_WithNoLevelPropertyInRule_ShouldAddAnEntryInBicepConfigAndTurnOffRule()
        {
            string bicepConfigFileContents = @"{
  ""analyzers"": {
    ""core"": {
      ""verbose"": false,
      ""enabled"": true,
      ""rules"": {
        ""no-unused-params"": {
        }
      }
    }
  }
}";
            string actual = BicepConfigureLinterRuleHandler.ConfigureLinterRule(bicepConfigFileContents, "no-unused-params");

            actual.Should().BeEquivalentToIgnoringNewlines(@"{
  ""analyzers"": {
    ""core"": {
      ""verbose"": false,
      ""enabled"": true,
      ""rules"": {
        ""no-unused-params"": {
          ""level"": ""off""
        }
      }
    }
  }
}");
        }

        [TestMethod]
        public void ConfigureLinterRule_WithNoRulesNode_ShouldAddAnEntryInBicepConfigAndTurnOffRule()
        {
            string bicepConfigFileContents = @"{
  ""analyzers"": {
    ""core"": {
      ""verbose"": false,
      ""enabled"": true
    }
  }
}";
            string actual = BicepConfigureLinterRuleHandler.ConfigureLinterRule(bicepConfigFileContents, "no-unused-params");

            actual.Should().BeEquivalentToIgnoringNewlines(@"{
  ""analyzers"": {
    ""core"": {
      ""verbose"": false,
      ""enabled"": true,
      ""rules"": {
        ""no-unused-params"": {
          ""level"": ""off""
        }
      }
    }
  }
}");
        }

        [TestMethod]
        public void ConfigureLinterRule_WithOnlyCurlyBraces_ShouldUseDefaultConfigAndTurnOffRule()
        {
            string actual = BicepConfigureLinterRuleHandler.ConfigureLinterRule("{}", "no-unused-params");

            actual.Should().BeEquivalentToIgnoringNewlines(@"{
  ""cloud"": {
    ""currentProfile"": ""AzureCloud"",
    ""profiles"": {
      ""AzureCloud"": {
        ""resourceManagerEndpoint"": ""https://management.azure.com"",
        ""activeDirectoryAuthority"": ""https://login.microsoftonline.com""
      },
      ""AzureChinaCloud"": {
        ""resourceManagerEndpoint"": ""https://management.chinacloudapi.cn"",
        ""activeDirectoryAuthority"": ""https://login.chinacloudapi.cn""
      },
      ""AzureUSGovernment"": {
        ""resourceManagerEndpoint"": ""https://management.usgovcloudapi.net"",
        ""activeDirectoryAuthority"": ""https://login.microsoftonline.us""
      }
    },
    ""credentialPrecedence"": [
      ""AzureCLI"",
      ""AzurePowerShell""
    ]
  },
  ""moduleAliases"": {
    ""ts"": {},
    ""br"": {}
  },
  ""analyzers"": {
    ""core"": {
      ""verbose"": false,
      ""enabled"": true,
      ""rules"": {
        ""no-hardcoded-env-urls"": {
          ""level"": ""warning"",
          ""disallowedhosts"": [
            ""gallery.azure.com"",
            ""management.core.windows.net"",
            ""management.azure.com"",
            ""database.windows.net"",
            ""core.windows.net"",
            ""login.microsoftonline.com"",
            ""graph.windows.net"",
            ""trafficmanager.net"",
            ""datalake.azure.net"",
            ""azuredatalakestore.net"",
            ""azuredatalakeanalytics.net"",
            ""vault.azure.net"",
            ""api.loganalytics.io"",
            ""asazure.windows.net"",
            ""region.asazure.windows.net"",
            ""batch.core.windows.net""
          ],
          ""excludedhosts"": [
            ""schema.management.azure.com""
          ]
        },
        ""no-unused-params"": {
          ""level"": ""off""
        }
      }
    }
  }
}");
        }

        [TestMethod]
        public void GetBicepConfigFilePathAndContents_WithInvalidBicepConfigFilePath_ShouldCreateBicepConfigFileUsingDefaultSettings()
        {
            var bicepPath = FileHelper.SaveResultFile(TestContext, "main.bicep", @"param storageAccountName string = 'test'");
            DocumentUri documentUri = DocumentUri.FromFileSystemPath(bicepPath);

            (string actualBicepConfigFilePath, string actualBicepConfigContents) = BicepConfigureLinterRuleHandler.GetBicepConfigFilePathAndContents(documentUri, "no-unused-params", string.Empty);

            var directoryContainingSourceFile = Path.GetDirectoryName(documentUri.GetFileSystemPath());
            string expectedBicepConfigFilePath = Path.Combine(directoryContainingSourceFile!, LanguageConstants.BicepConfigurationFileName);

            actualBicepConfigFilePath.Should().Be(expectedBicepConfigFilePath);
            actualBicepConfigContents.Should().BeEquivalentToIgnoringNewlines(@"{
  ""cloud"": {
    ""currentProfile"": ""AzureCloud"",
    ""profiles"": {
      ""AzureCloud"": {
        ""resourceManagerEndpoint"": ""https://management.azure.com"",
        ""activeDirectoryAuthority"": ""https://login.microsoftonline.com""
      },
      ""AzureChinaCloud"": {
        ""resourceManagerEndpoint"": ""https://management.chinacloudapi.cn"",
        ""activeDirectoryAuthority"": ""https://login.chinacloudapi.cn""
      },
      ""AzureUSGovernment"": {
        ""resourceManagerEndpoint"": ""https://management.usgovcloudapi.net"",
        ""activeDirectoryAuthority"": ""https://login.microsoftonline.us""
      }
    },
    ""credentialPrecedence"": [
      ""AzureCLI"",
      ""AzurePowerShell""
    ]
  },
  ""moduleAliases"": {
    ""ts"": {},
    ""br"": {}
  },
  ""analyzers"": {
    ""core"": {
      ""verbose"": false,
      ""enabled"": true,
      ""rules"": {
        ""no-hardcoded-env-urls"": {
          ""level"": ""warning"",
          ""disallowedhosts"": [
            ""gallery.azure.com"",
            ""management.core.windows.net"",
            ""management.azure.com"",
            ""database.windows.net"",
            ""core.windows.net"",
            ""login.microsoftonline.com"",
            ""graph.windows.net"",
            ""trafficmanager.net"",
            ""datalake.azure.net"",
            ""azuredatalakestore.net"",
            ""azuredatalakeanalytics.net"",
            ""vault.azure.net"",
            ""api.loganalytics.io"",
            ""asazure.windows.net"",
            ""region.asazure.windows.net"",
            ""batch.core.windows.net""
          ],
          ""excludedhosts"": [
            ""schema.management.azure.com""
          ]
        },
        ""no-unused-params"": {
          ""level"": ""off""
        }
      }
    }
  }
}");
        }

        [TestMethod]
        public void GetBicepConfigFilePathAndContents_WithNonExistentBicepConfigFile_ShouldCreateBicepConfigFileUsingDefaultSettings()
        {
            var bicepPath = FileHelper.SaveResultFile(TestContext, "main.bicep", @"param storageAccountName string = 'test'");
            DocumentUri documentUri = DocumentUri.FromFileSystemPath(bicepPath);
            (string actualBicepConfigFilePath, string actualBicepConfigContents) = BicepConfigureLinterRuleHandler.GetBicepConfigFilePathAndContents(documentUri, "no-unused-params", @"\nonExistent\bicepconfig.json");

            var directoryContainingSourceFile = Path.GetDirectoryName(documentUri.GetFileSystemPath());
            string expectedBicepConfigFilePath = Path.Combine(directoryContainingSourceFile!, LanguageConstants.BicepConfigurationFileName);
            actualBicepConfigFilePath.Should().Be(expectedBicepConfigFilePath);
            actualBicepConfigContents.Should().BeEquivalentToIgnoringNewlines(@"{
  ""cloud"": {
    ""currentProfile"": ""AzureCloud"",
    ""profiles"": {
      ""AzureCloud"": {
        ""resourceManagerEndpoint"": ""https://management.azure.com"",
        ""activeDirectoryAuthority"": ""https://login.microsoftonline.com""
      },
      ""AzureChinaCloud"": {
        ""resourceManagerEndpoint"": ""https://management.chinacloudapi.cn"",
        ""activeDirectoryAuthority"": ""https://login.chinacloudapi.cn""
      },
      ""AzureUSGovernment"": {
        ""resourceManagerEndpoint"": ""https://management.usgovcloudapi.net"",
        ""activeDirectoryAuthority"": ""https://login.microsoftonline.us""
      }
    },
    ""credentialPrecedence"": [
      ""AzureCLI"",
      ""AzurePowerShell""
    ]
  },
  ""moduleAliases"": {
    ""ts"": {},
    ""br"": {}
  },
  ""analyzers"": {
    ""core"": {
      ""verbose"": false,
      ""enabled"": true,
      ""rules"": {
        ""no-hardcoded-env-urls"": {
          ""level"": ""warning"",
          ""disallowedhosts"": [
            ""gallery.azure.com"",
            ""management.core.windows.net"",
            ""management.azure.com"",
            ""database.windows.net"",
            ""core.windows.net"",
            ""login.microsoftonline.com"",
            ""graph.windows.net"",
            ""trafficmanager.net"",
            ""datalake.azure.net"",
            ""azuredatalakestore.net"",
            ""azuredatalakeanalytics.net"",
            ""vault.azure.net"",
            ""api.loganalytics.io"",
            ""asazure.windows.net"",
            ""region.asazure.windows.net"",
            ""batch.core.windows.net""
          ],
          ""excludedhosts"": [
            ""schema.management.azure.com""
          ]
        },
        ""no-unused-params"": {
          ""level"": ""off""
        }
      }
    }
  }
}");
        }

        [TestMethod]
        public void GetBicepConfigFilePathAndContents_WithValidBicepConfigFile_ShouldReturnUpdatedBicepConfigFile()
        {
            string testOutputPath = Path.Combine(TestContext.ResultsDirectory, Guid.NewGuid().ToString());
            string bicepConfigFileContents = @"{
  ""analyzers"": {
    ""core"": {
      ""verbose"": false,
      ""enabled"": true,
      ""rules"": {
        ""no-unused-params"": {
          ""level"": ""warning""
        }
      }
    }
  }
}";
            string bicepConfigFilePath = FileHelper.SaveResultFile(TestContext, "bicepconfig.json", bicepConfigFileContents, testOutputPath, Encoding.UTF8);

            DocumentUri documentUri = DocumentUri.FromFileSystemPath("/path/to/main.bicep");
            (string actualBicepConfigFilePath, string actualBicepConfigContents) = BicepConfigureLinterRuleHandler.GetBicepConfigFilePathAndContents(documentUri, "no-unused-params", bicepConfigFilePath);

            actualBicepConfigFilePath.Should().Be(bicepConfigFilePath);
            actualBicepConfigContents.Should().BeEquivalentToIgnoringNewlines(@"{
  ""analyzers"": {
    ""core"": {
      ""verbose"": false,
      ""enabled"": true,
      ""rules"": {
        ""no-unused-params"": {
          ""level"": ""off""
        }
      }
    }
  }
}");
    }
#endif
    }
}
