using Mobile.Secrets.SecretCollectorStrategies;
using Newtonsoft.Json.Linq;

namespace UnitTests.SecretCollectorStrategies;

public class AppSettingsCollectorStrategy_MapSecrets_Tests
{
    private AppSettingsCollectorStrategy _appSettingsCollector;
    
    [SetUp]
    public void SetUp()
    {
        _appSettingsCollector = new AppSettingsCollectorStrategy("");
    }

    [TestCase("{\"IsProduction\": true}", true)]
    [TestCase("{\"IsProduction\": false}", false)]
    public void JObjectBooleanInput_ShouldReturnBooleanValue(string input, bool expected)
    {
        var jObject = JObject.Parse(input);
        
        var secrets = _appSettingsCollector.MapSecrets(jObject);
        var secret = secrets["IsProduction"];
        
        Assert.Multiple(() =>
        {
            Assert.That(secret is bool);
            Assert.That(secret, Is.EqualTo(expected));
        });
    }
    
    [TestCase("{\"DatabasePort\": 3}", 3)]
    [TestCase("{\"DatabasePort\": 24}", 24)]
    [TestCase("{\"DatabasePort\": 2024}", 2024)]
    [TestCase("{\"DatabasePort\": 9223372036854775807}", 9223372036854775807)]
    [TestCase("{\"DatabasePort\": -3}", -3)]
    [TestCase("{\"DatabasePort\": -24}", -24)]
    [TestCase("{\"DatabasePort\": -2024}", -2024)]
    [TestCase("{\"DatabasePort\": -9223372036854775808}", -9223372036854775808)]
    public void LongStringAsInput_ShouldReturnLongValue(string inputString, long expected)
    {
        var jObject = JObject.Parse(inputString);
        
        var secrets = _appSettingsCollector.MapSecrets(jObject);
        var longValue = secrets["DatabasePort"];
        
        Assert.Multiple(() =>
        {
            Assert.That(longValue is long);
            Assert.That(longValue, Is.EqualTo(expected)); 
        });
    }
    
    [TestCase("{\"RealisticConnectionString\": \"TestString\"}", "TestString")]
    [TestCase("{\"RealisticConnectionString\": \"[TestString\"}", "[TestString")]
    [TestCase("{\"RealisticConnectionString\": \"\"}", "")]
    public void StringAsInput_ShouldReturnStringValue(string inputString, string expected)
    {
        var jObject = JObject.Parse(inputString);
        
        var secrets = _appSettingsCollector.MapSecrets(jObject);
        var stringValue = secrets["RealisticConnectionString"];
        
        Assert.Multiple(() =>
        {
            Assert.That(stringValue is string);
            Assert.That(stringValue, Is.EqualTo(expected)); 
        });
    }
    
    [TestCase("{\"Array\": [\"Element\"] }", new[] { "Element" })]
    [TestCase("{\"Array\": [\"Value1\", \"Value2\", \"Value3\"] }", new[] { "Value1", "Value2", "Value3" })]
    [TestCase("{\"Array\": [\"1\", \"2\", \"3\"] }", new object[] { "1", "2", "3" })]
    [TestCase("{\"Array\": [1, 2, 3] }", new object[] { 1, 2, 3 })]
    [TestCase("{\"Array\": [\"true\", \"false\"] }", new object[] { "true", "false" })]
    [TestCase("{\"Array\": [true, false] }", new object[] { true, false })]
    [TestCase("{\"Array\": [] }", new object[] { })]
    public void ArrayStringAsInput_ShouldReturnArrayValue(string inputString, object[] expected)
    {
        var jObject = JObject.Parse(inputString);
        
        var secrets = _appSettingsCollector.MapSecrets(jObject);

        Assert.That(secrets["Array"], Is.EqualTo(expected)); 
    }
    
    [Test]
    public void DifferentTypesArrayStringInput_ShouldThrow()
    {
        var jObject = JObject.Parse("{\"Array\": [\"Value1\", 2, \"Value3\"] }");

        Assert.Throws<NotSupportedException>(() => _appSettingsCollector.MapSecrets(jObject));
    }

    [TestCase("{\"ConnectionStrings\": {\"Default\": \"A Realistic ConnectionString\"}}", "ConnectionStrings_Default")]
    [TestCase("{\"ConnectionStrings\": {\"Database\": {\"Default\": \"A Realistic ConnectionString\"}}}", "ConnectionStrings_Database_Default")]
    public void NestedObjectsAsInput_ShouldReturnUnderscoredSplitKeys(string inputString, string expectedKey)
    {
        var jObject = JObject.Parse(inputString);
        var secrets = _appSettingsCollector.MapSecrets(jObject);

        Assert.That(secrets, Contains.Key(expectedKey));
    }
}