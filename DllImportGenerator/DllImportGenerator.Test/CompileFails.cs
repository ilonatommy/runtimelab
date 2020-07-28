﻿using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

namespace DllImportGenerator.Test
{
    public class CompileFails
    {
        public static IEnumerable<object[]> CodeSnippetsToCompile()
        {
            yield return new object[] { CodeSnippets.UserDefinedPrefixedAttributes, 3 };
        }

        [Theory]
        [MemberData(nameof(CodeSnippetsToCompile))]
        public void ValidateSnippets(string source, int failCount)
        {
            Compilation comp = TestUtils.CreateCompilation(source);
            TestUtils.AssertPreSourceGeneratorCompilation(comp);

            var newComp = TestUtils.RunGenerators(comp, out var generatorDiags, new Microsoft.Interop.DllImportGenerator());
            Assert.Empty(generatorDiags);

            var newCompDiags = newComp.GetDiagnostics();

            // Verify the compilation failed with missing impl.
            int missingImplCount = 0;
            foreach (var diag in newCompDiags)
            {
                if ("CS8795".Equals(diag.Id))
                {
                    missingImplCount++;
                }
            }

            Assert.Equal(failCount, missingImplCount);
        }
    }
}
