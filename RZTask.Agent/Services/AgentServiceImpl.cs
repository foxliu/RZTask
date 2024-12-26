using Grpc.Core;
using RZTask.Common.Protos;
using ILogger = Serilog.ILogger;

namespace RZTask.Agent.Services
{
    public class AgentServiceImpl : AgentService.AgentServiceBase
    {
        private readonly ILogger _logger;

        public AgentServiceImpl(ILogger logger)
        {
            _logger = logger;
        }

        public override async Task<TaskResponse> ExecuteTask(TaskRequest request, IServerStreamWriter<TaskResponse> streamWriter, ServerCallContext context)
        {
            _logger.Information($"Executing task {request.TaskId}...");

            await Task.Delay(2000);

            return new TaskResponse
            {
                TaskId = request.TaskId,
                Status = TaskResponse.Types.TaskStatus.Completed,
                Result = $"Task {request.TaskId} executed successfully"
            };
        }

    }
}
