using MessagePackSchema.Test.Utils;

namespace MessagePackSchema.Test
{
    public class TestDeserializer
    {
        [Test]
        public void Test_Serialize()
        {
            var buffer = new DefaultType()
            {
                Name = "the name of the type",
                Names = new List<string> { "a", "hello world", "z", "5145", "b" },
                MyUint16 = 54512
            }.SerializeDebug();

            var instance = DefaultType.Deserialize(buffer);

            Assert.That(instance, Is.EqualTo(new DefaultType
            {
                Name = "the name of the type",
                Names = new List<string> { "a", "hello world", "z", "5145", "b" },
                MyUint16 = 54512
            }));
        }
    }
}