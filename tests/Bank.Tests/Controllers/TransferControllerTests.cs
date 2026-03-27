using System.Security.Claims;
using Bank.Api.Controllers;
using Bank.Domain;
using Bank.Domain.Exceptions;
using Bank.Service;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bank.Tests.Controllers;

public class TransferControllerTests
{
    private TransferController CreateController(Mock<IBankService> mockService, string? userName = null)
    {
        var logger = new Mock<ILogger<TransferController>>().Object;
        var controller = new TransferController(mockService.Object, logger);
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            userName != null ? [new Claim(ClaimTypes.Name, userName)] : [], "Test"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
        return controller;
    }

    [Fact(Skip = "Handler not yet implemented — remove Skip= when development is complete")]
    public async Task CreateTransfer_Returns201_WhenValid()
    {
        // Arrange
        var fromId = Guid.NewGuid();
        var toId = Guid.NewGuid();
        var fromAccount = new Account("alice", 200m) { };
        var transfer = new Transfer(fromId, toId, 50m);

        var mockService = new Mock<IBankService>();
        mockService.Setup(s => s.GetAccountAsync(fromId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fromAccount);
        mockService.Setup(s => s.CreateTransferAsync(fromId, toId, 50m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);

        var controller = CreateController(mockService, "alice");
        var request = new CreateTransferRequest(fromId, toId, 50m);

        // Act
        var result = await controller.CreateTransfer(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact(Skip = "Handler not yet implemented — remove Skip= when development is complete")]
    public async Task CreateTransfer_Returns400_WhenArgumentInvalid()
    {
        // Arrange
        var fromId = Guid.NewGuid();
        var toId = Guid.NewGuid();
        var fromAccount = new Account("alice", 200m) { };

        var mockService = new Mock<IBankService>();
        mockService.Setup(s => s.GetAccountAsync(fromId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fromAccount);
        mockService.Setup(s => s.CreateTransferAsync(fromId, toId, -1m, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Amount must be positive."));

        var controller = CreateController(mockService, "alice");
        var request = new CreateTransferRequest(fromId, toId, -1m);

        // Act
        var result = await controller.CreateTransfer(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    // TODO: Wrong owner (403)
    [Fact(Skip = "Handler not yet implemented — remove Skip= when development is complete")]
    public async Task CreateTransfer_Returns403_WhenCallerIsNotOwner()
    {
        // TODO: Mock GetAccountAsync to return an account owned by "bob"
        // Set userName to "alice" in CreateController(...)
        // Assert result is ForbidResult
        throw new NotImplementedException();
    }

    // TODO: Insufficient funds (422)
    [Fact(Skip = "Handler not yet implemented — remove Skip= when development is complete")]
    public async Task CreateTransfer_Returns422_WhenInsufficientFunds()
    {
        // TODO: Mock CreateTransferAsync to throw InsufficientFundsException
        // Assert result is UnprocessableEntityObjectResult
        throw new NotImplementedException();
    }

    // TODO: Account not found (404)
    [Fact(Skip = "Handler not yet implemented — remove Skip= when development is complete")]
    public async Task CreateTransfer_Returns404_WhenAccountNotFound()
    {
        // TODO: Mock GetAccountAsync to throw AccountNotFoundException
        // Assert result is NotFoundObjectResult
        throw new NotImplementedException();
    }
}
