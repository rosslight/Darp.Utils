namespace Darp.Utils.Dialog.Tests;

using System.ComponentModel.DataAnnotations;
using DialogData;
using FluentAssertions;

public class InputDialogViewModelTests
{
    [Fact]
    public void TryGetResultData_ShouldReturnFalse_WhenInputHasValidationErrors()
    {
        // Arrange
        var model = new InputDialogViewModel
        {
            ValidateInputCallback = _ => new ValidationResult("Invalid input"),
            Input = "invalid",
        };

        // Act
        bool result = model.TryGetResultData(out string? resultData);

        // Assert
        result.Should().BeFalse();
        resultData.Should().BeNull();
    }
}
