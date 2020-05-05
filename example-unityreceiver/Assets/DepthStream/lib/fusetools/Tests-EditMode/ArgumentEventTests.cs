using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace FuseTools.Tests
{
	public class ArgumentEventsTests
	{
		[Test]
		public void FloatEventTest()
		{
			FloatEvent e = new FloatEvent();
			float counter = 0;
			e.AddListener((val) => counter += 1.0f + val);
			Assert.AreEqual(counter, 0);
			e.Invoke(1.5f);
			Assert.AreEqual(counter, 2.5f);
			e.Invoke(2.0f);
			e.Invoke(0.5f);
			Assert.AreEqual(counter, 7.0f);
		}

		[Test]
        public void DoubleEventTest()
        {
            DoubleEvent e = new DoubleEvent();
            double counter = 0;
            e.AddListener((val) => counter += 1.0 + val);
            Assert.AreEqual(counter, 0);
            e.Invoke(1.5);
            Assert.AreEqual(counter, 2.5);
            e.Invoke(2.0);
            e.Invoke(0.5);
            Assert.AreEqual(counter, 7.0);
        }

		[Test]
        public void IntEventTest()
        {
            var e = new IntEvent();
            double counter = 0;
            e.AddListener((val) => counter += 1.0 + val);
            Assert.AreEqual(counter, 0);
            e.Invoke(1);
            Assert.AreEqual(counter, 2);
            e.Invoke(-2);
            e.Invoke(3);
            Assert.AreEqual(counter, 5);
        }
      
		[Test]
        public void UintEventTest()
        {
            var e = new UintEvent();
            double counter = 0;
            e.AddListener((val) => counter += 1.0 + val);
            Assert.AreEqual(counter, 0);
            e.Invoke(1);
            Assert.AreEqual(counter, 2);
            e.Invoke(2);
            e.Invoke(3);
            Assert.AreEqual(counter, 9);
        }
      
		[Test]
        public void StringEventTest()
        {
            var e = new StringEvent();
            string counter = "";
            e.AddListener((val) => counter += "-"+val);
            Assert.AreEqual(counter, "");
            e.Invoke("1");
            Assert.AreEqual(counter, "-1");
            e.Invoke("22");
            e.Invoke("33");
            Assert.AreEqual(counter, "-1-22-33");
        }
      
		[Test]
        public void BoolEventTest()
        {
            var e = new BoolEvent();
            int counter = 0;
			e.AddListener((val) => counter += (1 + (val ? 1 : 0)));
            Assert.AreEqual(counter, 0);
            e.Invoke(false);
            Assert.AreEqual(counter, 1);
            e.Invoke(true);
            e.Invoke(true);
            Assert.AreEqual(counter, 5);
        }

		[Test]
        public void Vector2EventTest()
        {
            var e = new Vector2Event();
            Vector2 counter = new Vector2();
            e.AddListener((val) => counter += val);
            Assert.AreEqual(counter, new Vector2(0, 0));
            e.Invoke(new Vector2(1, 0));
            Assert.AreEqual(counter, new Vector2(1, 0));
            e.Invoke(new Vector2(0, 1));
            e.Invoke(new Vector2(1, 0));
            Assert.AreEqual(counter, new Vector2(2, 1));
        }

		[Test]
        public void Vector3EventTest()
        {
            var e = new Vector3Event();
            Vector3 counter = new Vector3();
            e.AddListener((val) => counter += val);
            Assert.AreEqual(counter, new Vector3(0,0,0));
            e.Invoke(new Vector3(1,0,0));
			Assert.AreEqual(counter, new Vector3(1, 0, 0));
			e.Invoke(new Vector3(0, 1, 0));
			e.Invoke(new Vector3(0, 0, 1));
			Assert.AreEqual(counter, new Vector3(1, 1, 1));
        }
    }
}