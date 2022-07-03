using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;
using UndertaleModLib;

namespace HoloCure.ModLoader.API
{
    /// <summary>
    ///     Handles the resolution and handling of modded assemblies.
    /// </summary>
    public class ModAssemblyResolver
    {
        // Raw code adapted from: https://www.codeproject.com/Articles/1194332/Resolving-Assemblies-in-NET-Core
        // Adapted under The Code Project Open License (CPOL) 1.02: https://www.codeproject.com/info/cpol10.aspx
        public class Resolver
        {
            public readonly Assembly Assembly;

            private readonly List<Resolver> Dependencies = new();
            private readonly DependencyContext DependencyContext;
            private readonly CompositeCompilationAssemblyResolver AssemblyResolver;
            private readonly AssemblyLoadContext? AssemblyLoadContext;

            public Resolver(string assembly) {
                Assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assembly);

                DependencyContext = DependencyContext.Load(Assembly);
                AssemblyResolver = new CompositeCompilationAssemblyResolver(new ICompilationAssemblyResolver[]
                {
                    new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(assembly)),
                    new ReferenceAssemblyPathResolver(),
                    new PackageCompilationAssemblyResolver()
                });

                AssemblyLoadContext = AssemblyLoadContext.GetLoadContext(Assembly);
            }

            public void AddDependency(Resolver resolver) {
                Dependencies.Add(resolver);
            }

            public void Subscribe() {
                if (AssemblyLoadContext is null) return;
                AssemblyLoadContext.Resolving += HandleLoadAssembly;
            }

            public void Unsubscribe() {
                if (AssemblyLoadContext is null) return;
                AssemblyLoadContext.Resolving += HandleLoadAssembly;
            }

            private Assembly? HandleLoadAssembly(AssemblyLoadContext context, AssemblyName name) {
                bool NamesMatch(RuntimeLibrary library) {
                    return string.Equals(library.Name, name.Name, StringComparison.OrdinalIgnoreCase);
                }

                // Loop through dependencies to retrieve their libraries before using the mods provided by this resolver.
                // :coolandgood:
                foreach (Resolver resolver in Dependencies) {
                    Assembly? assembly = resolver.HandleLoadAssembly(context, name);

                    if (assembly is not null) return assembly;
                }

                RuntimeLibrary? library = DependencyContext.RuntimeLibraries.FirstOrDefault(NamesMatch);

                if (library is null) return null;

                CompilationLibrary wrapper = new(
                    library.Type,
                    library.Name,
                    library.Version,
                    library.Hash,
                    library.RuntimeAssemblyGroups.SelectMany(x => x.AssetPaths),
                    library.Dependencies,
                    library.Serviceable
                );
                List<string> assemblies = new();

                AssemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies);

                return assemblies.Count != 0 ? AssemblyLoadContext?.LoadFromAssemblyPath(assemblies[0]) : null;
            }
        }
    }
}