using System.Reflection;

namespace Wick.AutoMigration.Helper;

public static class AssemblyHelper
{
    public static List<Type>? GetImplementTypes(Type baseType)
    {
        var assemblies = AppDomain.CurrentDomain.GetReferenceAssemblies();
        var result = new List<Type>(assemblies.Count);

        foreach (var type in assemblies.SelectMany(assembly => assembly.GetTypes()))
            if (type.IsClass && !type.IsAbstract)
            {
                if (type.GetInterfaces().Any(@interface => @interface == baseType ||
                                                           (@interface.Name == baseType.Name &&
                                                            @interface.GetGenericTypeDefinition() == baseType)))
                    result.Add(type);
            }
            else if (baseType.IsClass)
            {
                if (baseType.IsGenericType && baseType.IsGenericTypeDefinition)
                {
                    var tempType = type;
                    while (tempType != null)
                    {
                        if (tempType.IsGenericType && type.GetGenericTypeDefinition() == baseType)
                        {
                            result.Add(type);
                            break;
                        }

                        tempType = tempType.BaseType;
                    }
                }
                else if (baseType.IsAssignableFrom(type))
                {
                    result.Add(type);
                }
            }

        return result.Distinct().ToList();
    }

    public static List<Type>? GetImplementTypes(Type baseType, params Assembly[] assemblies)
    {
        var result = new List<Type>(assemblies.Length);

        foreach (var type in assemblies.SelectMany(assembly => assembly.GetTypes()))
            if (type.IsClass && !type.IsAbstract)
            {
                if (type.GetInterfaces().Any(@interface => @interface == baseType ||
                                                           (@interface.Name == baseType.Name &&
                                                            @interface.GetGenericTypeDefinition() == baseType)))
                    result.Add(type);
            }
            else if (baseType.IsClass)
            {
                if (baseType.IsGenericType && baseType.IsGenericTypeDefinition)
                {
                    var tempType = type;
                    while (tempType != null)
                    {
                        if (tempType.IsGenericType && type.GetGenericTypeDefinition() == baseType)
                        {
                            result.Add(type);
                            break;
                        }

                        tempType = tempType.BaseType;
                    }
                }
                else if (baseType.IsAssignableFrom(type))
                {
                    result.Add(type);
                }
            }

        return result.Distinct().ToList();
    }

    public static List<Assembly> GetReferenceAssemblies(this AppDomain appDomain)
    {
        var result = new List<Assembly>();

        foreach (var assembly in appDomain.GetAssemblies()) GetReferenceAssemblies(assembly, result);

        return result;
    }

    private static void GetReferenceAssemblies(Assembly assembly, List<Assembly> assemblies)
    {
        foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
        {
            var loadedAssembly = Assembly.Load(referencedAssembly);
            if (assemblies.Contains(loadedAssembly))
                continue;

            assemblies.Add(loadedAssembly);
            GetReferenceAssemblies(loadedAssembly, assemblies);
        }
    }
}