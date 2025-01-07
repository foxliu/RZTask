using RZTask.Agent.Api;
using RZTask.Agent.Client;
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
            
            services.AddTransient<AgentRegistrar>();
            services.AddTransient<GrpcServerConnect>();
            services.AddSingleton<LocalInfo>();
            services.AddHostedService<HeartbeatkBackgroundService>();

            services.AddTransient<ExecuteShellCommand>();

            services.AddGrpc();
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
        }
    }
}
