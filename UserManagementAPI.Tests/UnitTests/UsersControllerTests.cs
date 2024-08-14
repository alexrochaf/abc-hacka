using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UserManagementAPI.Controllers;
using UserManagementAPI.Repositories;
using UserManagementAPI.Models;
using Xunit;
using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Data;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using UserManagementAPI.Services;

namespace UserManagementAPI.Tests.UnitTests;

public class UsersControllerTests
{
    [Fact]
    public async Task Test_GetAllUsers_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, Username = "user1", Email = "user1@example.com", FirstName = "John", LastName = "Doe" },
            new() { Id = 2, Username = "user2", Email = "user2@example.com", FirstName = "Jane", LastName = "Doe" }
        };

        var mockRepository = new Mock<IUserRepository>();
        mockRepository.Setup(repo => repo.GetAllUsersAsync()).ReturnsAsync(users);

        // Mock authentication
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Name, "testuser"),
        }, "mock"));

        var mockLogger = new Mock<ILogger<UsersController>>();
        var mockTokenService = new Mock<ITokenService>();

        var controller = new UsersController(mockRepository.Object, mockLogger.Object, mockTokenService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }

        };

        // Act
        var result = await controller.GetUsers();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<User>>>(result);
        var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedUsers = Assert.IsAssignableFrom<IEnumerable<User>>(okObjectResult.Value);
        Assert.Equal(2, returnedUsers.Count());
    }

    [Fact]
    public void Test_InMemoryDbSetup()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(databaseName: "TestInMemoryDb")
            .Options;

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("pass1");

        // Act
        using (var context = new UserDbContext(options))
        {
            context.Users.Add(new User { Id = 1, Username = "testuser", Email = "test@example.com", FirstName = "Test", LastName = "User", PasswordHash = "passwordHash" });
            context.SaveChanges();
        }

        // Assert
        using (var context = new UserDbContext(options))
        {
            Assert.Equal(1, context.Users.Count());
            Assert.Equal("testuser", context.Users.First().Username);
        }
    }

    [Fact]
    public async Task Test_GetUser_ReturnsUser()
    {
        // Arrange
        var user = new User { Id = 1, Username = "user1", Email = "user1@example.com", FirstName = "John", LastName = "Doe" };

        var mockRepository = new Mock<IUserRepository>();
        mockRepository.Setup(repo => repo.GetUserByIdAsync(1)).ReturnsAsync(user);

        var mockLogger = new Mock<ILogger<UsersController>>();
        var mockTokenService = new Mock<ITokenService>();

        var controller = new UsersController(mockRepository.Object, mockLogger.Object, mockTokenService.Object);

        // Act
        var result = await controller.GetUser(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<User>>(result);
        var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedUser = Assert.IsType<User>(okObjectResult.Value);
        Assert.Equal(1, returnedUser.Id);
        Assert.Equal("user1", returnedUser.Username);
    }

    [Fact]
    public async Task Test_CreateUser_ReturnsCreatedUser()
    {
        // Arrange
        var newUser = new User { Username = "newuser", Email = "newuser@example.com", FirstName = "New", LastName = "User", PasswordHash = "passwordHash" };

        var mockRepository = new Mock<IUserRepository>();
        mockRepository.Setup(repo => repo.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(new User
        {
            Id = 1,
            Username = "newuser",
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("passwordHash")
        });

        var mockLogger = new Mock<ILogger<UsersController>>();
        var mockTokenService = new Mock<ITokenService>();

        var controller = new UsersController(mockRepository.Object, mockLogger.Object, mockTokenService.Object);

        // Act
        var result = await controller.CreateUser(newUser);

        // Assert
        var actionResult = Assert.IsType<ActionResult<User>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var returnedUser = Assert.IsType<User>(createdAtActionResult.Value);
        Assert.Equal(1, returnedUser.Id);
        Assert.Equal("newuser", returnedUser.Username);
    }

    [Fact]
    public async Task Test_UpdateUser_ReturnsNoContent()
    {
        // Arrange
        var user = new User { Id = 1, Username = "user1", Email = "user1@example.com", FirstName = "John", LastName = "Doe" };

        var mockRepository = new Mock<IUserRepository>();
        mockRepository.Setup(repo => repo.UpdateUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<UsersController>>();
        var mockTokenService = new Mock<ITokenService>();

        var controller = new UsersController(mockRepository.Object, mockLogger.Object, mockTokenService.Object);

        // Act
        var result = await controller.UpdateUser(1, user);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Test_DeleteUser_ReturnsNoContent()
    {
        // Arrange
        var user = new User { Id = 1, Username = "user1", Email = "user1@example.com", FirstName = "John", LastName = "Doe" };

        var mockRepository = new Mock<IUserRepository>();
        mockRepository.Setup(repo => repo.UserExistsAsync(1)).ReturnsAsync(true);
        mockRepository.Setup(repo => repo.DeleteUserAsync(1)).Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<UsersController>>();
        var mockTokenService = new Mock<ITokenService>();

        var controller = new UsersController(mockRepository.Object, mockLogger.Object, mockTokenService.Object);

        // Act
        var result = await controller.DeleteUser(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        mockRepository.Verify(repo => repo.DeleteUserAsync(1), Times.Once);
    }
}