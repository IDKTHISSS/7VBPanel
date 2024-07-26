using _7VBPanel.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _7VBPanel.Instances
{
    public class CS2Instance
    {
        public Process CS2Process;
        private AccountInstance accountInstance;
        public CS2CmdComponent ConsoleCompnent = new CS2CmdComponent();
        public CS2WindowComponent CS2_WindowComponent = new CS2WindowComponent();
        public CS2Instance(AccountInstance accountInstance)
        {
            this.accountInstance = accountInstance;
        }
        public void Setup()
        {
            while (CS2Process == null)
            {
                Thread.Sleep(1000);
            }
            CS2_WindowComponent.Setup(CS2Process.MainWindowHandle);
        }
        public void Stop()
        {
            try
            {
                if (CS2Process != null)
                {
                    CS2Process.Kill();
                }

            }
            catch (Exception ex) { }
        }

    }

}
