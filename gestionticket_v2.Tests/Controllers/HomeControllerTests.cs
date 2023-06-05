using Xunit;
using gestionticket_v2.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using gestionticket_v2.Models;
using Moq;
using System.Threading.Tasks;
using gestionticket_v2.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace gestionticket_v2.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<MockableSignInManager> _signInManagerMock;
        private readonly gestionticket_v2Context _context;

        public HomeControllerTests()
        {
            // Mock du User Store
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            userStoreMock.Setup(x => x.FindByIdAsync(It.IsAny<string>(), CancellationToken.None))
                .ReturnsAsync(new ApplicationUser() { UserName = "TestUser" });

            var options = new DbContextOptionsBuilder<gestionticket_v2Context>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new gestionticket_v2Context(options);

            // Mock du User Manager
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object,
                null,
                new PasswordHasher<ApplicationUser>(),
                new IUserValidator<ApplicationUser>[0],
                new IPasswordValidator<ApplicationUser>[0],
                null,
                null,
                null,
                null);

            // Mock de la confirmation de l'utilisateur
            var userConfirmationMock = new Mock<IUserConfirmation<ApplicationUser>>();
            userConfirmationMock.Setup(x => x.IsConfirmedAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);

            // Mock du Sign In Manager
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _signInManagerMock = new Mock<MockableSignInManager>(
                _userManagerMock.Object,
                contextAccessor.Object,
                userPrincipalFactory.Object,
                null,
                null,
                null,
                userConfirmationMock.Object);
            var loggerMock = new Mock<ILogger<HomeController>>();
        }

        [Fact]
        public async Task Register_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<gestionticket_v2Context>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            var mockContext = new gestionticket_v2Context(options);

            var controller = new HomeController(null, _signInManagerMock.Object, _userManagerMock.Object, mockContext);
            controller.ModelState.AddModelError("error", "error");

            // Act
            var result = await controller.Register(new RegisterViewModel());

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Login_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            // Arrange
            var controller = new HomeController(null, _signInManagerMock.Object, _userManagerMock.Object, null);
            controller.ModelState.AddModelError("error", "error");

            // Act
            var result = await controller.Login(new LoginViewModel());

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Register_ReturnsRedirectToAction_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HomeController>>();
            var controller = new HomeController(loggerMock.Object, _signInManagerMock.Object, _userManagerMock.Object, _context);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            var model = new RegisterViewModel
            {
                Email = "test@test.com",
                Password = "Test123!",
                Nom = "Test",
                Prenom = "User",
                UserType = "Client"
            };

            // Act
            var result = await controller.Register(model);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Home", redirectToActionResult.ControllerName);
        }
    }
}
