using RZTask.Agent.Services;
using RZTask.Common.Structs;
using RZTask.Common.Utils;
using Serilog;

namespace RZTask.Agent
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<CertificateInfo>();
            services.AddGrpc();
            services.AddTransient<AgentRegistrar>();
            services.AddTransient<GrpcServerConnect>();
            services.AddSingleton<LocalInfo>();
            services.AddHostedService<HeartbeatkBackgroundService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseSerilogRequestLogging();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<AgentServiceImpl>();       // 映射 gRPC 服务
            });

            // 在 Agent 启动时主动向 Server 注册
            //var registrar = app.ApplicationServices.GetRequiredService<AgentRegistrar>();
            //registrar.RegisterAsync("Agent1", "http://localhost:50051", new[] { "TaskType1" }).Wait();
        }
    }
}
