using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace aggreagteDetectorDev
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if(args.Length>0 && (args[0]=="-D" || args[0]=="-d"))
            {
                Application.Run(new devMain(true));
            }
            else
                Application.Run(new devMain());
        }
    }
}
