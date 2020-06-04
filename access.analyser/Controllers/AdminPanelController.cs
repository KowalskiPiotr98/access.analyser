using access.analyser.Data;
using access.analyser.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace access.analyser.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminPanelController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        public AdminPanelController(RoleManager<IdentityRole> roleManager,
                           UserManager<IdentityUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var users = new List<AdminPanel>();

            foreach (var user in _userManager.Users)
            {
                users.Add(new AdminPanel { Id = user.Id, Username = user.UserName, Role = "User" });
            }
            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                for (int i=0; i < usersInRole.Count; i++)
                {
                    users[users.FindIndex(ind => ind.Username.Equals(usersInRole[i].UserName))]= new AdminPanel { Id = usersInRole[i].Id, Username = usersInRole[i].UserName, Role = role.Name };
                }
            }
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (id is null)
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (id is null)
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }
            await _userManager.DeleteAsync(user);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Permission(string id)
        {
            if (id is null)
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("Permission")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePermission(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
