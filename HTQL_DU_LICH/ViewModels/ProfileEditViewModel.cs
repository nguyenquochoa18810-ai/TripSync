using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HTQL_DU_LICH.ViewModels
{
    public class ProfileEditViewModel
    {
        
        public string FullName { get; set; } = "";

        public string Bio { get; set; } = "";

        public IFormFile? AvatarFile { get; set; }
    }
}