namespace Logitar.CQRS.Tests;

[Trait(Traits.Category, Categories.Unit)]
public class RetrySettingsTests
{
  [Fact(DisplayName = "Validate: it should not throw when the validation succeeded.")]
  public void Given_Succeeded_When_Validate_Then_NothingThrown()
  {
    RetrySettings settings = new()
    {
      RandomVariation = 1000,
      MaximumDelay = 1000
    };
    settings.Validate();
  }

  [Fact(DisplayName = "Validate: it should throw InvalidOperationException when the Exponential properties are not valid.")]
  public void Given_InvalidExponentialProperties_When_Validate_Then_InvalidOperationException()
  {
    RetrySettings settings = new()
    {
      Algorithm = RetryAlgorithm.Exponential,
      Delay = 0,
      ExponentialBase = 1
    };
    var exception = Assert.Throws<InvalidOperationException>(settings.Validate);
    string[] lines = exception.Message.Remove("\r").Split('\n');
    Assert.Equal(3, lines.Length);
    Assert.Equal("Validation failed.", lines[0]);
    Assert.Equal(" - 'Delay' must be greater than 0.", lines[1]);
    Assert.Equal(" - 'ExponentialBase' must be greater than 1.", lines[2]);
  }

  [Fact(DisplayName = "Validate: it should throw InvalidOperationException when the Fixed properties are not valid.")]
  public void Given_InvalidFixedProperties_When_Validate_Then_InvalidOperationException()
  {
    RetrySettings settings = new()
    {
      Algorithm = RetryAlgorithm.Fixed,
      MaximumDelay = 500
    };
    var exception = Assert.Throws<InvalidOperationException>(settings.Validate);
    string[] lines = exception.Message.Remove("\r").Split('\n');
    Assert.Equal(2, lines.Length);
    Assert.Equal("Validation failed.", lines[0]);
    Assert.Equal(" - 'MaximumDelay' must be 0 when 'Algorithm' is Fixed.", lines[1]);
  }

  [Fact(DisplayName = "Validate: it should throw InvalidOperationException when the Linear properties are not valid.")]
  public void Given_InvalidLinearProperties_When_Validate_Then_InvalidOperationException()
  {
    RetrySettings settings = new()
    {
      Algorithm = RetryAlgorithm.Linear,
      Delay = 0,
      MaximumDelay = 500
    };
    var exception = Assert.Throws<InvalidOperationException>(settings.Validate);
    string[] lines = exception.Message.Remove("\r").Split('\n');
    Assert.Equal(2, lines.Length);
    Assert.Equal("Validation failed.", lines[0]);
    Assert.Equal(" - 'Delay' must be greater than 0.", lines[1]);
  }

  [Theory(DisplayName = "Validate: it should throw InvalidOperationException when the Random properties are not valid.")]
  [InlineData(false)]
  [InlineData(true)]
  public void Given_InvalidRandomProperties_When_Validate_Then_InvalidOperationException(bool greater)
  {
    RetrySettings settings = new()
    {
      Algorithm = RetryAlgorithm.Random,
      Delay = 0,
      RandomVariation = greater ? 1 : -1,
      MaximumDelay = 1000
    };
    var exception = Assert.Throws<InvalidOperationException>(settings.Validate);
    string[] lines = exception.Message.Remove("\r").Split('\n');
    Assert.Equal(4, lines.Length);
    Assert.Equal("Validation failed.", lines[0]);
    Assert.Equal(" - 'Delay' must be greater than 0.", lines[1]);
    Assert.Equal(greater ? " - 'RandomVariation' must be less than or equal to 'Delay'." : " - 'RandomVariation' must be greater than 0.", lines[2]);
    Assert.Equal(" - 'MaximumDelay' must be 0 when 'Algorithm' is Random.", lines[3]);
  }

  [Fact(DisplayName = "Validate: it should throw InvalidOperationException when the shared properties are not valid.")]
  public void Given_InvalidSharedProperties_When_Validate_Then_InvalidOperationException()
  {
    RetrySettings settings = new()
    {
      Algorithm = (RetryAlgorithm)(-1),
      Delay = -1,
      MaximumDelay = -1,
      MaximumRetries = -1
    };
    var exception = Assert.Throws<InvalidOperationException>(settings.Validate);
    string[] lines = exception.Message.Remove("\r").Split('\n');
    Assert.Equal(5, lines.Length);
    Assert.Equal("Validation failed.", lines[0]);
    Assert.Equal(" - 'Delay' must be greater than or equal to 0.", lines[1]);
    Assert.Equal(" - 'MaximumDelay' must be greater than or equal to 0.", lines[2]);
    Assert.Equal(" - 'Algorithm' is not a valid retry algorithm.", lines[3]);
    Assert.Equal(" - 'MaximumRetries' must be greater than or equal to 0.", lines[4]);
  }
}
