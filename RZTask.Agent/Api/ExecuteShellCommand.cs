using RZTask.Common.Protos;
using RZTask.Common.Utils;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Timer = System.Timers.Timer;

namespace RZTask.Agent.Api
{
    public class ExecuteShellCommand : ICommandInterface
    {
        private readonly Serilog.ILogger _logger;
        private readonly IConfiguration _configuration;

        private string _fileName = string.Empty;
        private List<string> _arguments = new List<string>();

        private int _returnCode = -99;
        public int ReturnCode { get => _returnCode; }


        public event Action<string>? OnOutputReceived;

        private readonly ApplicationStore _applicationStore = ApplicationStore.Instance;

        private Queue<string> _outputBuffer = new Queue<string>();
        private Timer? _outputTimer;

        private static readonly Regex _parameterRegex = new Regex(@"['""].+?['""]|[^ ]+", RegexOptions.Compiled);


        public ExecuteShellCommand(Serilog.ILogger logger, IConfiguration config)
        {
            _logger = logger;
            _configuration = config;
        }

        private void AddArgumentsForCommand(string commandLine)
        {
            foreach (Match param in _parameterRegex.Matches(commandLine))
            {
                _arguments.Add(param.Value);
            }
        }

        public void InitializationCmd(TaskRequest request)
        {
            switch (request.Type)
            {
                case TaskRequest.Types.TaskType.Cmd:
                    _fileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash";
                    var paramTag = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "/C" : "-c";
                    AddArgumentsForCommand($"{paramTag} {request.FunctionName} {request.FunctionParmas}");
                    _logger.Debug($"{_fileName}, {_arguments}");
                    break;
                case TaskRequest.Types.TaskType.Sh:
                    _fileName = "/bin/sh";
                    AddArgumentsForCommand($"-c {request.FunctionName} {request.FunctionParmas}");
                    break;
                case TaskRequest.Types.TaskType.ShellScript:
                    _fileName = "/bin/bash";
                    var scriptFilePath = Path.Combine(_applicationStore!.ExecableFileDirectory, request.ProgramFileName);
                    AddArgumentsForCommand($"-c {scriptFilePath} {request.FunctionParmas}");
                    break;
                case TaskRequest.Types.TaskType.BinaryFile:
                    _fileName = Path.Combine(_applicationStore!.ExecableFileDirectory, request.ProgramFileName);
                    AddArgumentsForCommand(request.FunctionParmas);
                    break;
            }
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            var psi = new ProcessStartInfo
            {
                FileName = _fileName,
                //RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            foreach (var parma in _arguments)
            {
                psi.ArgumentList.Add(parma);
            }

            using (var process = new Process())
            {
                process.StartInfo = psi;

                if (_outputTimer == null)
                {
                    _outputTimer = new Timer(500);
                    _outputTimer.Elapsed += (sender, e) => FlushBuffer();
                }
                _outputTimer.Start();

                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        _outputBuffer.Enqueue(args.Data);
                    }
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        _outputBuffer.Enqueue($"[ERROR] {args.Data}");
                    }
                };

                try
                {
                    if (!process.Start())
                    {
                        var commandLine = $"{_fileName} {string.Join(" ", _arguments)}";
                        _logger.Error($"命令启动失败: {commandLine}");
                    }

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    var processCompletionTask = Task.Run(() =>
                    {
                        process.WaitForExit();
                    }, cancellationToken);

                    var timeout = _configuration.GetValue<int>("TaskTimeout") * 1000;
                    var timeoutTask = Task.Delay(timeout, cancellationToken);
                    var completedTask = await Task.WhenAny(processCompletionTask, timeoutTask);
                    if (completedTask == timeoutTask)
                    {
                        _logger.Error($"命令超时，强制停止进程: {_fileName} {string.Join(" ", _arguments)}");
                        process.Kill();
                    }


                    await processCompletionTask;

                    _returnCode = process.ExitCode;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"命令执行失败: {_fileName} {string.Join(" ", _arguments)}");
                    if (ex.StackTrace != null) { _logger.Error(ex.StackTrace); }
                }
                finally
                {
                    FlushBuffer();
                    _outputTimer?.Stop();
                    _outputTimer?.Dispose();
                }
            };
        }

        private void FlushBuffer()
        {
            while (_outputBuffer.Count > 0)
            {
                var data = _outputBuffer.Dequeue();
                OnOutputReceived?.Invoke(data);
            }
        }
    }
}
