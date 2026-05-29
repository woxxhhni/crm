using Cls.Application.Files;
using Cls.Domain.Enums;
using MediatR;
using System.Net.Mail;

namespace Cls.Api.Services
{
    public interface IFileService
    {
        Task<string> GetFileUrl(int fileId, CancellationToken ct);
        Task<int> UploadFile(int userId, IFormFile file, UploadFileBucket bucket, CancellationToken ct);
        Task<List<int>> UploadFiles(int userId, IEnumerable<IFormFile> files, UploadFileBucket bucket, CancellationToken ct);
    }
    public class FileService(IMediator mediator) : IFileService
    {
        public async Task<string> GetFileUrl(int fileId, CancellationToken ct)
        {
           return await mediator.Send(new GetDownloadUrlQuery(fileId, TimeSpan.FromHours(1)), ct);
        }
        public async Task<int> UploadFile(int userId, IFormFile file, UploadFileBucket bucket, CancellationToken ct)
        {
            await using var stream = file.OpenReadStream();
            var result = await mediator.Send(new UploadFileCommand(bucket, file.FileName, stream, file.ContentType, file.Length, userId), ct);
            return result is null ? 0 : result.Id;
        }

        public async Task<List<int>> UploadFiles(int userId, IEnumerable<IFormFile> files, UploadFileBucket bucket, CancellationToken ct)
        {
            var fileIds = new List<int>();
            foreach (var file in files)
            {
                if (file is null) continue;
                await using var stream = file.OpenReadStream();
                var result = await mediator.Send(new UploadFileCommand(bucket, file.FileName, stream, file.ContentType, file.Length, userId), ct);
                if (result is not null)
                    fileIds.Add(result.Id);
            }

            return fileIds;
        }
    }
}
