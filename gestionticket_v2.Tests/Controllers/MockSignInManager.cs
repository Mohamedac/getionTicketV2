using gestionticket_v2.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace gestionticket_v2.Tests.Controllers
{
    public class MockableSignInManager : SignInManager<ApplicationUser>
    {
        //Ce code représente une classe MockableSignInManager qui étend la classe SignInManager<ApplicationUser>.Elle est utilisée pour créer
        //    un gestionnaire de connexion fictif(mock) qui peut être utilisé lors des tests.La classe MockableSignInManager
        //    est utilisée dans les tests pour simuler le comportement du gestionnaire de connexio
        public MockableSignInManager(
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<ApplicationUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<ApplicationUser> confirmation)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }
    }

}
