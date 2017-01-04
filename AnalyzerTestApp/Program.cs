using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.IO;
using System.Linq;

namespace AnalyzerTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Compilation test = CreateTestCompilation();

            foreach (SyntaxTree sourceTree in test.SyntaxTrees.Where(st => st.FilePath.EndsWith("Mapper.cs")))
            {
                SemanticModel model = test.GetSemanticModel(sourceTree);
                AutoMapperRewritter rewriter = new AutoMapperRewritter(model);

                SyntaxNode newSource = rewriter.Visit(sourceTree.GetRoot());

                if (newSource != sourceTree.GetRoot())
                {
                    File.WriteAllText(sourceTree.FilePath, newSource.ToFullString());
                }
            }
        }

        private static Compilation CreateTestCompilation()
        {
            var msWorkspace = MSBuildWorkspace.Create();
            var project = msWorkspace.OpenProjectAsync(@"C:\Users\Samir.MAC\Documents\Workspace\AnalyzerTestApp\Mappers\Mappers.csproj").Result;
            return project.GetCompilationAsync().Result;
        }
    }
}
