using RZTask.Common.Protos;
using RZTask.Common.Utils;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace RZTask.Agent.Api
{
    public class ExecuteShellCommand
    {
        private readonly Serilog.ILogger _logger;

        private string _fileName = string.Empty;
        private string _arguments = string.Empty;

        private int _returnCode = -99;
        public int ReturnCode { get => _returnCode; }


        public event Action<string>? OnOutputReceived;


        public ExecuteShellCommand(Serilog.ILogger logger)
        {
            _logger = logger;
        }

        public void InitializationCmd(TaskRequest request)
        {
            _arguments = $"-c {request.FunctionName} {request.FunctionParmas}";
            switch (request.Type)
            {
                case TaskRequest.Types.TaskType.Cmd:
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        _fileName = "cmd.exe";
                        _arguments = $"/C {request.FunctionName} {request.FunctionParmas}";
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        _fileName = "/bin/bash";
                        _arguments = $"-c {request.FunctionName} {request.FunctionParmas}";
                    }
                    break;
                case TaskRequest.Types.TaskType.Sh:
                    _fileName = "/bin/sh";
                    _arguments = $"-c {request.FunctionName} {request.FunctionParmas}";
                    break;
                case TaskRequest.Types.TaskType.ExecutableFile:
                    var store = ApplicationStore.Instance;
                    _fileName = Path.Combine(store!.ExecableFileDirectory, request.FileName);
                    break;
            }
        }

        public void Start()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = _fileName,
                Arguments = _arguments,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var process = new Process
            {
                StartInfo = processStartInfo,
            };

            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    OnOutputReceived?.Invoke(args.Data);
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    OnOutputReceived?.Invoke(args.Data);
                }
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();
                _returnCode = process.ExitCode;
            }
            catch (Exception ex)
            {
                _logger.Error($"命令执行失败: {ex.Message}");
                if (ex.StackTrace != null) { _logger.Error(ex.StackTrace); }
            }
            finally
            { 
                process.Dispose(); 
            }
        }
    }
}
