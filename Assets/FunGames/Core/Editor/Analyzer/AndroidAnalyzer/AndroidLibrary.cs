namespace FunGames.Core.Editor.Analyzer
{
    public class AndroidLibrary
    {
        /// <summary>
        /// Path of lib file or .androidlib dir
        /// </summary>
        public string Path;

        /// <summary>
        /// Will be null if it's not a file but an .androidlib dir.
        /// </summary>
        public string FileName;
            
        /// <summary>
        /// A guess about the version based on the filename. It may be null.
        /// </summary>
        public string FileVersion;

        /// <summary>
        /// Additional info to FileName or AndroidLibDirName. Is null if the lib did not originate from a package.
        /// </summary>
        public string Package;

        /// <summary>
        /// Is null if the lib did not originate from a package.
        /// </summary>
        public string PackageVersion;
        
        public bool IsFromPackage => !string.IsNullOrEmpty(Package);

        /// <summary>
        /// Is null if the lib did not originate from a package.
        /// </summary>
        public string AndroidLibDirName;

        public AndroidLibrary(
            string filePath, string fileName, string version, string package = null, 
            string packageVersion = null, string androidLibDirName = null)
        {
            Path = filePath.Replace("\\", "/");
            FileName = fileName;
            FileVersion = version;
            Package = package;
            PackageVersion = packageVersion;
            AndroidLibDirName = androidLibDirName;
        }
    }
}