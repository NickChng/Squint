//using System.Windows.Forms;

namespace Squint
{

        public static class IDGenerator
        {
            private static int _IDbase = -2;
            private static object LockIDGen = new object();
            public static int GetUniqueId()
            {
                lock (LockIDGen)
                {
                    _IDbase--;
                    return _IDbase;
                }
            }
        }
}
