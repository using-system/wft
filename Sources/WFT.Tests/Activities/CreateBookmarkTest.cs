using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFT.Activities;

namespace WFT.Tests.Activities
{
    [TestClass]
    public class CreateBookmarkTest
    {
        [TestMethod]
        public void CreateOneBookmarkActivityOKTest()
        {
            int bookmarkCpt = 0;

            new CreateBookmark()
            {
                BookmarkName = "MyBookmark"
            }.RunSync(
            (A, B) =>
            {
                Assert.AreEqual("MyBookmark", B.BookmarkName);
                bookmarkCpt++;
                A.ResumeBookmark(B.BookmarkName, null);
            });

            Assert.AreEqual(1, bookmarkCpt);
        }

        [TestMethod]
        public void CreateOnBookmarkActivityWithValueOKTest()
        {
            int bookmarkCpt = 0;

            Variable<int> result = new Variable<int>("result");

            new Sequence()
            {
                Variables = { result },
                Activities = 
                {
                    new CreateBookmark<int>()
                    {
                        BookmarkName = "MyBookmark",
                        Result = result
                    },
                    new If((C) => C.GetReferenceValue<int>(result) != 4)
                    {
                        Then = new Throw()
                        {
                            Exception = new InArgument<Exception>((C) => new NotSupportedException())
                        }
                    }
                }
            }.RunSync(
            (A, B) =>
            {
                Assert.AreEqual("MyBookmark", B.BookmarkName);
                bookmarkCpt++;
                A.ResumeBookmark(B.BookmarkName, 4);
            });

            Assert.AreEqual(1, bookmarkCpt);
        }
    }
}
