using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using gestionticket_v2.Data;
using gestionticket_v2.Models;
using Microsoft.AspNetCore.Identity;

namespace gestionticket_v2.Controllers
{
    public class TicketsController : Controller
    {
        private readonly gestionticket_v2Context _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(gestionticket_v2Context context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<TicketsController> logger)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> TechnicianTickets(string searchTerm)
        {
            if (!User.Identity.IsAuthenticated)
            {
                // L'utilisateur n'est pas authentifié. Redirigez-le vers la page de connexion.
                return RedirectToAction("Login", "Home");
            }

            var userIdString = _userManager.GetUserId(User);
            if (userIdString == null)
            {
                // L'utilisateur n'est pas connecté. Redirigez-le vers la page de connexion.
                return RedirectToAction("Login", "Home");
            }

            var user = await _userManager.FindByIdAsync(userIdString);
            if (user == null)
            {
                // L'utilisateur n'existe pas dans la base de données. Gérez ce cas de manière appropriée.
                _logger.LogWarning($"L'utilisateur avec l'ID '{userIdString}' n'existe pas dans la base de données.");
                return RedirectToAction("Login", "Account");
            }

            // Obtenez les tickets assignés à ce technicien
            IQueryable<Ticket> tickets = _context.Tickets
                .Include(t => t.Priorite)
                .Include(t => t.Categorie)
                .Where(t => t.AssigneeId == userIdString);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                tickets = tickets.Where(t => t.Titre.Contains(searchTerm));
            }

            // Le tri doit être effectué après l'application de tous les filtres.
            tickets = tickets.OrderByDescending(t => t.Priorite.Nom == "Élevé" ? 3 : t.Priorite.Nom == "Moyen" ? 2 : t.Priorite.Nom == "Faible" ? 1 : 0)
                .ThenBy(t => t.DateCreation);

            // Passez les tickets à la vue...
            return View(await tickets.ToListAsync());
        }

        public async Task<IActionResult> ClientTickets(string searchTerm, string searchBy)
        {
            // Si TempData contient le nom du technicien, passez-le au ViewBag
            if (TempData["TechnicianName"] != null)
            {
                ViewBag.TechnicianName = TempData["TechnicianName"].ToString();
            }
            if (!User.Identity.IsAuthenticated)
            {
                // L'utilisateur n'est pas authentifié. Redirigez-le vers la page de connexion.
                return RedirectToAction("Login", "Home");
            }

            var userIdString = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userIdString);
            if (user == null)
            {
                // L'utilisateur n'existe pas dans la base de données. Gérez ce cas de manière appropriée.
                _logger.LogWarning($"L'utilisateur avec l'ID '{userIdString}' n'existe pas dans la base de données.");
                return RedirectToAction("Login", "Home");
            }

            if (userIdString != null)
            {
                IQueryable<Ticket> tickets = _context.Tickets.Where(t => t.AuteurId == userIdString);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    switch (searchBy)
                    {
                        case "titre":
                            tickets = tickets.Where(t => t.Titre.Contains(searchTerm));
                            break;
                        case "description":
                            tickets = tickets.Where(t => t.Description.Contains(searchTerm));
                            break;
                        case "auteur":
                            tickets = tickets.Where(t => t.Auteur.UserName.Contains(searchTerm));
                            break;
                        case "assignee":
                            tickets = tickets.Where(t => t.Assignee.UserName.Contains(searchTerm));
                            break;
                        case "categorie":
                            tickets = tickets.Where(t => t.Categorie.Nom.Contains(searchTerm));
                            break;
                        case "priorite":
                            tickets = tickets.Where(t => t.Priorite.Nom.Contains(searchTerm));
                            break;
                        case "ticketId": // Gérer la recherche par ID de ticket
                            int ticketId;
                            if (int.TryParse(searchTerm, out ticketId))
                            {
                                tickets = tickets.Where(t => t.Id == ticketId);
                            }
                            break;
                        default:
                            break;
                    }
                }

                ViewBag.CurrentFilter = searchTerm;
                ViewBag.CurrentSearchBy = searchBy;

