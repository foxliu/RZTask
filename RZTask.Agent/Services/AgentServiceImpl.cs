using Grpc.Core;
using RZTask.Agent.Api;
using RZTask.Common.Protos;
using ILogger = Serilog.ILogger;

namespace RZTask.Agent.Services
{
    public class AgentServiceImpl : AgentService.AgentServiceBase
    {
        private readonly ILogger _logger;
        private readonly ExecuteShellCommand _executeShellCommand;

        public AgentServiceImpl(ILogger logger, ExecuteShellCommand execute)
        {
            _logger = logger;
            _executeShellCommand = execute;
        }

        public override async Task ExecuteTask(TaskRequest request, IServerStreamWriter<TaskResponse> streamWriter, ServerCallContext context)
        {
            _logger.Information($"Executing task {@request}...");

            try
            {
                await streamWriter.WriteAsync(new TaskResponse
                {
                    TaskId = request.TaskId,
                    Status = TaskResponse.Types.TaskStatus.InProgress,
                    Result = $"Task {request.TaskId} executing",
                    ReturnCode = -99,
                });

                _executeShellCommand.InitializationCmd(request);

                _executeShellCommand.OnOutputReceived += async (output) =>
                {
                    if (!string.IsNullOrEmpty(output))
                    {
                        await streamWriter.WriteAsync(new TaskResponse
                        {
                            TaskId = request.TaskId,
                            Status = TaskResponse.Types.TaskStatus.InProgress,
                            Result = output,
                            ReturnCode = -99,
                        });
                    }
                };

                _executeShellCommand.Start();

                await streamWriter.WriteAsync(new TaskResponse
                {
                    TaskId = request.TaskId,
                    Status = TaskResponse.Types.TaskStatus.Completed,
                    Result = $"Task {request.TaskId} executed successfully",
                    ReturnCode = _executeShellCommand.ReturnCode
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Execute task error: {ex.Message}");
                if (ex.StackTrace != null)
                {
                    _logger.Error(ex.StackTrace);
                }
                await streamWriter.WriteAsync(new TaskResponse
                {
                    TaskId = request.TaskId,
                    Status = TaskResponse.Types.TaskStatus.Failed,
                    Result = $"Task {request.TaskId} executed failed: {ex.Message}",
                    ReturnCode = -1,
                });
            }
            await Task.CompletedTask;
        }
    }
}
