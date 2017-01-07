using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.IO;
using System.Linq;

namespace AnalyzerTestApp
{
    internal class Program
    {
        private static void Main()
        {
            var compilation = CreateTestCompilation();

            foreach (var sourceTree in compilation.SyntaxTrees.Where(st => st.FilePath.EndsWith("Mapper.cs")))
            {
                var model = compilation.GetSemanticModel(sourceTree);
                var rewriter = new AutoMapperRewriter(model);
                var newSource = rewriter.Visit(sourceTree.GetRoot());

                if (newSource == sourceTree.GetRoot())
                {
                    continue;
                }

                var extension = Path.GetExtension(sourceTree.FilePath);
                var fileName = sourceTree.FilePath.Substring(0, sourceTree.FilePath.Length - extension.Length);

                File.WriteAllText(fileName + "_rewritten" + extension, newSource.ToFullString(), sourceTree.Encoding);
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
