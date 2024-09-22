using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using _7VBPanel.Managers;
using System.Collections.ObjectModel;
using _7VBPanel.Utils;
using _7VBPanel.Structures;
using System.Text.RegularExpressions;
using _7VBPanel.UI.Elements;
using System.Threading;
using _7VBPanel.Instances;
using System.Security.Principal;

using System.Runtime.InteropServices;
using FlaUI.Core.Conditions;
using SharpDX.DXGI;
using SharpDX;
using FlaUI.Core.WindowsAPI;
using HidSharp;

namespace _7VBPanel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> VideoAdapters { get; set; }

        private void AddItem(string name, Brush Color)
        {
            ToggleSelection checkBox = new ToggleSelection();
            checkBox.ToggleText = name;
            checkBox.TextColor = Color;
            AccountListBox.Items.Add(checkBox);
        }
        public MainWindow()
        {
            InitializeComponent();

            SettingsManager.LoadSettings();
            AccountManager.LoadAccounts();
           
           
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HardwareUtils.InitializeHardwareMonitor();
            VendorIDTextBox.Text = SettingsManager.VendorID;
            DeviceIDTextBox.Text = SettingsManager.DeviceID;
            if(SettingsManager.VendorID == "0" || SettingsManager.DeviceID == "0")
            {
                VendorIDTextBox.Text = "";
                DeviceIDTextBox.Text = "";
            }
            using (var factory = new Factory1())
            {
                Adapter highPerformanceAdapter = null;
                long maxVRAM = 0;

                foreach (var adapter in factory.Adapters)
                {
                    var description = adapter.Description;

                    long adapterVRAM = description.DedicatedVideoMemory;

                    if (adapterVRAM > maxVRAM)
                    {
                        maxVRAM = adapterVRAM;
                        highPerformanceAdapter = adapter;
                    }

                }

                if (highPerformanceAdapter != null)
                {
                    var desc = highPerformanceAdapter.Description;
                   
                    int vendorId = desc.VendorId;
                    int deviceId = desc.DeviceId;
                    Console.WriteLine($"VendorID: 0x{vendorId:X4} ({vendorId})");
                    Console.WriteLine($"DeviceID: 0x{deviceId:X4} ({deviceId})");
                    Console.WriteLine($"Description: {desc.Description}");
                }
                else
                {
                    Console.WriteLine("High-performance GPU not found.");
                }
            }
            CS2ArgumentsTextBox.Text = SettingsManager.CS2Arguments;
            bool IsSteamFolder = FileExistsIgnoreCase(SettingsManager.SteamPath, "Steam.exe");
            SteamPathBtn.ButtonCircleColor = IsSteamFolder ? Brushes.Green : Brushes.Red;
            string cs2Path = System.IO.Path.Combine(SettingsManager.CS2Path, "game", "bin", "win64");
            bool IsCS2Folder = FileExistsIgnoreCase(cs2Path, "cs2.exe");
            CS2PathBtn.ButtonCircleColor = IsCS2Folder ? Brushes.Green : Brushes.Red;

           
            
            foreach (var account in AccountManager.AccountList)
            {
                AddItem(account.Login, account.Color);
                account.OnColorChangedEvent += OnColorChanged;
            }
        }
        private void OnColorChanged(string Login, Brush NewColor)
        {
            foreach (ToggleSelection account in AccountListBox.Items)
            {
                account.Dispatcher.Invoke(() =>
                {
                    if (Login == account.ToggleText)
                    {
                        account.TextColor = NewColor;
                    }
                });
            }
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left ||
                e.GetPosition(this).Y >= 30) return;
            try
            {
                DragMove();
            }
            catch (Exception ex)
            { }
        }

        private void CloseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void SelectPathButton_ButtonClick(object sender, RoutedEventArgs e)
        {

            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                bool IsSteamFolder = FileExistsIgnoreCase(dialog.SelectedPath, "Steam.exe");
                SteamPathBtn.ButtonCircleColor = IsSteamFolder ? Brushes.Green : Brushes.Red;
                if (IsSteamFolder)
                {
                    SettingsManager.SteamPath = dialog.SelectedPath;
                    SettingsManager.SaveSettings();
                }
            }
        }
        static bool FileExistsIgnoreCase(string directoryPath, string fileName)
        {
            if (directoryPath == null) return false;
            DirectoryInfo directory = new DirectoryInfo(directoryPath);
            if (!directory.Exists)
            {
                return false;
            }

            FileInfo[] files = directory.GetFiles();
            foreach (FileInfo file in files)
            {
                if (string.Equals(file.Name, fileName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
        private void SelectPathButton_ButtonClick_1(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                string cs2Path = System.IO.Path.Combine(dialog.SelectedPath, "game", "bin", "win64");
                bool IsCS2Folder = FileExistsIgnoreCase(cs2Path, "cs2.exe");
                CS2PathBtn.ButtonCircleColor = IsCS2Folder ? Brushes.Green : Brushes.Red;
                if (IsCS2Folder)
                {
                    SettingsManager.CS2Path = dialog.SelectedPath;
                    SettingsManager.SaveSettings();
                }
            }
        }

        private void ButtonWIthTextOnly_ButtonClick(object sender, RoutedEventArgs e)
        {
            CS2ArgumentsTextBox.Text = SettingsManager.CS2Arguments;
            bool IsSteamFolder = FileExistsIgnoreCase(SettingsManager.SteamPath, "Steam.exe");
            SteamPathBtn.ButtonCircleColor = IsSteamFolder ? Brushes.Green : Brushes.Red;
            string cs2Path = System.IO.Path.Combine(SettingsManager.CS2Path, "game", "bin", "win64");
            bool IsCS2Folder = FileExistsIgnoreCase(cs2Path, "cs2.exe");
            if (!IsSteamFolder)
            {
                MessageBox.Show("Steam Path is incorrect!!!");
                return;
            }
            if (!IsCS2Folder)
            {
                MessageBox.Show("CS2 Path is incorrect!!!");
                return;
            }
            List<AccountInstance> Accounts = AccountManager.GetSelectedAccounts(AccountListBox);
            
            new Thread(() => {
                foreach (var account in Accounts)
                {
                    if(account.MaFile == null)
                    {
                        MessageBox.Show($"{account.Login} MaFile NotFound.");
                        continue;
                    }
                    if(account.MaFile.Session == null)
                    {
                        MessageBox.Show($"{account.Login} MaFile Broken.");
                        continue;
                    }
                    if (account.MaFile.Session.SteamID == 0)
                    {
                        MessageBox.Show($"{account.Login} MaFile Broken.");
                        continue;
                    }
                    account.SteamClient.Start();
                }
            }).Start();
            
        }

        private void ButtonWIthTextOnly_ButtonClick_1(object sender, RoutedEventArgs e)
        {
            IntPtr hdc = Win32.GetDC(IntPtr.Zero);
            int screenWidth = Win32.GetDeviceCaps(hdc, Win32.DESKTOPHORZRES);
            int screenHeight = Win32.GetDeviceCaps(hdc, Win32.DESKTOPVERTRES);
            int x = 0;
            int y = 0;
            int maxHeightInRow = 0;

            foreach (var account in AccountManager.AccountList.Where(acc => acc.AccountStatus == Instances.EAccountStatus.InMainMenu))
            {
                if (Win32.GetWindowRect(account.CS2Client.CS2_WindowComponent.GetWindowHandle(), out Utils.RECT rect))
                {
                    int windowWidth = rect.Right - rect.Left;
                    int windowHeight = rect.Bottom - rect.Top;

                    if (x + windowWidth > screenWidth)
                    {
                        x = 0;
                        y += maxHeightInRow;
                        maxHeightInRow = 0;
                    }

                    account.CS2Client.CS2_WindowComponent.MoveCSWindow(x, y);
                    x += windowWidth;

                    if (windowHeight > maxHeightInRow)
                    {
                        maxHeightInRow = windowHeight;
                    }

                    if (y + windowHeight > screenHeight)
                    {
                        break;
                    }
                }
            }
        }
        private void ButtonWIthTextOnly_ButtonClick_2(object sender, RoutedEventArgs e)
        {
            ProcessesUtils.KillAllSteamAndCS();
        }
        private void ButtonWIthTextOnly_ButtonClick_3(object sender, RoutedEventArgs e)
        {
            
            List<AccountInstance> Accounts = AccountManager.GetSelectedAccounts(AccountListBox);

            new Thread(() => {
                foreach (var account in Accounts)
                {
                    account.SteamClient.Stop();
                }
            }).Start();
        }
        
        private void ButtonWIthTextOnly_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void VendorIDTextBox_Text_Changed(object sender, RoutedEventArgs e)
        {
            SettingsManager.VendorID = VendorIDTextBox.Text;
            SettingsManager.SaveSettings();
        }

        private void DeviceIDTextBox_Text_Changed(object sender, RoutedEventArgs e)
        {
            SettingsManager.DeviceID = DeviceIDTextBox.Text;
            SettingsManager.SaveSettings();
        }
    }
}
