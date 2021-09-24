using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Devices;

namespace AzureFunction.AlertTriggered
{
    public  class DisableDevice
    {
        private readonly IConfiguration _configuration;
        static RegistryManager registryManager;
        public DisableDevice(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [FunctionName("DisableDevice")]
        public  async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request to disable device.");

            JObject response = new JObject();
            var privateKey = _configuration.GetValue<string>("IotHubConnectionString");

            try
            {
                string deviceId = req.Query["device_id"];
                if (string.IsNullOrEmpty(deviceId))
                {
                    response.Add("message", "Error: Please provide valid device ID");
                    response.Add("code", 400);
                    return new BadRequestObjectResult(response);
                }
                registryManager = RegistryManager.CreateFromConnectionString(privateKey);
                Device device = await registryManager.GetDeviceAsync(deviceId);
                if (device is null)
                {
                    response.Add("message", $"Error: Device {deviceId} not found");
                    response.Add("code", 404);
                    return new BadRequestObjectResult(response);
                }
                device.Status = DeviceStatus.Disabled;
                await registryManager.UpdateDeviceAsync(device);

                response.Add("message", "Device disabled successfully");
                response.Add("code", 200);
                return new OkObjectResult(response);
            }
            catch (Exception e)
            {
                response.Add("message", e.Message);
                response.Add("stacktrace", e.StackTrace);
                response.Add("code", 500);
                return new BadRequestObjectResult(response);
            }
        }
    }
}
