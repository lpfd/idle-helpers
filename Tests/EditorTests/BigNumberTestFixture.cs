using LeapForward.IdleHelpers;
using NUnit.Framework;

[TestFixture]
public class BigNumberTestFixture
{
    [Test]
    public void Add()
    {
        BigNumber a = 1;
        BigNumber b = 42;

        BigNumber c = a + b;

        Assert.AreEqual((BigNumber)43, c);
    }

    [Test]
    public void Subtract()
    {
        BigNumber a = 1;
        BigNumber b = 42;

        BigNumber c = a - b;

        Assert.AreEqual(-41, (int)c);
    }

    [Test]
    public void Negate()
    {
        BigNumber a = 42;

        BigNumber c = -a;

        Assert.AreEqual(-42, (int)c);
    }

    [Test]
    public void Multiply()
    {
        BigNumber a = 100;
        BigNumber b = 1000;

        BigNumber c = a * b;

        Assert.AreEqual((BigNumber)100000, c);
    }

    [Test]
    public void Divide()
    {
        BigNumber a = 1000;
        BigNumber b = 100;

        BigNumber c = a / b;

        Assert.AreEqual((BigNumber)10, c);
    }

    [Test]
    public void Power()
    {
        BigNumber a = 1000;
        double b = 100;

        BigNumber c = a.Pow(b);

        Assert.AreEqual(1e300, (double)c);
    }

    [Test]
    [TestCase(-1, 0)]
    [TestCase(-1, 1)]
    [TestCase(-1, 10)]
    [TestCase(-10, 0)]
    [TestCase(-10, 1)]
    [TestCase(-10, 10)]
    [TestCase(1, 2)]
    [TestCase(1, 10)]
    [TestCase(10, 12)]
    public void LessAndGreater(int a, int b)
    {
        BigNumber left = a;
        BigNumber right = b;

        Assert.Less(a, b);
        Assert.Greater(b, a);
    }
}