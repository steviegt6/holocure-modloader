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
        private class Resolver
        {
            public readonly Assembly Assembly;

            private readonly Resolver[]? Dependencies;
            private readonly DependencyContext DependencyContext;
            private readonly CompositeCompilationAssemblyResolver AssemblyResolver;
            private readonly AssemblyLoadContext? AssemblyLoadContext;

            public Resolver(string assembly, Resolver[]? dependencies) {
                Dependencies = dependencies;
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

            public void Subscribe() {
                if (AssemblyLoadContext is null) return;
                AssemblyLoadContext.Resolving += HandleLoadAssembly;
            }

            public void Unsubscribe() {
                if (AssemblyLoadContext is null) return;
                AssemblyLoadContext.Resolving += HandleLoadAssembly;
            }

            public Assembly? HandleLoadAssembly(AssemblyLoadContext context, AssemblyName name) {
                bool NamesMatch(RuntimeLibrary library) {
                    return string.Equals(library.Name, name.Name, StringComparison.OrdinalIgnoreCase);
                }

                // Loop through dependencies to retrieve their libraries before using the mods provided by this resolver.
                // :coolandgood:
                if (Dependencies is not null) {
                    foreach (Resolver resolver in Dependencies) {
                        Assembly? assembly = resolver.HandleLoadAssembly(context, name);

                        if (assembly is not null) return assembly;
                    }
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

        private class LoadedMod : IMod
        {
            private readonly IMod Mod;
            private readonly Resolver Resolver;

            public LoadedMod(IMod mod, Resolver resolver) {
                Mod = mod;
                Resolver = resolver;
            }

            public void Load() {
                Resolver.Subscribe();
            }

            public void Unload() {
                Resolver.Unsubscribe();
            }

            public void PatchGame(UndertaleData gameData) {
                Mod.PatchGame(gameData);
            }
        }
    }
}