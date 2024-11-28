using Mobile.Secrets.SecretCollectorStrategies;

namespace UnitTests.SecretCollectorStrategies;

public class EnvironmentCollectorStrategy_MapSecrets_Tests
{
    private EnvironmentCollectorStrategy _environmentCollector;
    
    [SetUp]
    public void SetUp()
    {
        _environmentCollector = new EnvironmentCollectorStrategy("");
    }
    
    [TestCase("true", true)]
    [TestCase("True", true)]
    [TestCase("TRUE", true)]
    [TestCase("false", false)]
    [TestCase("False", false)]
    [TestCase("FALSE", false)]
    public void BooleanStringAsInput_ShouldReturnBooleanValue(string inputString, bool expected)
    {
        const string key = "BooleanKey";
        var input = BuildInput(key, inputString);

        var secrets = _environmentCollector.MapSecrets(input);
        var booleanValue = secrets[key];
        
        Assert.That(booleanValue, Is.EqualTo(expected)); 
    }

    [TestCase("3", 3)]
    [TestCase("24", 24)]
    [TestCase("2024", 2024)]
    [TestCase("9223372036854775807", 9223372036854775807)]
    [TestCase("-3", -3)]
    [TestCase("-24", -24)]
    [TestCase("-2024", -2024)]
    [TestCase("-9223372036854775808", -9223372036854775808)]
    public void LongStringAsInput_ShouldReturnLongValue(string inputString, long expected)
    {
        const string key = "IntKey";
        var input = BuildInput(key, inputString);

        var secrets = _environmentCollector.MapSecrets(input);
        var longValue = secrets[key];
        
        Assert.That(longValue, Is.EqualTo(expected));
    }

    [TestCase("TestString", "TestString")]
    [TestCase("[TestString", "[TestString")]
    [TestCase("", "")]
    public void StringAsInput_ShouldReturnStringValue(string inputString, string expected)
    {
        const string key = "StringKey";
        var input = BuildInput(key, inputString);

        var secrets = _environmentCollector.MapSecrets(input);
        var stringValue = secrets[key];
        
        Assert.That(stringValue, Is.EqualTo(expected)); 
    }

    [Test]
    public void NullAsInput_ShouldReturnEmptyString()
    {
        const string key = "StringKey";
        var input = BuildInput(key, null);

        var secrets = _environmentCollector.MapSecrets(input);
        Assert.That(secrets[key], Is.EqualTo(""));
    }

    [TestCase("[\"Element\"]", new[] { "Element" })]
    [TestCase("[\"Value1\", \"Value2\", \"Value3\"]", new[] { "Value1", "Value2", "Value3" })]
    [TestCase("[\"1\", \"2\", \"3\"]", new object[] { "1", "2", "3" })]
    [TestCase("[1, 2, 3]", new object[] { 1, 2, 3 })]
    [TestCase("[\"true\", \"false\"]", new object[] { "true", "false" })]
    [TestCase("[true, false]", new object[] { true, false })]
    [TestCase("[]", new object[] { })]
    public void ArrayStringAsInput_ShouldReturnArrayValue(string inputString, object[] expected)
    {
        const string key = "ArrayKey";
        var input = BuildInput(key, inputString);

        var secrets = _environmentCollector.MapSecrets(input);

        Assert.That(secrets[key], Is.EqualTo(expected)); 
    }

    [Test]
    public void DifferentTypesArrayStringInput_ShouldThrow()
    {
        var input = BuildInput("ArrayKey", "[\"Value1\", 2, \"Value3\"]");

        Assert.Throws<NotSupportedException>(() => _environmentCollector.MapSecrets(input));
    }

    private static List<KeyValuePair<string, string>> BuildInput(string key, string value)
    {
        return [new(key, value)];
    }
}