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

                ICommandInterface? commandInterface = request.Type switch
                {
                    TaskRequest.Types.TaskType.Cmd => _executeShellCommand,
                    TaskRequest.Types.TaskType.Sh => _executeShellCommand,
                    TaskRequest.Types.TaskType.BinaryFile => _executeShellCommand,
                    TaskRequest.Types.TaskType.ShellScript => _executeShellCommand,
                    _ => null
                } ?? throw new InvalidOperationException($"没有匹配的任务类型: {request.Type}");

                commandInterface.InitializationCmd(request);

                commandInterface.OnOutputReceived += async (output) =>
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

                var cancelationTokenSource = new CancellationTokenSource();
                var timeoutTask = Task.Delay((int)request.Timeout * 1000);
                var commandTask = commandInterface.Start(cancelationTokenSource.Token);

                var completedTask = await Task.WhenAny(commandTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    cancelationTokenSource.Cancel();

                    await streamWriter.WriteAsync(new TaskResponse
                    {
                        TaskId = request.TaskId,
                        Status = TaskResponse.Types.TaskStatus.Cancel,
                        Result = $"Task {request.TaskId} executed timeout, cancel",
                        ReturnCode = -1
                    });
                }

                await commandTask.ConfigureAwait(false);

                await streamWriter.WriteAsync(new TaskResponse
                {
                    TaskId = request.TaskId,
                    Status = TaskResponse.Types.TaskStatus.Completed,
                    Result = $"Task {request.TaskId} executed Completed",
                    ReturnCode = commandInterface.ReturnCode
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
        }
    }
}
