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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(DateTime? uploadedOn)
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

    }
}
