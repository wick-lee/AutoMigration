using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.Loader;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Wick.AutoMigration.Exceptions;

namespace Wick.AutoMigration.Helper;

public static class MigrationHelper
{
    public static IEnumerable<Assembly> DefaultMigrationAssemblies => GetAssemblies();

    private static IEnumerable<Assembly> GetAssemblies()
    {
        var result = new List<Assembly>()
        {
            typeof(DbContext).Assembly,
            typeof(DbContextAttribute).Assembly,
            typeof(ModelSnapshot).Assembly,
            typeof(AssemblyTargetedPatchBandAttribute).Assembly,
            typeof(Expression).Assembly,
            typeof(object).Assembly,
            typeof(JsonElement).Assembly
        };

        var doMainAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        var runtimeAssembly = doMainAssemblies.SingleOrDefault(a => a.GetName().Name == "System.Runtime");
        if (runtimeAssembly != null)
        {
            result.Add(runtimeAssembly);
        }

        var standardAssembly = doMainAssemblies.SingleOrDefault(a => a.GetName().Name == "netstandard");
        if (standardAssembly != null)
        {
            result.Add(standardAssembly);
        }

        return result.Distinct();
    }

    public static T CompileSnapshot<T>(IEnumerable<Assembly> assemblies, string source)
    {
        var sharpParseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
        var compilationOptions =
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithAssemblyIdentityComparer(
                DesktopAssemblyIdentityComparer.Default);
        var compilation = CSharpCompilation.Create("Dynamic",
            new[] { SyntaxFactory.ParseSyntaxTree(source, sharpParseOptions) },
            assemblies.Select(assembly => MetadataReference.CreateFromFile(assembly.Location)), compilationOptions);

        using var memoryStream = new MemoryStream();
        var emitResult = compilation.Emit(memoryStream);

        if (!emitResult.Success)
        {
            throw new MigrationException(
                $"Compilation efcore model snapshot failed, error message: {string.Join("\r\n", emitResult.Diagnostics.Select(d => d.GetMessage()))}");
        }

        memoryStream.Seek(0, SeekOrigin.Begin);
        var context = new AssemblyLoadContext(null, true);
        var assembly = context.LoadFromStream(memoryStream);
        var modelType = assembly.DefinedTypes.Single(type => typeof(T).IsAssignableFrom(type));

        return (T)Activator.CreateInstance(modelType);
    }
}