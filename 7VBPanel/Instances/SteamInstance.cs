using _7VBPanel.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading;
using _7VBPanel.Utils;
using System.Threading.Tasks;
using System.Security.Principal;
using FlaUI.Core.AutomationElements;
using FlaUI.Core;
using FlaUI.UIA3;

namespace _7VBPanel.Instances
{
    public class SteamInstance
    {
        private AccountInstance accountInstance;
        public Process SteamProcess;
        public SteamInstance(AccountInstance _accountInstance)
        {
            accountInstance = _accountInstance;
        }
        public void Start()
        {
            CS2Optimizer.ConfigureAllFiles((accountInstance.MaFile.Session.SteamID - 76561197960265728).ToString(), SettingsManager.SteamPath, SettingsManager.CS2Path, accountInstance.Login);
            SteamProcess = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = $"{SettingsManager.SteamPath}" + $"\\steam.exe",
                Arguments = $"-silent -login -nofriendsui -vgui -master_ipc_name_override {accountInstance.Login} -noverifyfiles -nobootstrapupdate -skipinitialbootstrap -norepairfiles -overridepackageurl -disable-winh264 -language english -applaunch 730 -con_logfile {accountInstance.Login}.log -exec boost.cfg -allowmultiple " + SettingsManager.CS2Arguments,
                UseShellExecute = false
            };
            processStartInfo.Environment["VPROJECT"] = "whatever";
            SteamProcess.StartInfo = processStartInfo;
            SteamProcess.Exited += delegate
            {
                accountInstance.SetAccountColor(Brushes.White);
            };
            SteamProcess.EnableRaisingEvents = true;
            SteamProcess.Start();
            accountInstance.SetAccountColor(Brushes.Yellow);
            Thread.Sleep(5000);
            SteamUtils.LoginInSteamWindowFlaUIMethod(SteamProcess.Id, accountInstance);
            accountInstance.AccountStatus = EAccountStatus.WaitCS2;
            new Thread(() =>
            {
                accountInstance.FindCS2();
                accountInstance.CS2Client.ConsoleCompnent.Setup(accountInstance);
                accountInstance.CS2Client.ConsoleCompnent.StartReadingConsole();
                while (accountInstance.AccountStatus != EAccountStatus.InMainMenu)
                {
                    Thread.Sleep(1000);
                }
                accountInstance.CS2Client.CS2_WindowComponent.ChangeWindowTitle(accountInstance.Login);
                accountInstance.SetAccountColor(Brushes.Green);

            }).Start();
        }
        public void Stop()
        {
            if (SteamProcess == null) return;
            try
            {
                if (accountInstance.CS2Client != null)
                {
                    accountInstance.CS2Client.Stop();
                }
                SteamProcess.Kill();
                accountInstance.SetAccountColor(Brushes.White);
            }
            catch (Exception e)
            {

            }
        }
        public void UpdateCS2()
        {
            Win32.SetForegroundWindow(SteamProcess.MainWindowHandle);
            UIA3Automation val = new UIA3Automation();
            AutomationElement mainWindow = ((AutomationBase)val).FromHandle(SteamProcess.MainWindowHandle);

            var libraryElement = mainWindow.FindFirstDescendant(
            x => x.ByText("LIBRARY")).AsTextBox();
            if (libraryElement == null) return;
            libraryElement.Focus();
            libraryElement.Click();

            var CSElement = mainWindow.FindFirstDescendant(
            x => x.ByText("Counter-Strike 2")).AsTextBox();
            if (CSElement == null) return;
            CSElement.Focus();
            CSElement.Click();

            var UpdateButton = mainWindow.FindFirstDescendant(
            x => x.ByText("UPDATE")).AsButton();
            if (UpdateButton == null) return;
            UpdateButton.Invoke();
        }
    }

}
