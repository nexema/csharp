using MessagePackSchema.Test.Utils;

namespace MessagePackSchema.Test
{
    public class TestSerialize
    {
        [Test]
        public void Test_Serialize()
        {
            DefaultType instance = new()
            {
                Name = "the name of the type",
                Names = new List<string> { "a", "hello world", "z", "5145", "b" },
                MyUint16 = 54512
            };

            var buffer = instance.Serialize();
            Assert.That(buffer, Is.EqualTo(instance.SerializeDebug()));
        }
    }
}