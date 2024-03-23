using Database;
using NUnit.Framework;

public class DatabaseConnectorTest
{
    [Test]
    public void DatabaseRootTest()
    {
        Assert.IsNotNull(DatabaseConnector.Instance.Root);
    }
}