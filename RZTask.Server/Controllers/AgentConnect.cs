using RZTask.Common.Protos;
using RZTask.Server.Data;
using RZTask.Server.Models;

namespace RZTask.Server.Controllers
{
    public class AgentConnect
    {
        private readonly AppDbContext _dbContext;

        public AgentConnect(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void RegisterAgent(AgentRegistrationRequest request)
        {
            var agentInfo = _dbContext.Agents.FirstOrDefault(a => a.AgentId == request.AgentId);

            if (agentInfo == null)
            {
                agentInfo = new Agents
                {
                    AgentId = request.AgentId,
                    GrpcAddress = request.GrpcAddress,
                    AppName = request.AppName,
                    KeyData = request.KeyData.ToByteArray(),
                    CertData = request.CertData.ToByteArray(),
                    LastHeartbeat = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "remote api",
                };
                _dbContext.Agents.Add(agentInfo);
            }
            else
            {
                agentInfo.GrpcAddress = request.GrpcAddress;
                agentInfo.AppName = request.AppName;
                agentInfo.KeyData = request.KeyData.ToByteArray();
                agentInfo.CertData = request.CertData.ToByteArray();
                agentInfo.LastHeartbeat = DateTime.Now;
                agentInfo.UpdatedBy = "remote api";
                agentInfo.UpdatedAt = DateTime.Now;
            }

            _dbContext.SaveChanges();
        }

        public Response AgentHeartbeat(AgentHeartbeatRequest request)
        {
            try
            {
                var agentInfo = _dbContext.Agents.First(a => a.AgentId == request.AgentId);
                if (agentInfo == null)
                {
                    return new Response
                    {
                        Code = 417,
                        Message = $"Not found agent: {request.AgentId}"
                    };
                }
                agentInfo.LastHeartbeat = DateTime.Now;
                agentInfo.UpdatedBy = "remote_api";
                agentInfo.UpdatedAt = DateTime.Now;

                _dbContext.SaveChanges();

                return new Response
                {
                    Code = 200,
                    Message = "OK"
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    Code = 500,
                    Message = ex.Message,
                };
            }
        }

    }
}
