using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
namespace aggreagteDetectorDev
{
    public static class packageInfo
    {
        /// <summary>
        /// assembly: AssemblyInformationalVersion
        /// </summary>
        public static string Version
        {
            get
            {
                try
                {
                    return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

                }
                catch
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Gets the application build Version (assembly: AssemblyVersion)
        /// </summary>
        /// <value>
        /// The application build.
        /// </value>
        public static string Build
        {
            get
            {
                try
                {
                    return Assembly.GetExecutingAssembly().GetName().Version.ToString();
                }
                catch
                {
                    return "";
                }
            }
        }


    }
}
