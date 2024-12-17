using Grpc.Core;
using RZTask.Common.Protos;
using ILogger = Serilog.ILogger;

namespace RZTask.Agent.Services
{
    public class AgentServiceImpl :ServerService.ServerServiceBase
    {
        private readonly ILogger logger;

        public AgentServiceImpl(ILogger logger)
        {
            this.logger = logger;
        }

        //public override async Task<TaskResponse> ExecuteTask(TaskRequest request, ServerCallContext context)
        //{
        //    logger.LogInformation($"Executing task {request.TaskId}...");

        //    await Task.Delay(2000);

        //    return new TaskResponse
        //    {
        //        TaskId = request.TaskId,
        //        Status = TaskResponse.Types.TaskStatus.Completed,
        //        Result = $"Task {request.TaskId} executed successfully"
        //    };
        //}
    }
}
