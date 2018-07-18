using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Makaretu
{
    /// <summary>
    ///   Asserting an <see cref="Exception"/>.
    /// </summary>
    public static class ExceptionAssert
    {

        public static T Throws<T>(Action action, string expectedMessage = null) where T : Exception
        {
            try
            {
                action();
            }
            catch (AggregateException e)
            {
                var match = e.InnerExceptions.OfType<T>().FirstOrDefault();
                if (match != null)
                {
                    if (expectedMessage != null)
                        Assert.AreEqual(expectedMessage, match.Message, "Wrong exception message.");
                    return match;
                }

                throw;
            }
            catch (T e)
            {
                if (expectedMessage != null)
                    Assert.AreEqual(expectedMessage, e.Message);
                return e;
            }
            catch (Exception e)
            {
                Assert.Fail("Exception of type {0} should be thrown not {1}.", typeof(T), e.GetType());
            }

            Assert.Fail("Expected Exception of type {0} but nothing was thrown.", typeof(T));
            return null;
        }

        public static Exception Throws(Action action, string expectedMessage = null)
        {
            return Throws<Exception>(action, expectedMessage);
        }

    }
}