                return View(await tickets.ToListAsync());
            }
            else
            {
                // Gérer le cas où l'utilisateur n'est pas connecté.
                return View("Error");
            }
        }

        // GET: Tickets
        public async Task<IActionResult> Index(string searchTerm, string searchBy)
        {
            IQueryable<Ticket> tickets = _context.Tickets
                .Include(t => t.Assignee)
                .Include(t => t.Auteur)
                .Include(t => t.Categorie)
                .Include(t => t.Priorite)
                .OrderByDescending(t => t.Priorite.Nom == "Élevé") // Placer les tickets prioritaires en tête de liste
                .ThenBy(t => t.DateCreation); // Trier par date de création

            if (!string.IsNullOrEmpty(searchTerm))
            {
                switch (searchBy)
                {
                    case "titre":
                        tickets = tickets.Where(t => t.Titre.Contains(searchTerm));
                        break;
                    case "description":
                        tickets = tickets.Where(t => t.Description.Contains(searchTerm));
                        break;
                    case "auteur":
                        tickets = tickets.Where(t => t.Auteur.UserName.Contains(searchTerm));
                        break;
                    case "assignee":
                        tickets = tickets.Where(t => t.Assignee.UserName.Contains(searchTerm));
                        break;
                    case "categorie":
                        tickets = tickets.Where(t => t.Categorie.Nom.Contains(searchTerm));
                        break;
                    case "priorite":
                        tickets = tickets.Where(t => t.Priorite.Nom.Contains(searchTerm));
                        break;
                    case "ticketId": // Gérer la recherche par ID de ticket
                        int ticketId;
                        if (int.TryParse(searchTerm, out ticketId))
                        {
                            tickets = tickets.Where(t => t.Id == ticketId);
                        }
                        break;
                    default:
                        break;
                }
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.SearchBy = searchBy;

            return View(await tickets.ToListAsync());
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Priorite)
                .Include(t => t.Categorie)
                .Include(t => t.Auteur)
                .Include(t => t.Assignee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            var priorityList = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Élevé" },
                new SelectListItem { Value = "2", Text = "Moyen" },
                new SelectListItem { Value = "3", Text = "Faible" },
                // traduire le dropdown en français pour les priorites
            };

            var categoryList = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Problème technique" },
                new SelectListItem { Value = "2", Text = "Demande de facturation" },
                new SelectListItem { Value = "3", Text = "Question de vente" },
                new SelectListItem { Value = "4", Text = "Autre" },
                new SelectListItem { Value = "5", Text = "Demande de fonctionnalité" },
                new SelectListItem { Value = "6", Text = "Rapport de bogue" },
                new SelectListItem { Value = "7", Text = "Modification de compte" },
                new SelectListItem { Value = "8", Text = "Résiliation de compte" },
                new SelectListItem { Value = "9", Text = "Création de compte" },
                new SelectListItem { Value = "10", Text = "Facturation de compte" },
                new SelectListItem { Value = "11", Text = "Connexion de compte" },
                // traduire le dropdown en français pour les categories
            };

            ViewBag.PrioriteId = priorityList;
            ViewBag.CategorieId = categoryList;

            return View();
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Titre,Description,PrioriteId,CategorieId")] Ticket ticket)
        {
            ModelState.Remove("Auteur");
            ModelState.Remove("Statut");
            ModelState.Remove("Assignee");
            ModelState.Remove("AuteurId");
            ModelState.Remove("AssigneeId");
            ModelState.Remove("PiecesJointes");
            ModelState.Remove("Priorite");
            ModelState.Remove("Categorie");
            ModelState.Remove("Comment");

            if (ModelState.IsValid)
            {
                // Définir les valeurs par défaut
                ticket.DateCreation = DateTime.Now;
                ticket.Statut = "New"; // Statut par défaut
                ticket.Comment = "";
                ticket.AuteurId = _userManager.GetUserId(User); // ID de l'utilisateur actuel
                ticket.AssigneeId = await GetLeastAssignedTechnicianId(); // Assigner au technicien avec le moins de tickets

                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction("ClientTickets");
            }

            return View(ticket);
        }

        private async Task<string> GetLeastAssignedTechnicianId()
        {
            // Obtenez tous les techniciens
            var usersInRole = await _userManager.GetUsersInRoleAsync("MembreSupportTechnique");

            // S'il n'y a pas de techniciens, retournez null
            if (!usersInRole.Any())
            {
                return null;
            }

            // Regroupez les tickets par AssigneeId, comptez-les et triez-les par le nombre de tickets
            var technicianTicketCounts = await _context.Tickets
                .GroupBy(t => t.AssigneeId)
                .Select(g => new { TechnicianId = g.Key, TicketCount = g.Count() })
                .ToListAsync();

            // S'il n'y a pas de tickets, retournez l'ID du premier technicien
            if (!technicianTicketCounts.Any())
            {
                return usersInRole.First().Id;
            }

            // Trouvez le technicien avec le moins de tickets
            var leastTicketsTechnician = technicianTicketCounts.OrderBy(t => t.TicketCount).First();

            // Vérifiez s'il y a des techniciens sans tickets
            var techniciansWithNoTickets = usersInRole.Where(u => !technicianTicketCounts.Any(t => t.TechnicianId == u.Id)).ToList();

            // S'il y a des techniciens sans tickets, retournez l'ID du premier
            if (techniciansWithNoTickets.Any())
            {
                return techniciansWithNoTickets.First().Id;
            }

            // Retournez l'ID du technicien avec le moins de tickets
            return leastTicketsTechnician.TechnicianId;
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Priorite)
                .Include(t => t.Categorie)
                .Include(t => t.Auteur)
                .Include(t => t.Assignee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            var priorityList = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Élevé" },
                new SelectListItem { Value = "2", Text = "Moyen" },
                new SelectListItem { Value = "3", Text = "Faible" },
                // traduire les valeurs du dropdown en français
            };

            var categoryList = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Problème technique" },
                new SelectListItem { Value = "2", Text = "Demande de facturation" },
                new SelectListItem { Value = "3", Text = "Question de vente" },
                new SelectListItem { Value = "4", Text = "Autre" },
                new SelectListItem { Value = "5", Text = "Demande de fonctionnalité" },
                new SelectListItem { Value = "6", Text = "Rapport de bogue" },
                new SelectListItem { Value = "7", Text = "Modification de compte" },
                new SelectListItem { Value = "8", Text = "Résiliation de compte" },
                new SelectListItem { Value = "9", Text = "Création de compte" },
                new SelectListItem { Value = "10", Text = "Facturation de compte" },
                new SelectListItem { Value = "11", Text = "Connexion de compte" },
            };

            ViewBag.Priorite = new SelectList(priorityList, "Value", "Text", ticket.PrioriteId);
            ViewBag.Categorie = new SelectList(categoryList, "Value", "Text", ticket.CategorieId);

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titre,Description,PrioriteId,CategorieId,AuteurId,Statut,DateCreation,DateModification,Comment")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            var existingTicket = await _context.Tickets.FindAsync(id);
            if (existingTicket == null)
            {
                return NotFound();
            }

            // Mettre à jour les propriétés du ticket existant
            existingTicket.Titre = ticket.Titre;
            existingTicket.Description = ticket.Description;

            // Mettre à jour l'objet Categorie ainsi que son ID
            existingTicket.CategorieId = ticket.CategorieId;
            existingTicket.Categorie = await _context.Categorie.FindAsync(ticket.CategorieId);

            // Mettre à jour l'objet Priorite ainsi que son ID
            existingTicket.PrioriteId = ticket.PrioriteId;
            existingTicket.Priorite = await _context.Priorite.FindAsync(ticket.PrioriteId);

            existingTicket.Statut = ticket.Statut;
            existingTicket.DateModification = DateTime.Now;
            existingTicket.Comment = ticket.Comment;

            try
            {
                _context.Update(existingTicket);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketExists(ticket.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Tickets/Delete/5
        // Récupère les détails d'un ticket pour l'afficher dans la vue de suppression
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Assignee)
                .Include(t => t.Auteur)
                .Include(t => t.Categorie)
                .Include(t => t.Priorite)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        // Supprime le ticket confirmé de la base de données
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("L'ensemble d'entités 'gestionticket_v2Context.Ticket' est nul.");
            }
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Vérifie si un ticket existe dans la base de données en fonction de son ID
        private bool TicketExists(int id)
        {
            return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }

    }

}
