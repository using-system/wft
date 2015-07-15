using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Activities.Statements.Fakes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFT.Activities;

namespace WFT.Tests.Activities
{
    [TestClass]
    public class CrontabDelayTest
    {
        [TestMethod]
        public void CrontabDelayOKTest()
        {
            using(IDisposable context = ShimsContext.Create())
            {
                TimeSpan delay = TimeSpan.MinValue;
                ShimDelay.AllInstances.ExecuteNativeActivityContext = (D, C) =>
                {
                    delay =  D.Duration.Get(C);
                };

                new CrontabDelay()
                {
                    CronExpression = "0 12 3,9,15 8-10 *",
                    StartDate = new InArgument<DateTime>((C) => new DateTime(2014, 1, 5))
                }.Invoke();

                Assert.AreNotEqual(TimeSpan.MinValue, delay);
                Assert.AreEqual(new TimeSpan(210, 12, 0, 0) , delay);
            }

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void CrontabDelayWithBadExpressionKOTest()
        {
            new CrontabDelay()
            {
                CronExpression = "badexpression"
            }.Invoke();
        }
    }
}
