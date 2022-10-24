namespace MessagePackSchema.Test
{
    public class TestGeneratedType
    {
        [Test]
        public void Test_Equals()
        {
            DefaultType defaultType = new()
            {
                Name = "the name of the type",
                Names = new List<string> { "a", "hello world", "z", "5145", "b" }
            };

            DefaultType anotherType = new()
            {
                Name = "the name of the type",
                Names = new List<string> { "a", "hello world", "z", "5145", "b" }
            };

            DefaultType differentType = new()
            {
                Name = "the name of the type",
                Names = new List<string> { "a", "z", "hello world", "5145", "b" }
            };

            Assert.That(defaultType, Is.EqualTo(anotherType));
            Assert.That(defaultType, Is.Not.EqualTo(differentType));
        }
    }
}