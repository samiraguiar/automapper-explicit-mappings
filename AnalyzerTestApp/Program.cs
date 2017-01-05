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

                var ignoreAllNonExistingFinder = new IgnoreAllNonExistingFinder(model);
                ignoreAllNonExistingFinder.Visit(newSource);

                if (ignoreAllNonExistingFinder.ToBeRemoved.Any())
                {
                    newSource = newSource.RemoveNodes(ignoreAllNonExistingFinder.ToBeRemoved, SyntaxRemoveOptions.KeepNoTrivia);

                    //foreach (var node in ignoreAllNonExistingFinder.ToBeRemoved)
                    //{
                    //    var newNode = newSource.FindNode(node.FullSpan);
                    //    newSource = newSource.RemoveNode(newNode, SyntaxRemoveOptions.KeepNoTrivia);
                    //}
                }

                // TODO: Keep file encoding when saving

                var extension = Path.GetExtension(sourceTree.FilePath);
                var fileName = sourceTree.FilePath.Substring(0, sourceTree.FilePath.Length - extension.Length);

                File.WriteAllText(fileName + "_rewritten" + extension, newSource.ToFullString());
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
