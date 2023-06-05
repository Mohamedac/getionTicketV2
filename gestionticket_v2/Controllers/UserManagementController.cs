using gestionticket_v2.Data;
using gestionticket_v2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace gestionticket_v2.Controllers
{
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly gestionticket_v2Context _context;

        public UserManagementController(UserManager<ApplicationUser> userManager, gestionticket_v2Context context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Action pour afficher la liste des utilisateurs
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                user.Roles = await _userManager.GetRolesAsync(user);
            }
            return View(users);
        }

        // ... autres actions comme Index, Create, Edit...

        // GET: UserManagement/Delete/5
        // Action HTTP GET pour afficher les détails de l'utilisateur à supprimer
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: UserManagement/Delete/5
        // Action HTTP POST pour supprimer l'utilisateur confirmé
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Trouver un autre utilisateur pour réattribuer les tickets
                var newAssignee = await _userManager.Users.FirstOrDefaultAsync(u => u.Id != id);
                if (newAssignee == null)
                {
                    // S'il n'y a pas d'autre utilisateur, nous ne pouvons pas continuer
                    ModelState.AddModelError("", "Aucun autre utilisateur n'existe pour réattribuer les tickets.");
                    return View("Delete", user);
                }

                // Réattribuer les tickets
                var tickets = _context.Tickets.Where(t => t.AssigneeId == id);
                foreach (var ticket in tickets)
                {
                    ticket.AssigneeId = newAssignee.Id;
                }

                await _context.SaveChangesAsync();

                // Supprimer l'utilisateur
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    // Gérer l'erreur
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    // Retourner à la vue Delete avec le modèle utilisateur
                    return View("Delete", user);
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
