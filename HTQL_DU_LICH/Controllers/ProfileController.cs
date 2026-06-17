using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using HTQL_DU_LICH.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;


namespace HTQL_DU_LICH.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        private readonly IWebHostEnvironment _environment;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.IsProfileCompleted =
                user.IsProfileCompleted;

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            var model = new ProfileEditViewModel
            {
                FullName = user.FullName ?? "",
                Bio = user.Bio ?? ""
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProfileEditViewModel model)
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                model.FullName = user.FullName;
            }

            if (!string.IsNullOrWhiteSpace(model.FullName))
            {
                user.FullName = model.FullName;
            }

            if (!string.IsNullOrWhiteSpace(model.Bio))
            {
                user.Bio = model.Bio;
            }



            if (model.AvatarFile != null)
            {
                var fileName =
                    Guid.NewGuid() +
                    Path.GetExtension(
                        model.AvatarFile.FileName);

                var uploadFolder = Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "avatars");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var path = Path.Combine(
                    uploadFolder,
                    fileName);

                using (var stream =
                       new FileStream(path,
                       FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }

                user.AvatarUrl =
                    "/uploads/avatars/" + fileName;
            }

            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Index(ApplicationUser model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            user.FullName = model.FullName ?? "";
            user.Bio = model.Bio ?? "";

            user.IsProfileCompleted = true;

            await _userManager.UpdateAsync(user);

            TempData["Success"] =
                "Cập nhật thành công";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Interests()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var selectedIds = await _context.UserInterests
                .Where(x => x.UserId == user.Id)
                .Select(x => x.InterestId)
                .ToListAsync();
            var model = await _context.Interests
                .Select(i => new InterestSelectionViewModel
                {
                    Id = i.Id,
                    Name = i.Name,
                    IsSelected = selectedIds.Contains(i.Id)
                })
                .ToListAsync();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Interests(
            List<InterestSelectionViewModel> model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var oldInterests = _context.UserInterests
                .Where(x => x.UserId == user.Id);
            _context.UserInterests.RemoveRange(oldInterests);
            foreach (var item in model)
            {
                if (item.IsSelected)
                {
                    _context.UserInterests.Add(
                        new UserInterest
                        {
                            UserId = user.Id,
                            InterestId = item.Id
                        });
                }
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã lưu sở thích";
            return RedirectToAction(nameof(Interests));
        }
        [HttpPost]
        public async Task<IActionResult> UploadAvatar(UploadAvatarViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            if (model.AvatarFile != null)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(model.AvatarFile.FileName);

                var uploadFolder = Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "avatars");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string path = Path.Combine(
                    uploadFolder,
                    fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }

                user.AvatarUrl = "/uploads/avatars/" + fileName;

                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction(nameof(Index));
        }
    }

}