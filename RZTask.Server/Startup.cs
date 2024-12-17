using Microsoft.EntityFrameworkCore;
using RZTask.Server.Controllers;
using RZTask.Server.Data;
using Serilog;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        var connectionString = Configuration.GetConnectionString("DefaultConnection") ?? "";

        services.AddGrpc();  // 添加 gRPC 服务
        services.AddDbContext<AppDbContext>(option => option.UseMySQL(connectionString));
        services.AddTransient<AgentConnect>();
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
            //var grpcConfig = Configuration.GetSection("Kestrel:Endpoints:Grpc").Value;
            // 注册 TaskService 服务
            endpoints.MapGrpcService<ServerServiceImpl>();
        });
    }
}
