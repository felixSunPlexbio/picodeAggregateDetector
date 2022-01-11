// <auto-generated />
#define GLOBALNAMESPACE
#define NOMETADATA
#pragma warning disable 0436

#if ADDMETADATA
[assembly: System.Reflection.AssemblyMetadata("GitInfo.IsDirty", ThisAssembly.Git.IsDirtyString)]
[assembly: System.Reflection.AssemblyMetadata("GitInfo.RepositoryUrl", ThisAssembly.Git.RepositoryUrl)]
[assembly: System.Reflection.AssemblyMetadata("GitInfo.Branch", ThisAssembly.Git.Branch)]
[assembly: System.Reflection.AssemblyMetadata("GitInfo.Commit", ThisAssembly.Git.Commit)]
[assembly: System.Reflection.AssemblyMetadata("GitInfo.Sha", ThisAssembly.Git.Sha)]
[assembly: System.Reflection.AssemblyMetadata("GitInfo.CommitDate", ThisAssembly.Git.CommitDate)]
[assembly: System.Reflection.AssemblyMetadata("GitInfo.BaseVersion.Major", ThisAssembly.Git.BaseVersion.Major)]
[assembly: System.Reflection.AssemblyMetadata("GitInfo.BaseVersion.Minor", ThisAssembly.Git.BaseVersion.Minor)]
[assembly: System.Reflection.AssemblyMetadata("GitInfo.BaseVersion.Patch", ThisAssembly.Git.BaseVersion.Patch)]
[assembly: System.Reflection.AssemblyMetadata("GitInfo.Commits", ThisAssembly.Git.Commits)]
[assembly: System.Reflection.AssemblyMetadata("GitInfo.Tag", ThisAssembly.Git.Tag)]
[assembly: System.Reflection.AssemblyMetadata("GitInfo.BaseTag", ThisAssembly.Git.BaseTag)]
[assembly: System.Reflection.AssemblyMetadata("GitInfo.SemVer.Major", ThisAssembly.Git.SemVer.Major)]
[assembly: System.Reflection.AssemblyMetadata("GitInfo.SemVer.Minor", ThisAssembly.Git.SemVer.Minor)]
[assembly: System.Reflection.AssemblyMetadata("GitInfo.SemVer.Patch", ThisAssembly.Git.SemVer.Patch)]
[assembly: System.Reflection.AssemblyMetadata("GitInfo.SemVer.Label", ThisAssembly.Git.SemVer.Label)]
[assembly: System.Reflection.AssemblyMetadata("GitInfo.SemVer.DashLabel", ThisAssembly.Git.SemVer.DashLabel)]
[assembly: System.Reflection.AssemblyMetadata("GitInfo.SemVer.Source", ThisAssembly.Git.SemVer.Source)]
#endif

#if LOCALNAMESPACE
namespace 
{
#endif
  /// <summary>Provides access to the current assembly information.</summary>
  partial class ThisAssembly
  {
    /// <summary>Provides access to the git information for the current assembly.</summary>
    public partial class Git
    {
      /// <summary>IsDirty: true</summary>
      public const bool IsDirty = true;

      /// <summary>IsDirtyString: true</summary>
      public const string IsDirtyString = @"true";

      /// <summary>Repository URL: https://github.com/felixSunPlexbio/picodeAggregateDetector</summary>
      public const string RepositoryUrl = @"https://github.com/felixSunPlexbio/picodeAggregateDetector";

      /// <summary>Branch: denkaQC</summary>
      public const string Branch = @"denkaQC";

      /// <summary>Commit: 9ade07b</summary>
      public const string Commit = @"9ade07b";

      /// <summary>Sha: 9ade07bc6725c306124dde9e3d23833c029fd96c</summary>
      public const string Sha = @"9ade07bc6725c306124dde9e3d23833c029fd96c";

      /// <summary>Commit date: 2022-01-07T13:22:56+08:00</summary>
      public const string CommitDate = @"2022-01-07T13:22:56+08:00";

      /// <summary>Commits on top of base version: 2</summary>
      public const string Commits = @"2";

      /// <summary>Tag: 0.2.0-2-g9ade07b</summary>
      public const string Tag = @"0.2.0-2-g9ade07b";

      /// <summary>Base tag: 0.2.0</summary>
      public const string BaseTag = @"0.2.0";

      /// <summary>Provides access to the base version information used to determine the <see cref="SemVer" />.</summary>      
      public partial class BaseVersion
      {
        /// <summary>Major: 0</summary>
        public const string Major = @"0";

        /// <summary>Minor: 2</summary>
        public const string Minor = @"2";

        /// <summary>Patch: 0</summary>
        public const string Patch = @"0";
      }

      /// <summary>Provides access to SemVer information for the current assembly.</summary>
      public partial class SemVer
      {
        /// <summary>Major: 0</summary>
        public const string Major = @"0";

        /// <summary>Minor: 2</summary>
        public const string Minor = @"2";

        /// <summary>Patch: 2</summary>
        public const string Patch = @"2";

        /// <summary>Label: </summary>
        public const string Label = @"";

        /// <summary>Label with dash prefix: </summary>
        public const string DashLabel = @"";

        /// <summary>Source: Tag</summary>
        public const string Source = @"Tag";
      }
    }
  }
#if LOCALNAMESPACE
}
#endif
#pragma warning restore 0436