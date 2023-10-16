using AutoMapper;
using HermesChat.Data.Context;
using HermesChat.Data.Models;
using HermesChat.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HermesChat.Web.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly HermesChatContext _context;
        private readonly IMapper _mapper;

        public UserProfileController(UserManager<User> userManager, HermesChatContext context, IMapper mapper)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> UserProfile()
        {
            // Get the logged-in user.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Create a new view model.
            var viewModel = new UserProfile();

            // Map the data from the user to the view model.
            _mapper.Map(user, viewModel);

            // Get the profile from the database.
            var profile = await _context.Profile.FirstOrDefaultAsync(p => p.UserId == user.Id);

            // If a profile exists, map its data to the view model.
            if (profile != null)
            {
                _mapper.Map(profile, viewModel);
            }

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(IFormFile Avatar)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var profile = await _context.Profile.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile == null)
            {
                profile = new Data.Models.Profile { UserId = user.Id };
                _context.Profile.Add(profile);
                await _context.SaveChangesAsync();
            }

            if (Avatar != null && Avatar.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await Avatar.CopyToAsync(memoryStream);
                profile.Avatar = memoryStream.ToArray();
            }

            _context.Profile.Update(profile);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserProfile");
        }
    }
}
