using gestionticket_v2.Data;
using gestionticket_v2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace gestionticket_v2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly gestionticket_v2Context _context;

        public HomeController(ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, gestionticket_v2Context context)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Login() // GET
        {
            return View();
        }

        public IActionResult Register() // GET
        {
            return View();
        }

        [HttpPost] // est utilisé pour indiquer que cette méthode d'action 
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid) // vérifie si le modèle est valide
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false); // vérifie si l'utilisateur existe dans la base de données
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email); // récupère l'utilisateur
                    var roles = await _userManager.GetRolesAsync(user);//   récupère les rôles de l'utilisateur

                    // Rediriger en fonction du rôle de l'utilisateur
                    if (roles.Contains("Client")) // si l'utilisateur est un client
                    {
                        return RedirectToAction("ClientTickets", "Tickets"); // remplacer par les noms de votre action et de votre contrôleur
                    }
                    else if (roles.Contains("MembreSupportTechnique")) // si l'utilisateur est un membre du support technique
                    {
                        return RedirectToAction("TechnicianTickets", "Tickets"); // remplacer par les noms de votre action et de votre contrôleur
                    }
                    else if (roles.Contains("Admin")) // si l'utilisateur est un administrateur
                    {
                        return RedirectToAction("Index", "Tickets"); // remplacer par les noms de votre action et de votre contrôleur
                    }
                }
                ModelState.AddModelError(string.Empty, "Tentative de connexion invalide."); // si l'utilisateur n'existe pas dans la base de données
            }
            return View(model); // retourne la vue avec le modèle
        }

        [HttpPost] // est utilisé pour indiquer que cette méthode d'action
        public async Task<IActionResult> Register(RegisterViewModel model) // POST
        {
            if (model == null) // vérifie si le modèle est null
            {
                throw new ArgumentNullException(nameof(model)); // si le modèle est null, une exception est levée
            }

            if (_userManager == null) // vérifie si le gestionnaire d'utilisateurs est null
            {
                throw new ArgumentNullException(nameof(_userManager)); // si le gestionnaire d'utilisateurs est null, une exception est levée
            }
            if (_signInManager == null) // vérifie si le gestionnaire de connexion est null
            {
                throw new ArgumentNullException(nameof(_signInManager)); // si le gestionnaire de connexion est null, une exception est levée
            }
            if (_context == null) // vérifie si le contexte est null
            {
                throw new ArgumentNullException(nameof(_context)); // si le contexte est null, une exception est levée
            }
            if (ModelState.IsValid) // vérifie si le modèle est valide
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, Nom = model.Nom, Prenom = model.Prenom, UserType = model.UserType };// créer un nouvel utilisateur
                var result = await _userManager.CreateAsync(user, model.Password); // créer l'utilisateur dans la base de données

                if (result.Succeeded) // vérifie si l'utilisateur a été créé avec succès
                {
                    // Assigner le rôle à l'utilisateur et créer l'entité correspondante
                    if (model.UserType == "Client")
                    {
                        await _userManager.AddToRoleAsync(user, Roles.Client); // assigne le rôle au nouvel utilisateur

                        var client = new Client { Id = user.Id, Nom = model.Nom, Prenom = model.Prenom }; // créer un nouveau client
                        _context.Client.Add(client); // ajoute le client au contexte
                    }
                    else if (model.UserType == "MembreSupportTechnique") // si l'utilisateur est un membre du support technique
                    {
                        await _userManager.AddToRoleAsync(user, Roles.MembreSupportTechnique); //

                        var technician = new MembreSupportTechnique { Id = user.Id, Nom = model.Nom, Prenom = model.Prenom };
                        _context.MembreSupportTechnique.Add(technician);
                    }

                    await _context.SaveChangesAsync();

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var viewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(viewModel);
        }
    }
}
