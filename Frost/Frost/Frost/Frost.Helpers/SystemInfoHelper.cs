using System;
using System.Linq;
using System.Runtime.InteropServices;
using Vortice.DXGI;
using Windows.System.Profile;

namespace Frost
{
    public static class SystemInfoHelper
    {
        public static string GetOSVersion()
        {
            ulong v = ulong.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
            return $"Windows {(v >> 48) & 0xFFFF}.{(v >> 32) & 0xFFFF}.{(v >> 16) & 0xFFFF}";
        }

        public static string GetTotalRam()
        {
            ulong bytes = Windows.System.MemoryManager.AppMemoryUsageLimit;
            return $"{bytes / (1024 * 1024 * 1024)} GB RAM";
        }

        public static string GetCpuName()
        {
            // For packaged apps, the only safe way is Environment
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "x64 CPU",
                Architecture.X86 => "x86 CPU",
                Architecture.Arm64 => "ARM64 CPU",
                Architecture.Arm => "ARM CPU",
                _ => "Unknown CPU"
            };
        }

        public static int GetCpuCores()
        {
            return Environment.ProcessorCount;
        }

        public static (string Name, string VRAM) GetGpuInfo()
        {
            try
            {
                using var factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();
                IDXGIAdapter1 adapter = null;

                for (uint i = 0; factory.EnumAdapters1(i, out var a).Success; i++)
                {
                    AdapterDescription1 desc = a.Description1;

                    // Skip software adapters
                    if ((desc.Flags & AdapterFlags.Software) != 0)
                        continue;

                    adapter = a;
                    break;
                }

                if (adapter == null)
                    return ("Unknown GPU", "VRAM: Unknown");

                AdapterDescription1 adapterDesc = adapter.Description1;
                string name = adapterDesc.Description.Trim();
                string vram = $"{adapterDesc.DedicatedVideoMemory / (1024 * 1024 * 1024)} GB";

                return (name, vram);
            }
            catch
            {
                return ("Unknown GPU", "VRAM: Unknown");
            }
        }
    }
}
