using RZTask.Server.Data;
using RZTask.Server.Models;

namespace RZTask.Server.Services
{
    public class AgentRegistry
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<AgentRegistry> _logger;

        public AgentRegistry(AppDbContext dbContext, ILogger<AgentRegistry> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public void RegisterAgent(string grpcAddress, string appName, string createdBy)
        {
            var agent = _dbContext.Agents.FirstOrDefault(a => a.GrpcAddress == grpcAddress);
            if (agent == null)
            {
                agent = new Models.Agents
                {
                    GrpcAddress = grpcAddress,
                    AppName = appName,
                    LastHeartbeat = DateTime.UtcNow,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow,
                };
                _dbContext.Agents.Add(agent);
            }
            else
            {
                agent.AppName = appName;
                agent.LastHeartbeat = DateTime.UtcNow;
                agent.UpdatedBy = createdBy;
                agent.UpdatedAt = DateTime.UtcNow;
            }

            _dbContext.SaveChanges();
        }

        // 获取所有在线 Agents
        public IEnumerable<Agents> GetOnlineAgents()
        {
            var threshold = DateTime.UtcNow.AddMinutes(-5);  // 心跳超时 5 分钟
            return _dbContext.Agents.Where(a => a.LastHeartbeat >= threshold);
        }

        // 更新心跳
        public void UpdateHeartbeat(string grpcAddress)
        {
            var agent = _dbContext.Agents.FirstOrDefault(b => b.GrpcAddress == grpcAddress);
            if (agent != null)
            {
                agent.LastHeartbeat = DateTime.UtcNow;
                _dbContext.SaveChanges();
            }
        }
    }
}
