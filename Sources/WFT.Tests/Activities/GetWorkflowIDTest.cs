using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFT.Activities;

namespace WFT.Tests.Activities
{
    [TestClass]
    public class GetWorkflowIDTest
    {
        [TestMethod]
        public void GetWorflowIDOKTest()
        {
            GetWorkflowID activity = new GetWorkflowID();
            var results = activity.Invoke();

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results.ContainsKey("Result"));
            Assert.IsInstanceOfType(results["Result"], typeof(Guid));
            Assert.AreNotEqual(Guid.Empty, results["Result"]);
        }
    }
}
