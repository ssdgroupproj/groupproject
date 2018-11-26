using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using encrypt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace encrypt.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public AdminController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager) {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // GET: Admin
        public ActionResult Index()
        {
            return View(_userManager.Users);
        }

        public async Task<ActionResult> DeActivate(string id) {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            DateTime date = new DateTime(2018, 12, 30);

            await _userManager.SetLockoutEndDateAsync(user, new DateTimeOffset(date));

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Activate(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            DateTime date = new DateTime(2018, 11, 25);

            await _userManager.SetLockoutEndDateAsync(user, new DateTimeOffset(date));

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Email(string id) {
            return View(await _userManager.FindByIdAsync(id));
        }

        [HttpPost]
        public async Task<ActionResult> Email()
        {
            return RedirectToAction("Index");
        }
    }
}