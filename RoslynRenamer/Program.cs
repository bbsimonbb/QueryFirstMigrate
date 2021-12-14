using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Build.Locator;

namespace RoslynRenamer
{
    public class Program
    {
        static async Task  Main(string[] args)
        {
            var solutionPath = args[0];
            MSBuildLocator.RegisterDefaults();
            using (var ws = MSBuildWorkspace.Create())
            {
                var sln = await ws.OpenSolutionAsync(solutionPath);

                foreach (var p in sln.Projects)
                {
                    System.Console.WriteLine($"{p.Name}");
                    foreach (var d in p.Documents)
                    {
                        System.Console.WriteLine($"  {d.Name} - {d.FilePath}");
                        var m = await d.GetSemanticModelAsync();
                        var r = await d.GetSyntaxRootAsync();
                        var n = r.DescendantNodes();

                        var mes = n.OfType<MethodDeclarationSyntax>();
                        foreach (var ms in mes)
                        {
                            var s = m.GetDeclaredSymbol(ms);
                            System.Console.WriteLine($"     {ms.ToString()} - {s.Name}");
                            sln = await Renamer.RenameSymbolAsync(sln, s, $"{s.Name}-dash", sln.Options);
                        }

                        var classes = n.OfType<ClassDeclarationSyntax>();
                        foreach (var c in classes)
                        {
                            var s = m.GetDeclaredSymbol(c);
                            System.Console.WriteLine($"     {c.ToString()} - {s.Name}");
                            sln = await Renamer.RenameSymbolAsync(sln, s, $"{s.Name}-dash", sln.Options);
                        }
                    }
                };
                ws.TryApplyChanges(sln);
            }
            System.Console.WriteLine("done!!");
        }
    }
}

