using System;
using System.Linq;
using System.ServiceModel.Description;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Wcf.Tests.TestObjects;

namespace Unity.Wcf.Tests
{
    [TestClass]
    public class ServiceHostTest
    {
        [TestMethod]
        public void ShouldCreateServiceHost()
        {
            var fac = new TestServiceHostFactory();

            var host = fac.CreateServiceHost(typeof(TestService).AssemblyQualifiedName, new Uri[0]);

            Assert.IsInstanceOfType(host, typeof(UnityServiceHost));

            Assert.AreEqual(host.Description.ServiceType, typeof(TestService));
        }

        [TestMethod]
        public void TestBehaviors()
        {
            var servicebehavior = A.Fake<IContractBehavior>();

            var fac = A.Fake<Func<IUnityContainer, object>>();

            A.CallTo(fac).WithReturnType<object>().Returns(servicebehavior);


            using (var container = new UnityContainer()
                .RegisterFactory<IContractBehavior>("test", fac)
                .RegisterType<IServiceBehavior, TestServiceBehavior>("test"))
            using (var host = new UnityServiceHost(container, typeof(string)))
            {
                Assert.AreEqual(7, host.Description.Behaviors.Count);
                Assert.IsTrue(host.Description.Behaviors.Contains(typeof(TestServiceBehavior)));

                A.CallTo(fac).MustHaveHappenedOnceExactly();
            }
        }

        [TestMethod]
        public void TestCall()
        {
            var host = new TestSupport.TestHost<ITestService, TestService>();
            try
            {
                using (var container = new UnityContainer())
                //using (var http = new HttpClient { BaseAddress = baseAddress })
                {
                    // Enable metadata publishing.
                    //ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                    //smb.HttpGetEnabled = true;
                    //smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                    //host.Description.Behaviors.Add(smb);

                    // Open the ServiceHost to start listening for messages. Since
                    // no endpoints are explicitly configured, the runtime will create
                    // one endpoint per base address for each service contract implemented
                    // by the service.
                    host.Start();

                    Console.WriteLine("The service is ready at {0}", host.BaseAddress);
                    //Console.WriteLine("Press <Enter> to stop the service.");
                    //Console.ReadLine();

                    var proxy = host.GetProxy();
                    var res = proxy.GetData(5);
                }
            }
            finally
            {
                host.Stop();
            }
        }
    }
}
