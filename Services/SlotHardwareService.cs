using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace FYP_Navperks.Services
{
    public interface ISlotHardwareService
    {
        Task ReserveSlotHardware(string slotCode);
        Task ReleaseSlotHardware(string slotCode);
    }

    public class SlotHardwareService : ISlotHardwareService
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _barrierEndpoints;

        public SlotHardwareService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = new HttpClient();

            // Map barriers (NodeMCUs) with IP addresses from appsettings.json
            _barrierEndpoints = new Dictionary<string, string>
            {
                { "Barrier1", configuration["HardwareSettings:Barrier1"] },
                { "Barrier2", configuration["HardwareSettings:Barrier2"] },
                { "Barrier3", configuration["HardwareSettings:Barrier3"] },
                { "Barrier4", configuration["HardwareSettings:Barrier4"] },
            };
        }

        public async Task ReserveSlotHardware(string slotCode)
        {
            var barrierName = GetBarrierName(slotCode);
            if (barrierName == null)
                throw new Exception($"No barrier found for slotCode {slotCode}");

            var endpoint = _barrierEndpoints[barrierName];
            var url = $"{endpoint}/reserve"; // Example: http://192.168.1.50/reserve

            var content = new StringContent($"{{\"slotCode\":\"{slotCode}\"}}", Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to reserve hardware slot {slotCode} on {barrierName}");
        }

        public async Task ReleaseSlotHardware(string slotCode)
        {
            var barrierName = GetBarrierName(slotCode);
            if (barrierName == null)
                throw new Exception($"No barrier found for slotCode {slotCode}");

            var endpoint = _barrierEndpoints[barrierName];
            var url = $"{endpoint}/release"; // Example: http://192.168.1.50/release

            var content = new StringContent($"{{\"slotCode\":\"{slotCode}\"}}", Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to release hardware slot {slotCode} on {barrierName}");
        }

        // Helper method
        private string GetBarrierName(string slotCode)
        {
            if (slotCode.StartsWith("CS-S1") || slotCode.StartsWith("CS-S2") || slotCode.StartsWith("CS-S3"))
                return "Barrier1";
            if (slotCode.StartsWith("CS-S4") || slotCode.StartsWith("CS-S5") || slotCode.StartsWith("CS-S6"))
                return "Barrier2";
            if (slotCode.StartsWith("CM-S1") || slotCode.StartsWith("CM-S2") || slotCode.StartsWith("CM-S3"))
                return "Barrier3";
            if (slotCode.StartsWith("CM-S4") || slotCode.StartsWith("CM-S5") || slotCode.StartsWith("CM-S6"))
                return "Barrier4";

            return null;
        }
    }
}
