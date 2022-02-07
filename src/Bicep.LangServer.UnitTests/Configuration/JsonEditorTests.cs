// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
        public void asdfg()
        {
            var json = @"{
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
}";
            (int line, int column, string text)? insertion = new JsonEditor(json).GetObjectPropertyInsertion("analyzers.core.rules", "rule-name", new { level = "warning" });
            insertion.Should().NotBeNull();
            var newText = ApplyInsertion(json, insertion!.Value);
            //insertion.Should().Be((1, 1, "\"hello\""));
#pragma warning disable RS0030 // Do not used banned APIs
            Console.WriteLine(newText);
#pragma warning restore RS0030 // Do not used banned APIs
        }

        private string ApplyInsertion(string text, (int line, int column, string text) insertion)
        {
            ImmutableArray<int> lineStarts = TextCoordinateConverter.GetLineStarts(text);
            int offset = TextCoordinateConverter.GetOffset(lineStarts, insertion.line, insertion.column);
            return text.Substring(0, offset) + insertion.text + text.Substring(offset);
        }
    }
}