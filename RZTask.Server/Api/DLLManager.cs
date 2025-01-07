using RZTask.Server.Data;
using RZTask.Server.Models;
using System.Security.Cryptography;

namespace RZTask.Server.Api
{
    public class DLLManager
    {
        private readonly AppDbContext _dbContext;

        public DLLManager(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public DllFiles? SaveDLLFile(string filePath, string createBy)
        {
            var fileInfo = new FileInfo(filePath);
            var md5 = CalculateMD5(filePath);

            var existingDll = _dbContext.DllFiles.FirstOrDefault(d => d.FileName == fileInfo.Name);

            if (existingDll != null)
            {
                var history = new DllHistory
                {
                    FileId = existingDll.Id,
                    MD5 = md5,
                    Version = existingDll.Version,
                    UploadTime = DateTime.UtcNow,
                    CreatedBy = createBy,
                    CreatedAt = DateTime.UtcNow,
                };
                _dbContext.DllHistory.Add(history);

                existingDll.MD5 = md5;
                existingDll.UploadTime = DateTime.UtcNow;
                existingDll.UpdatedBy = createBy;
                existingDll.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var dll = new DllFiles
                {
                    FileName = fileInfo.Name,
                    MD5 = md5,
                    // todo: 需要添加版本号的管理逻辑
                    Version = "1.0.0",
                    UploadTime = DateTime.UtcNow,
                    CreatedBy = createBy,
                    CreatedAt = DateTime.UtcNow,
                };
                _dbContext.DllFiles.Add(dll);
            }

            _dbContext.SaveChanges();
            return existingDll ?? _dbContext.DllFiles.LastOrDefault(q => q.FileName == fileInfo.Name);
        }

        public string CalculateMD5(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            return BitConverter.ToString(md5.ComputeHash(stream));
        }
    }
}
