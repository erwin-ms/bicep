// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Bicep.Core.Configuration;
using Bicep.Core.Text;
using Bicep.Core.UnitTests.Utils;
using Bicep.LanguageServer.Configuration;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OmniSharp.Extensions.LanguageServer.Protocol;
using ConfigurationManager = Bicep.Core.Configuration.ConfigurationManager;
using IOFileSystem = System.IO.Abstractions.FileSystem;

namespace Bicep.LangServer.UnitTests.Configuration
{
    [TestClass]
    public class JsonEditorTests
    {
        [NotNull]
        public TestContext? TestContext { get; set; }//asdfg

        [TestMethod]
        public void EmptyPaths_Throws()
        {
            Action action = () => TestInsertion(
                "",
                "",
                new { level = "warning" },
                "");
            action.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void AlreadyExists_TopLevelPath()
        {
            TestInsertion(
                @"{
  ""analyzers"": {
    ""core"": {
      ""verbose"": false,
      ""enabled"": true,
      ""rules"": {
        ""no-unused-params"": {
          ""level"": ""info""
        }
      }
    }
  }
}",
                "analyzers",
                "value",
                null);
        }

        [TestMethod]
        public void AlreadyExists_Middle()
        {
            TestInsertion(
                @"{
  ""analyzers"": {
    ""core"": {
      ""verbose"": false,
      ""enabled"": true,
      ""rules"": {
        ""no-unused-params"": {
          ""level"": ""info""
        }
      }
    }
  }
}",
                "analyzers.core.rules.no-unused-params",
                new { level = "warning" },
                null
            );
        }

        [TestMethod]
        public void AlreadyExists_Leaf()
        {
            TestInsertion(
                @"{
  ""analyzers"": {
    ""core"": {
      ""verbose"": false,
      ""enabled"": true,
      ""rules"": {
        ""no-unused-params"": {
          ""level"": ""info""
        }
      }
    }
  }
}",
                "analyzers.core.rules.no-unused-params.level",
                "warning",
                null
            );
        }

        [TestMethod]
        public void InvalidJson_Empty_ShouldReproduceDefaultValue()
        {
            TestInsertion(
                "",
                "analyzers.core.rules.no-unused-params.level",
                "warning",
                @"{
  ""analyzers"": {
    ""core"": {
      ""rules"": {
        ""no-unused-params"": {
          ""level"": ""warning""
        }
      }
    }
  }
}"
            );
        }

        [TestMethod]
        public void InvalidJson_JustWhitespaceAndComments_ShouldAppendDefaultValue()
        {
            TestInsertion(
                @"
                    // Well hello there

                // again",
                "analyzers.core.rules.no-unused-params.level",
                "warning",
                @"{
                    // Well hello there

                // again
  ""analyzers"": {
    ""core"": {
      ""rules"": {
        ""no-unused-params"": {
          ""level"": ""warning""
        }
      }
    }
  }
}");
        }

        [TestMethod]
        public void EmptyJsonObjectNoSpaces()
        {
            TestInsertion(
                @"{}",
                "analyzers.core.rules.no-unused-params.level",
                "warning",
                @"{
  ""analyzers"": {
    ""core"": {
      ""rules"": {
        ""no-unused-params"": {
          ""level"": ""warning""
        }
      }
    }
  }}");
        }

        [TestMethod]
        public void EmptyJsonObjectWithNewline()
        {
            TestInsertion(
                @"{
}",
                "analyzers.core.rules.no-unused-params.level",
                "warning",
                @"{
  ""analyzers"": {
    ""core"": {
      ""rules"": {
        ""no-unused-params"": {
          ""level"": ""warning""
        }
      }
    }
  }
}");
        }

        [TestMethod]
        public void EmptyJson()
        {
            TestInsertion(
                "",
                "analyzers.core.rules.no-unused-params.level",
                "info",
                @"{
  ""analyzers"": {
    ""core"": {
      ""rules"": {
        ""no-unused-params"": {
          ""level"": ""info""
        }
      }
    }
  }
}");
        }

        [TestMethod]
        public void EmptyJsonWithWhitespace()
        {
            TestInsertion(
                @"

",
                "analyzers.core.rules.no-unused-params.level",
                "warning",
                @"

{
  ""analyzers"": {
    ""core"": {
      ""rules"": {
        ""no-unused-params"": {
          ""level"": ""warning""
        }
      }
    }
  }
}");
        }

        private void TestInsertion(string beforeText, string insertionPath, object insertionValue, string? afterText)
        {
            (int line, int column, string text)? insertion =
                new JsonEditor(beforeText).
                    InsertIfNotExist(
                        insertionPath.Split('.').Where(p => p.Length > 0).ToArray(),
                        insertionValue);
            if (afterText is null)
            {
                insertion.Should().BeNull();
            }
            else
            {
                insertion.Should().NotBeNull();
                var newText = JsonEditor.ApplyInsertion(beforeText, insertion!.Value);
                newText.Should().Be(afterText);
            }
        }
    }
}