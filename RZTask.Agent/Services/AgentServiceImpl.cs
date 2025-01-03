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

        public override async Task ExecuteTask(TaskRequest request, IServerStreamWriter<TaskResponse> streamWriter, ServerCallContext context)
        {
            _logger.Information($"Executing task {request.TaskId}...");

            await streamWriter.WriteAsync(new TaskResponse
            {
                TaskId = request.TaskId,
                Status = TaskResponse.Types.TaskStatus.InProgress,
                Result = $"Task {request.TaskId} executing"
            });

            await Task.Delay(1000);

            await streamWriter.WriteAsync(new TaskResponse
            {
                TaskId = request.TaskId,
                Status = TaskResponse.Types.TaskStatus.Completed,
                Result = $"Task {request.TaskId} executed successfully"
            });
            return;
        }

    }
}
