using _7VBPanel.Managers;
using _7VBPanel.Structures;
//using LibreHardwareMonitor.Hardware;
//using LibreHardwareMonitor.Hardware.Cpu;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
//using Computer = LibreHardwareMonitor.Hardware.Computer;

namespace _7VBPanel.Utils
{
    public static class HardwareUtils
    {
        private static List<VideoAdapter> AvalibleVideoAdapters;
    //    private static Computer _computer;
        private static ComputerInfo _computerInfo;

        public static void InitializeHardwareMonitor()
        {
           /* _computer = new Computer
            {
                IsCpuEnabled = true,
                IsMemoryEnabled = true
            };*/
            _computerInfo = new ComputerInfo();
            //_computer.Open();
        }

        public static int GetMemoryUsagePercent()
        {
            long totalMemory = (long)(_computerInfo.TotalPhysicalMemory / (1024 * 1024)); // В мегабайтах
            long availableMemory = (long)(_computerInfo.AvailablePhysicalMemory / (1024 * 1024)); // В мегабайтах
            return (int)((totalMemory - availableMemory) / (double)totalMemory * 100);
        }

        public static string GetPagingFileUsageInfo()
        {
            try
            {
                Win32.MEMORYSTATUSEX memStatus = new Win32.MEMORYSTATUSEX();
                memStatus.dwLength = (uint)Marshal.SizeOf(typeof(Win32.MEMORYSTATUSEX));

                if (Win32.GlobalMemoryStatusEx(ref memStatus))
                {
                    double totalPageFileSize = Convert.ToDouble(memStatus.ullTotalPageFile) / (1024 * 1024 * 1024);
                    double availablePageFileSize = Convert.ToDouble(memStatus.ullAvailPageFile) / (1024 * 1024 * 1024);
                    double usedPageFileSize = totalPageFileSize - availablePageFileSize;
                    return $"{(int)Math.Ceiling(usedPageFileSize)}GB\n{(int)Math.Ceiling(totalPageFileSize)}GB";
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public static int GetPagingFileUsagePercent()
        {
            try
            {
                Win32.MEMORYSTATUSEX memStatus = new Win32.MEMORYSTATUSEX();
                memStatus.dwLength = (uint)Marshal.SizeOf(typeof(Win32.MEMORYSTATUSEX));

                if (Win32.GlobalMemoryStatusEx(ref memStatus))
                {
                    double totalPageFileSize = Convert.ToDouble(memStatus.ullTotalPageFile);
                    double availablePageFileSize = Convert.ToDouble(memStatus.ullAvailPageFile);
                    double usedPageFileSize = totalPageFileSize - availablePageFileSize;
                    double percentUsage = (usedPageFileSize / totalPageFileSize) * 100.0;
                    return Convert.ToInt32(Math.Round(percentUsage));
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
        public static string GetMemoryInfo()
        {
            try
            {
                ComputerInfo computerInfo = new ComputerInfo();
                ulong totalMemory = computerInfo.TotalPhysicalMemory;
                ulong availableMemory = computerInfo.AvailablePhysicalMemory;
                string usedMemory = $"{(totalMemory - availableMemory) / (1024 * 1024 * 1024)}GB";
                string totalMemoryStr = $"{(int)Math.Ceiling(totalMemory / (1024.0 * 1024 * 1024))}GB"; ;

                return $"{usedMemory}\n{totalMemoryStr}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return "Error retrieving memory information";
            }
        }
        public static string GetCpuVoltage()
        {

            return "Unknown";
            /* double totalClock = 0;
             int coreCount = 0;

             foreach (IHardware hardware in _computer.Hardware)
             {
                 if (hardware.HardwareType != HardwareType.Cpu) continue;
                 hardware.Update();
                 foreach (ISensor sensor in hardware.Sensors)
                 {
                     if (sensor.SensorType != SensorType.Clock && sensor.Name.Contains("Core #")) continue;
                     totalClock += sensor.Value.GetValueOrDefault();
                     coreCount++;
                 }
             }
             if (coreCount > 0)
             {
                 double avgClock = totalClock / coreCount;
                 return $"{avgClock / 1000:0.0} GHz";
             }
             return "Unknown";
 */
        }

        public static int GetCpuUsagePercent()
        {
            return 0;
          /*  float cpuUsage = 0;

            if (_computer != null && _computer.Hardware != null)
            {
                var cpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);

                if (cpu != null)
                {
                    cpu.Update();

                    var loadSensor = cpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name == "CPU Total");

                    if (loadSensor != null && loadSensor.Value.HasValue)
                    {
                        cpuUsage = (float)loadSensor.Value.Value;
                    }
                }
            }*/

/*            return (int)cpuUsage;*/
        }
        public static VideoAdapter GetVideoAdapter(int VendorID, int DeviceID)
        {
            foreach (var adapter in AvalibleVideoAdapters)
            {
                if (adapter.VendorID == VendorID && adapter.DeviceID == DeviceID)
                {
                    return adapter;
                }
            }
            return new VideoAdapter();
        }
        public static VideoAdapter GetVideoAdapter(string AdapterID)
        {
            foreach (var adapter in AvalibleVideoAdapters)
            {
                if (adapter.AdapterID == AdapterID)
                {
                    return adapter;
                }
            }
            return new VideoAdapter();
        }
        public static List<VideoAdapter> GetVideoAdapters()
        {
            return AvalibleVideoAdapters;
        }
        public static void LoadVideoAdapters()
        {
            AvalibleVideoAdapters = new List<VideoAdapter>();
            try
            {
                string query = "SELECT * FROM Win32_VideoController";
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

                foreach (ManagementObject obj in searcher.Get())
                {
                    VideoAdapter videoAdapter = new VideoAdapter();
                    videoAdapter.AdapterID = obj["PNPDeviceID"].ToString();
                    videoAdapter.AdapterName = obj["Caption"].ToString();
                    string[] source = (from e in obj["PNPDeviceID"].ToString().Split('\\')
                                       where e.Contains('&') && e.Contains("DEV") && e.Contains("VEN")
                                       select e).First().Split('&');
                    videoAdapter.VendorID = int.Parse(source.First((string e) => e.Contains("VEN")).Replace("VEN_", ""), NumberStyles.HexNumber);
                    videoAdapter.DeviceID = int.Parse(source.First((string e) => e.Contains("DEV")).Replace("DEV_", ""), NumberStyles.HexNumber);
                    AvalibleVideoAdapters.Add(videoAdapter);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cant be loaded video adapters.");
            }
        }
        public static void GetVendorAndDeviceID(out int VendorID, out int DeviceID)
        {
            VendorID = 0;
            DeviceID = 0;
            foreach (var adapeter in AvalibleVideoAdapters)
            {
                if (adapeter.AdapterID == SettingsManager.SelectedGPUID)
                {
                    VendorID = adapeter.VendorID;
                    DeviceID = adapeter.DeviceID;
                    return;
                }
            }
            VendorID = AvalibleVideoAdapters[0].VendorID;
            DeviceID = AvalibleVideoAdapters[0].DeviceID;
        }
    }

}
