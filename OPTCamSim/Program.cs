using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenTK;

namespace OPTCAMSim
{
    class MainClass
    {
        [STAThreadAttribute]
        public static void Main(string[] args)
        {
            //check if the args value is "opt" or "dcnc" then continue the app otherwise abort
            //if (!(args.Contains("opt") || args.Contains("dcnc")))
            //    return;

            Toolkit.Init(new ToolkitOptions { Backend = PlatformBackend.PreferX11});

            int w =1;
            if (w == 0)
            {
                //Test tool shapes
                var demo = new TDemoWindow();
                // Run the game at 60 updates per second
                demo.Run(60.0);
            }
            else
            {
                //run main program
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new DemoForm.MainForm());
            }
        }
    }
}
