using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using RZTask.Common.Protos;
using RZTask.Common.Utils;
using RZTask.Server.Data;
using System.Security.Cryptography.X509Certificates;
using System.Text;

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
        public async Task<IActionResult> ExecTask()
        {
            var agentId = "192.168.136.250";

            var agentInfo = _appDbContext.Agents.FirstOrDefault(a => a.AgentId == agentId);

            if (agentInfo == null)
            {
                _logger.Information($"Not Found AgentId: {agentId}");
                return NotFound(agentId);
            }

            try
            {
                var clientCert = new X509Certificate2(System.Text.Encoding.UTF8.GetBytes(agentInfo.Certificate));

                var grpcChannel = GrpcClientService.CreateChannel(agentInfo.GrpcAddress, clientCert);
                var client = new AgentService.AgentServiceClient(grpcChannel);

                var taskRequest = new TaskRequest
                {
                    TaskId = agentId,
                    Type = TaskRequest.Types.TaskType.Cmd,
                    FunctionType = "",
                    FileName = "",
                    FunctionName = "ipconfig",
                    FunctionParmas = "/all"
                };

                using var call = client.ExecuteTask(taskRequest);

                var result = new StringBuilder();

                await foreach (var response in call.ResponseStream.ReadAllAsync())
                {
                    result.AppendLine(response.Result.ToString());
                    //return Ok(response);
                }

                return Ok(result.ToString());
            }
            catch (Exception ex)
            {
                _logger.Error($"ExecTask error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }
    }
}
