using System.Collections.Generic;

namespace FunGames.Core.Editor.Analyzer
{
    /// <summary>
    /// One leaf in the dependency tree.
    /// </summary>
    public class Dependency
    {
        public enum GradleType { Package, LibFile, Project }

        public string Name;
        public GradleType Type;

        public string OriginalVersion;
        public string Version;
        public Dependency Parent;
        public List<Dependency> Children = new List<Dependency>();

        // used for fast comparison
        public string NameWithVersion;
        public string NameWithOriginalVersion;

        /// <summary>
        /// One leaf in the dependecy tree.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="originalVersion"></param>
        /// <param name="version"></param>
        /// <param name="parent"></param>
        public Dependency(string name, GradleType type, string originalVersion, string version, Dependency parent)
        {
            Name = name;
            Type = type;

            OriginalVersion = originalVersion;
            Version = version;

            Parent = parent;

            if (!string.IsNullOrEmpty(version))
                NameWithVersion = Name + ":" + version;
            else
                NameWithVersion = null;

            if (!string.IsNullOrEmpty(originalVersion))
                NameWithOriginalVersion = Name + ":" + originalVersion;
            else
                NameWithOriginalVersion = null;
        }
        
        /// <summary>
        /// Find a package which machtes this dependency or any of its children.
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public List<Dependency> Find(string package)
        {
            var results = new List<Dependency>();
            Find(this, package, results);
            return results;
        }

        private void Find(Dependency dependency, string package, List<Dependency> results)
        {
            if (dependency.Name == package
                || dependency.NameWithVersion == package
                || dependency.NameWithOriginalVersion == package
            )
                results.Add(dependency);

            foreach (var child in dependency.Children)
            {
                Find(child, package, results);
            }
        }

        /// <summary>
        /// The root of this dependency in the graph. If this already is the root then it returns itself.
        /// </summary>
        /// <returns></returns>
        public Dependency GetRoot()
        {
            Dependency root = this;
            var newRoot = Parent;
            while (newRoot != null)
            {
                root = newRoot;
                newRoot = root.Parent;
            }

            return root;
        }
    }
}