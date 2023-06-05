using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using gestionticket_v2.Data;

public class StatisticsController : Controller
{
    private gestionticket_v2Context _context;

    public StatisticsController(gestionticket_v2Context context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<JsonResult> TotalTickets()
    {
        var totalTickets = await _context.Tickets.CountAsync();
        return Json(totalTickets);
    }

    [HttpGet]
    public async Task<JsonResult> OpenClosedTickets()
    {
        var newTickets = await _context.Tickets.CountAsync(t => t.Statut == "Nouveau");
        var inProgressTickets = await _context.Tickets.CountAsync(t => t.Statut == "En cours");
        var completedTickets = await _context.Tickets.CountAsync(t => t.Statut == "Terminé");
        var closedTickets = await _context.Tickets.CountAsync(t => t.Statut == "Fermé");

        return Json(new { newTickets, inProgressTickets, completedTickets, closedTickets });
    }

    [HttpGet]
    public async Task<JsonResult> TicketsPerCategory()
    {
        var ticketsPerCategory = await _context.Tickets
            .Include(t => t.Categorie) // Assure que la relation avec Categorie est chargée
            .GroupBy(t => t.Categorie.Nom) // Groupe par le nom de la catégorie
            .Select(g => new { Categorie = g.Key, Count = g.Count() })
            .ToListAsync();

        return Json(ticketsPerCategory);
    }

    [HttpGet]
    public async Task<JsonResult> TicketsPerTeamMember()
    {
        var ticketsPerMember = await _context.Tickets
            .Include(t => t.Assignee)
            .Where(t => t.AssigneeId != null) // Filtrer les tickets avec un assigné
            .GroupBy(t => new { t.AssigneeId, FullName = t.Assignee.Nom + " " + t.Assignee.Prenom })
            .Select(g => new { AssigneeName = g.Key.FullName, Count = g.Count() })
            .OrderBy(g => g.AssigneeName) // Tri par le nom de l'assigné
            .ToListAsync();

        return Json(ticketsPerMember);
    }

    [HttpGet]
    public async Task<IActionResult> AverageResolutionTimePerCategory()
    {
        var tickets = await _context.Tickets
            .AsNoTracking()
            .Include(t => t.Categorie)
            .Where(t => t.DateModification > t.DateCreation) // S'assure que DateModification est supérieur à DateCreation
            .Select(t => new { t.Categorie.Nom, t.DateCreation, t.DateModification })
            .ToListAsync();

        var results = tickets
            .GroupBy(t => t.Nom)
            .Select(g => new
            {
                CategoryName = g.Key,
                AverageResolutionTime = g.Average(t => (t.DateModification - t.DateCreation).TotalHours)
            })
            .ToList();

        return Json(results);
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
