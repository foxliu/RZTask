using RZTask.Common.Protos;
using RZTask.Common.Utils;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace RZTask.Agent.Api
{
    public class ExecuteShellCommand
    {
        private readonly Serilog.ILogger _logger;

        private string _fileName = string.Empty;
        private List<string> _arguments = new List<string>();

        private int _returnCode = -99;
        public int ReturnCode { get => _returnCode; }


        public event Action<string>? OnOutputReceived;

        private readonly ApplicationStore _applicationStore = ApplicationStore.Instance;


        public ExecuteShellCommand(Serilog.ILogger logger)
        {
            _logger = logger;
        }

        public void InitializationCmd(TaskRequest request)
        {
            string pattern =  @"['""].+?['""]|[^ ]+";

            switch (request.Type)
            {
                case TaskRequest.Types.TaskType.Cmd:
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        _fileName = "cmd.exe";
                        foreach (Match parma in Regex.Matches($"/C {request.FunctionName} {request.FunctionParmas}", pattern))
                        {
                            _arguments.Add(parma.Value);
                        }
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        _fileName = "/bin/bash";
                        foreach (Match parma in Regex.Matches($"-c {request.FunctionName} {request.FunctionParmas}", pattern))
                        {
                            _arguments.Add(parma.Value);
                        }
                    }
                    break;
                case TaskRequest.Types.TaskType.Sh:
                    _fileName = "/bin/sh";
                    foreach (Match parma in Regex.Matches($"-c {request.FunctionName} {request.FunctionParmas}", pattern))
                    {
                        _arguments.Add(parma.Value);
                    }
                    break;
                case TaskRequest.Types.TaskType.ShellScript:
                    _fileName = "/bin/bash";
                    var scriptFilePath = Path.Combine(_applicationStore!.ExecableFileDirectory, request.FileName);
                    foreach (Match parma in Regex.Matches($"-c {scriptFilePath} {request.FunctionParmas}", pattern))
                    {
                        _arguments.Add(parma.Value);
                    }
                    break;
                case TaskRequest.Types.TaskType.BinaryFile:
                    _fileName = Path.Combine(_applicationStore!.ExecableFileDirectory, request.FileName);
                    foreach (Match parma in Regex.Matches(request.FunctionParmas, pattern))
                    {
                        _arguments.Add(parma.Value);
                    }
                    break;
            }
        }

        public void Start()
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

            var process = new Process
            {
                StartInfo = psi,
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
                    OnOutputReceived?.Invoke($"[ERROR] {args.Data}");
                }
            };

            try
            {
                if (!process.Start())
                {
                    _logger.Error($"命令启动失败: {_fileName} {@_arguments}");
                }
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
