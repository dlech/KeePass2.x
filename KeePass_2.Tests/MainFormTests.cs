using KeePass.Forms;

namespace KeePass.Tests
{
    [TestClass]
    public class MainFormTests
    {
        [TestMethod]
        public void Constructor_DoesNotThrow()
        {
            // Basic test to ensure constructor does not throw
            var form = new MainForm();
            Assert.IsNotNull(form);
        }
    }
}