using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Core
{

    /// <summary>
    /// Provides an ordered collection of modules based on their dependencies.
    /// </summary>
    /// <remarks>
    /// Module A is dependent on module B if module A is declared in an assembly that references an assembly where module B is declared.
    /// It implements deterministic depth-first topological sort that ignores circular dependencies.
    /// </remarks>
    internal sealed class ModuleEntrySort
    {

        /// <summary>
        /// Represents a state of a module entry during topological sort.
        /// </summary>
        private enum ModuleEntryState
        {
            /// <summary>
            /// The module entry has not been processed yet.
            /// </summary>
            Alive,

            /// <summary>
            /// The module entry has been processed.
            /// </summary>
            Dead,

            /// <summary>
            /// The module entry is being processed.
            /// </summary>
            Processing
        }

        
        /// <summary>
        /// Sorts the specified collection of modules based on their dependencies.
        /// </summary>
        /// <param name="modules">A collection of modules.</param>
        /// <returns>An ordered collection of modules where each module comes after its dependencies.</returns>
        public List<ModuleEntry> Sort(List<ModuleEntry> modules)
        {
            if (modules == null)
            {
                throw new ArgumentNullException("modules");
            }

            // Make result deterministic
            var source = new List<ModuleEntry>(modules);
            source.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.ModuleMetadata.Name, b.ModuleMetadata.Name));
           
            // A mapping from an assembly name to a list module indices
            var moduleIndicesByAssemblyName = CreateModuleIndicesByAssemblyName(source);

            // An ordered collection of modules where each module comes after its dependencies
            var result = new List<ModuleEntry>(source.Count);

            // Current state of modules
            var state = new ModuleEntryState[source.Count];

            for (var i = 0; i < source.Count; i++)
            {
                state[i] = ModuleEntryState.Alive;
            }

            for (var i = 0; i < source.Count; i++)
            {
                Visit(i, source, moduleIndicesByAssemblyName, state, result);
            }
            
            return result;
        }


        /// <summary>
        /// Processes the specified module.
        /// </summary>
        /// <param name="index">The module index.</param>
        /// <param name="modules">A collection of modules to sort.</param>
        /// <param name="moduleIndicesByAssemblyName">A mapping from an assembly name to a list module indices.</param>
        /// <param name="state">Current state of modules.</param>
        /// <param name="result">An ordered collection of modules where each module comes after its dependencies.</param>
        private void Visit(int index, List<ModuleEntry> modules, Dictionary<String, List<Int32>> moduleIndicesByAssemblyName, ModuleEntryState[] state, List<ModuleEntry> result)
        {
            // Ignore modules that have been already processed
            if (state[index] == ModuleEntryState.Dead)
            {
                return;
            }

            // Ignore circular dependencies
            if (state[index] == ModuleEntryState.Processing)
            {
                return;
            }

            state[index] = ModuleEntryState.Processing;
            foreach (var dependencyIndex in GetModuleDependencies(index, modules, moduleIndicesByAssemblyName))
            {
                Visit(dependencyIndex, modules, moduleIndicesByAssemblyName, state, result);
            }
            state[index] = ModuleEntryState.Dead;

            var module = modules[index];
            result.Add(module);
        }


        /// <summary>
        /// Determines a set of dependencies for the specified module and returns a collection of their indices.
        /// </summary>
        /// <param name="index">The module index.</param>
        /// <param name="modules">A collection of modules to sort.</param>
        /// <param name="moduleIndicesByAssemblyName">A mapping from an assembly name to a list module indices.</param>
        /// <returns>A collection of dependency indices.</returns>
        private List<Int32> GetModuleDependencies(int index, List<ModuleEntry> modules, Dictionary<String, List<Int32>> moduleIndicesByAssemblyName)
        {
            var result = new List<Int32>(15);
            var module = modules[index];

            foreach (var assemblyName in module.GetType().Assembly.GetReferencedAssemblies().Select(x => x.FullName))
            {
                List<Int32> dependencyIndices = null;
                if (moduleIndicesByAssemblyName.TryGetValue(assemblyName, out dependencyIndices))
                {
                    result.AddRange(dependencyIndices);
                }
            }

            return result;
        }
        
        
        /// <summary>
        /// Creates a mapping from an assembly name to a list module indices, and returns it.
        /// </summary>
        /// <param name="modules">A collection of modules to sort.</param>
        /// <returns>A mapping from an assembly name to a list module indices.</returns>
        private Dictionary<String, List<Int32>> CreateModuleIndicesByAssemblyName(List<ModuleEntry> modules)
        {
            var result = new Dictionary<String, List<Int32>>(modules.Count, StringComparer.InvariantCultureIgnoreCase);

            for (var index = 0; index < modules.Count; index++)
            {
                var module = modules[index];
                var assemblyName = module.GetType().Assembly.FullName;

                List<Int32> moduleIndices = null;
                if (!result.TryGetValue(assemblyName, out moduleIndices))
                {
                    moduleIndices = new List<int>(7);
                    result.Add(assemblyName, moduleIndices);
                }
                moduleIndices.Add(index);
            }

            return result;
        }

    }

}