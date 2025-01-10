using Google.Protobuf;
using Grpc.Core;
using RZTask.Common.Protos;
using RZTask.Server.Controllers;
using System.Security.Cryptography;

public class ServerServiceImpl : ServerService.ServerServiceBase
{
    private readonly Serilog.ILogger logger;

    private readonly AgentConnect agentConnect;
    private readonly IConfiguration configuration;

    public ServerServiceImpl(Serilog.ILogger logger, IConfiguration config, AgentConnect agentConnect)
    {
        this.logger = logger;
        this.agentConnect = agentConnect;
        this.configuration = config;
    }

    public override Task<Response> RegisterAgent(
        AgentRegistrationRequest request,
        ServerCallContext context)
    {
        try
        {
            agentConnect.RegisterAgent(request);
            return Task.FromResult(new Response
            {
                Code = 200,
                Message = "Agent registered successfully"
            });
        }
        catch (Exception e)
        {
            string message = $"Register Agent error: {e.Message}";
            logger.Error(message, e);
            return Task.FromResult(new Response
            {
                Code = 501,
                Message = message
            });
        }
    }


    public override Task<Response> AgentHeartbeat(AgentHeartbeatRequest request, ServerCallContext context)
    {
        return Task.FromResult(agentConnect.AgentHeartbeat(request));
    }

    public override async Task DownloadFile(DownlaodFileRequest request, IServerStreamWriter<DownloadResponse> responseStream, ServerCallContext context)
    {
        string msg = string.Empty;
        try
        {
            logger.Information("Recive download dll file [{dllFile}] request from {agentId}", request.FileName, request.AgentId);
            var dllFileDirectory = configuration.GetValue<string>("FileStore:DllDirectory") ?? "./";
            var dllPath = Path.Combine(dllFileDirectory, request.FileName);
            if (!File.Exists(dllPath))
            {
                msg = "Can not found file " + dllPath;
                logger.Warning(msg);
                await responseStream.WriteAsync(new DownloadResponse
                {
                    FileName = request.FileName,
                    Message = msg,
                    Status = DownloadResponse.Types.DownloadStatus.Failed,
                });
                return;
            }
            if (!VerifyMD5(dllPath, request.Md5))
            {
                msg = string.Format("MD5 verify failed, request: {requestMd5}, current: {currentMd5}");
                logger.Warning(msg);
                await responseStream.WriteAsync(new DownloadResponse
                {
                    FileName = request.FileName,
                    Message = msg,
                    Status = DownloadResponse.Types.DownloadStatus.Failed
                });
                return;
            }
            using (var fileStream = new FileStream(dllPath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    var chunk = ByteString.CopyFrom(buffer, 0, bytesRead);
                    await responseStream.WriteAsync(new DownloadResponse
                    {
                        FileName = request.FileName,
                        Content = chunk,
                        Message = "Pending",
                        Status = DownloadResponse.Types.DownloadStatus.Pending,
                    });
                }
                await responseStream.WriteAsync(new DownloadResponse
                {
                    FileName = request.FileName,
                    Message = "Success",
                    Status = DownloadResponse.Types.DownloadStatus.Success,
                });
            }
            return;
        }
        catch (FileNotFoundException ex)
        {
            msg = "Can not found file: " + ex.Message + Environment.NewLine + ex.StackTrace;
        }
        catch (UnauthorizedAccessException ex)
        {
            msg = "Permission error: " + ex.Message + Environment.NewLine + ex.StackTrace;
        }
        catch (IOException ex)
        {
            msg = "File read error: " + ex.Message + Environment.NewLine + ex.StackTrace;
        }
        catch (Exception ex)
        {
            msg = "Unkonw error: " + ex.Message + Environment.NewLine + ex.StackTrace;
        }
        logger.Error(msg);
        await responseStream.WriteAsync(new DownloadResponse
        {
            FileName = request.FileName,
            Message = msg,
            Content = ByteString.Empty,
            Status = DownloadResponse.Types.DownloadStatus.Failed
        });
        return;
    }

    public static string CalculateMD5(string filePath)
    {
        try
        {
            using (MD5 mD5 = MD5.Create())
            using (var fileStream = File.OpenRead(filePath))
            {
                byte[] hash = mD5.ComputeHash(fileStream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Calculate md5 failed: " + ex.Message);
        }
    }

    private bool VerifyMD5(string filePath, string expectedMd5)
    {
        var hash = CalculateMD5(filePath);
        return hash == expectedMd5;
    }
}
