using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RZTask.Server.Models
{
    // Agents 表对应的实体
    [Table("t_agents"), Index(nameof(GrpcAddress), IsUnique = true, Name = "grpc_address")]
    [Index(nameof(AgentId), IsUnique = true, Name = "agent_id")]
    [Comment("Agent 信息表")]
    public class Agents : BaseEntity
    {
        [Required, Column("agent_id"), Comment("Agent的唯一性标识")]
        public string AgentId { get; set; }

        [Required, Column("grpc_address"), Comment("Agent 的 gRPC 地址")]
        public string GrpcAddress { get; set; }

        [Required, Column("app_name"), Comment("Agent 应用名")]
        public string AppName { get; set; }

        [Required, Column("certificate"), Comment("证书内容")]
        public string Certificate { get; set; }

        [Required, Column("last_heartbeat"), Comment("上一次心跳时间")]
        public DateTime LastHeartbeat { get; set; }
    }

    // Dll File 表对应的实体
    [Table("t_dll_files"), Index(nameof(FileName), IsUnique = true, Name = "file_name")]
    [Comment("DLL 文件当前信息表")]
    public class DllFiles : BaseEntity
    {
        [Required, Column("file_name"), Comment("DLL文件名称")]
        public string FileName { get; set; }

        [Required, Column("md5"), Comment("文件的MD5值")]
        public string MD5 { get; set; }

        [Required, Column("version"), Comment("文件版本号")]
        public string Version { get; set; }

        [Required, Column("upload_time"), Comment("文件上传时间")]
        public DateTime UploadTime { get; set; }
    }

    // Dll History 表对应的实体
    [Table("t_dll_history"), Comment("DLL 文件历史信息表")]
    public class DllHistory : BaseEntity
    {
        [Required, Column("file_id"), Comment("dll文件ID")]
        public int FileId { get; set; }

        [Required, Column("md5"), Comment("文件的MD5值")]
        public string MD5 { get; set; }

        [Required, Column("version"), Comment("文件版本号")]
        public string Version { get; set; }

        [Required, Column("upload_time"), Comment("文件上传时间")]
        public DateTime UploadTime { get; set; }
    }

    // 任务与Agent对应关系表 对应的实体
    [Table("t_task_agent"), Index(nameof(TaskId), nameof(AgentId), IsUnique = true, Name = "task_agent")]
    [Comment("任务分配到的Agent执行表")]
    public class TaskAgent : BaseEntity
    {
        [Required, Column("task_id"), Comment("任务ID")]
        public int TaskId { get; set; }

        [Required, Column("agent_id"), Comment("Agent ID")]
        public int AgentId { get; set; }

        [Required, Column("status"), Comment("任务状态")]
        public int Status { get; set; }

        [Required, Column("task_data"), Comment("任务数据")]
        public string TaskData { get; set; }
    }

    // Tasks 表对应的实体
    [Table("t_tasks"), Index(nameof(DllFileId), Name = "dll_file_id"), Comment("任务信息表")]
    public class Tasks : BaseEntity
    {
        [Required, Column("task_type"), Comment("任务类型")]
        public string TaskType { get; set; }

        [Required, Column("dll_file_id"), Comment("任务使用的DLL文件ID")]
        public int DllFileId { get; set; }

        [Column("function_type"), Comment("通反射调用时需要使用 命名空间.类名")]
        public string FunctionType { get; set; }

        [Required, Column("function_name")]
        [Comment("调用的函数名称，如果是shell则为执行命令绝对路径")]
        public string FunctionName { get; set; }

        [Required, Column("function_params")]
        [Comment("函数调用的参数，如果是shell则是参数内容，shell的参数以空格分格，其他的参数以逗号分隔")]
        public string FunctionParams { get; set; }

        [Required, Column("description"), Comment("任务描述信息")]
        public string Description { get; set; }
    }
}
