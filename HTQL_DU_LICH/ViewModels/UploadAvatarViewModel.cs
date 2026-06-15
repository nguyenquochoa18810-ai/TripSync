using Microsoft.AspNetCore.Http;

namespace HTQL_DU_LICH.ViewModels
{
    public class UploadAvatarViewModel
    {
        public IFormFile? AvatarFile { get; set; }
    }
}