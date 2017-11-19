using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBusNetMQUnitTest
{
    [TestClass()]
    public class Startup
    {
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            StartupConfig.GetConfiguredContainer();
        }

        [AssemblyCleanup()]
        public static void AssemblyCleanup()
        {

        }
    }
}
