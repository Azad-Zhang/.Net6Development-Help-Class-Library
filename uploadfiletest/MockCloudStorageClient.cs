namespace 上传文件
{
    class MockCloudStorageClient : IStorageClient
    {
        public StorageType StorageType => StorageType.Public;
        private readonly IWebHostEnvironment hostEnv; //获取Web服务器的一个环境
        private readonly IHttpContextAccessor httpContextAccessor; //获取当前HttpContext的信息

        public MockCloudStorageClient(IWebHostEnvironment hostEnv, IHttpContextAccessor httpContextAccessor)
        {
            this.hostEnv = hostEnv;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<Uri> SaveAsync(string key, Stream content, CancellationToken cancellationToken = default)
        {
            if (key.StartsWith('/'))
            {
                throw new ArgumentException("key should not start with /", nameof(key));
            }
            string workingDir = Path.Combine(hostEnv.ContentRootPath, "wwwroot"); //上传到本地FileService.WebAPI下的wwwroot做备份文件服务器
            string fullPath = Path.Combine(workingDir, key);
            string? fullDir = Path.GetDirectoryName(fullPath);//get the directory
            if (!Directory.Exists(fullDir))//automatically create dir
            {
                Directory.CreateDirectory(fullDir);
            }
            if (File.Exists(fullPath))//如果已经存在，则尝试删除
            {
                File.Delete(fullPath);
            }
            using Stream outStream = File.OpenWrite(fullPath);
            await content.CopyToAsync(outStream, cancellationToken);
            var req = httpContextAccessor.HttpContext.Request;
            string url = req.Scheme + "://" + req.Host + "/FileService/" + key;
            return new Uri(url);
        }
    }

}
