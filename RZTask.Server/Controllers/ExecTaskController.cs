using Microsoft.AspNetCore.Mvc;
using RZTask.Common.Protos;
using RZTask.Common.Utils;
using RZTask.Server.Data;
using System.Security.Cryptography.X509Certificates;

namespace RZTask.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExecTaskController : ControllerBase
    {
        private readonly Serilog.ILogger _logger;
        private readonly AppDbContext _appDbContext;

        public ExecTaskController(Serilog.ILogger logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public IActionResult ExecTask()
        {
            var agentId = "192.168.136.250";

            var agentInfo = _appDbContext.Agents.FirstOrDefault(a => a.AgentId == agentId);

            if (agentInfo == null)
            {
                return NotFound(agentId);
            }

            try
            {
                var clientCert = new X509Certificate2(System.Text.Encoding.UTF8.GetBytes(agentInfo.Certificate));
                var privateKey = agentInfo.PrivateKey;

                var grpcChannel = GrpcClientService.CreateChannel(agentInfo.GrpcAddress, clientCert);
                var client = new AgentService.AgentServiceClient(grpcChannel);

                var taskRequest = new TaskRequest
                {
                    TaskId = agentId,
                    TaskType = "shell",
                    FunctionType = "",
                    DllFileName = "test.dll",
                    FunctionName = "ipconfig",
                    FunctionParmas = "/all"
                };

                var response = client.ExecuteTask(taskRequest);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
