using Microsoft.AspNetCore.Mvc;

namespace 上传文件.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class UploaderController : ControllerBase
    {

        private readonly FSDomainService domainService;

        public UploaderController(FSDomainService domainService)
        {
            this.domainService = domainService;
        }
        [HttpPost]
        [RequestSizeLimit(60_000_000)]
        public async Task<ActionResult<Uri>> Upload([FromForm] UploadRequest request, CancellationToken cancellationToken = default)
        {
            var file = request.File;
            string fileName = file.FileName;
            using Stream stream = file.OpenReadStream();
            var upItem = await domainService.UploadAsync(stream, fileName, cancellationToken);
            //dbContext.Add(upItem);
            return upItem.RemoteUrl;
        }
    }
}
