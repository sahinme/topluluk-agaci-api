using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Nnn.ApplicationCore.Services.BlobService
{
    public interface IBlobService
    {
        Task<string> InsertFile(IFormFile asset);
    }
}