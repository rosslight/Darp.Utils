namespace Darp.Utils.Dialog.Tests;

using System.ComponentModel.DataAnnotations;
using DialogData;
using FluentAssertions;

public class UsernamePasswordViewModelTests
{
    [Fact]
    public async Task IsCurrentStepValid_ShouldBeFalse_WhenPasswordStepHasPasswordErrorAfterUsernameError()
    {
        // Arrange
        var model = new UsernamePasswordViewModel
        {
            ValidateUsernameHandler = _ => new ValidationResult("Invalid username"),
            ValidatePasswordHandler = _ => new ValidationResult("Invalid password"),
            Username = "invalid",
        };
        await model.RequestNextStepAsync(CancellationToken.None);

        // Act
        model.Password = "invalid";

        // Assert
        model.IsCurrentStepValid.Should().BeFalse();
    }

    [Fact]
    public async Task RequestNextStepAsync_ShouldNotAdvance_WhenCurrentStepIsInvalid()
    {
        // Arrange
        var model = new UsernamePasswordViewModel
        {
            ValidatePasswordHandler = _ => new ValidationResult("Invalid password"),
            Username = "username",
        };
        await model.RequestNextStepAsync(CancellationToken.None);
        model.Password = "invalid";

        // Act
        bool shouldClose = await model.RequestNextStepAsync(CancellationToken.None);

        // Assert
        shouldClose.Should().BeFalse();
        model.Step.Should().Be(UsernamePasswordStep.RequestPassword);
    }

    [Fact]
    public async Task RequestNextStepAsync_ShouldReportPasswordError_WhenCheckFails()
    {
        // Arrange
        var model = new UsernamePasswordViewModel
        {
            CheckHandler = (_, _, _) => Task.FromResult(false),
            Username = "username",
        };
        await model.RequestNextStepAsync(CancellationToken.None);
        model.Password = "password";

        // Act
        bool shouldClose = await model.RequestNextStepAsync(CancellationToken.None);

        // Assert
        shouldClose.Should().BeFalse();
        model.Step.Should().Be(UsernamePasswordStep.RequestPassword);
        model
            .GetErrors(nameof(UsernamePasswordViewModel.Password))
            .Select(x => x.ErrorMessage)
            .Should()
            .Equal("Username or password are invalid");
        model.IsCurrentStepValid.Should().BeFalse();
    }

    [Fact]
    public async Task Password_ShouldClearCheckError_WhenEdited()
    {
        // Arrange
        var model = new UsernamePasswordViewModel
        {
            CheckHandler = (_, _, _) => Task.FromResult(false),
            Username = "username",
        };
        await model.RequestNextStepAsync(CancellationToken.None);
        model.Password = "password";
        await model.RequestNextStepAsync(CancellationToken.None);

        // Act
        model.Password = "other password";

        // Assert
        model.GetErrors(nameof(UsernamePasswordViewModel.Password)).Should().BeEmpty();
        model.IsCurrentStepValid.Should().BeTrue();
    }

    [Fact]
    public async Task RequestNextStepAsync_ShouldNotReportCheckError_WhenPasswordChangedDuringCheck()
    {
        // Arrange
        var model = new UsernamePasswordViewModel { Username = "username" };
        model.CheckHandler = (_, _, _) =>
        {
            model.Password = "edited while checking";
            return Task.FromResult(false);
        };
        await model.RequestNextStepAsync(CancellationToken.None);
        model.Password = "password";

        // Act
        bool shouldClose = await model.RequestNextStepAsync(CancellationToken.None);

        // Assert
        shouldClose.Should().BeFalse();
        model.GetErrors(nameof(UsernamePasswordViewModel.Password)).Should().BeEmpty();
    }

    [Fact]
    public async Task Username_ShouldClearCheckError_WhenEdited()
    {
        // Arrange
        var model = new UsernamePasswordViewModel
        {
            CheckHandler = (_, _, _) => Task.FromResult(false),
            Username = "usrname",
        };
        await model.RequestNextStepAsync(CancellationToken.None);
        model.Password = "password";
        await model.RequestNextStepAsync(CancellationToken.None);
        model.RequestUsernameStepCommand.Execute(null);

        // Act
        model.Username = "username";
        await model.RequestNextStepAsync(CancellationToken.None);

        // Assert
        model.Step.Should().Be(UsernamePasswordStep.RequestPassword);
        model.GetErrors(nameof(UsernamePasswordViewModel.Password)).Should().BeEmpty();
        model.IsCurrentStepValid.Should().BeTrue();
    }
}
