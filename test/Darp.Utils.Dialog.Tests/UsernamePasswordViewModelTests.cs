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
}
