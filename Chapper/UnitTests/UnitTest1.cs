using System;
using Chapper;
using StorageClient;
using Microsoft.AspNet.SignalR.Hubs;
using Moq;
using System.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;

namespace UnitTests
{
    public class FakeStorage : IStorageClient
    {

        public KeyValuePair<string, string> MessageText
        {
            get;
            private set;
        }
        public string LoginName
        {
            get;
            private set;
        }
        public string LogoutName
        {
            get;
            private set;
        }

        public void StorageMessage(string name, string message)
        {
            this.MessageText = new KeyValuePair<string, string>(name, message);
        }
        public void StorageLogin(string name)
        {
            this.LoginName = name;
        }
        public void StorageLogout(string name)
        {
            this.LogoutName = name;
        }
    }

    [TestClass]
    public class Tests
    {
        /// <summary>
        /// Test that login works properly
        /// </summary>
        [TestMethod]
        public void Test_01()
        {
            bool usersonlineSent = false;
            bool newUserCalled = false;
            bool nameMatches = false;
            bool nameFound = false;

            string userName = "TestUser01";
            var fakeStorage = new FakeStorage();
            var hub = new ChappHub(fakeStorage);

            var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
            // var all = new Mock<HubCallerContext>();

            var mockContext = new Mock<HubCallerContext>();
            mockContext.Setup(m => m.ConnectionId).Returns("1");

            hub.Context = mockContext.Object;
            hub.Clients = mockClients.Object;

            dynamic caller = new ExpandoObject();
            dynamic others = new ExpandoObject();
            caller.usersOnline = new Action<string[]>((names) =>
            {
                usersonlineSent = true;
                nameFound = names.Contains(userName);
            });
            others.broadcastNewUser = new Action<string>((name) =>
            {
                nameMatches = (name == userName);
                newUserCalled = true;
            });

            mockClients.Setup(m => m.Others).Returns((ExpandoObject)others);
            mockClients.Setup(m => m.Caller).Returns((ExpandoObject)caller);

            hub.Login(userName);

            // Make sure that usersonline was sent
            Assert.IsTrue(usersonlineSent, "Users online was never called!");
            // Make sure that username is found from the list
            Assert.IsTrue(nameFound, "Username was not found from the usersonline list");
            // Make sure that toher user have  received the correct username
            Assert.IsTrue(nameMatches, "Username was not sent to other users");
            // Make sure that the others were indicated about the new user
            Assert.IsTrue(newUserCalled, "New user was never called!");

            // Check that username was given to storage
            Assert.AreEqual(userName, fakeStorage.LoginName, "Storage: username did not match");
        }

        /// <summary>
        /// Test that message sending works properly
        /// </summary>
        [TestMethod]
        public void Test_02()
        {
            System.Threading.Thread.Sleep(500);
            bool sendCalled = false;
            bool messageMatched = false;
            bool nameMatches = false;
            string userName = "TestUser02";
            string testMessage = "TestMessage";

            var fakeStorage = new FakeStorage();
            var hub = new ChappHub(fakeStorage);

            var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
            // var all = new Mock<HubCallerContext>();

            var mockContext = new Mock<HubCallerContext>();
            mockContext.Setup(m => m.ConnectionId).Returns("2");

            hub.Context = mockContext.Object;
            hub.Clients = mockClients.Object;

            dynamic caller2 = new ExpandoObject();
            dynamic others2 = new ExpandoObject();
            caller2.usersOnline = new Action<string[]>((names) => {
                bool how = names.Contains(userName);
            });
            others2.broadcastNewUser = new Action<string>((name) => { });

            dynamic all2 = new ExpandoObject();

            all2.broadcastMessage = new Action<string, string>((name, message) =>
            {
                sendCalled = true;
                messageMatched = (testMessage == message);
                nameMatches = (userName == name);
            });

            mockClients.Setup(m => m.All).Returns((ExpandoObject)all2);
            mockClients.Setup(m => m.Others).Returns((ExpandoObject)others2);
            mockClients.Setup(m => m.Caller).Returns((ExpandoObject)caller2);

            hub.Login(userName);
            hub.Send(testMessage);

            // Check results
            // Make sure that broadcastMessage was called
            Assert.IsTrue(sendCalled,"Send was not called");
            // Make sure that the message is same that was sent
            Assert.IsTrue(messageMatched,"Message did not match");
            // Make sure that username matches with the sender
            Assert.IsTrue(nameMatches,"Username did not match");

            // Check that message was properly sent to storage
            Assert.AreEqual(userName, fakeStorage.MessageText.Key, "Storage: username did not match");
            Assert.AreEqual(testMessage, fakeStorage.MessageText.Value, "Storage: message did not match");
        }


        /// <summary>
        /// Test that logout works properly
        /// </summary>
        [TestMethod]
        public void Test_03()
        {
            bool usersonlineSent = false;
            bool nameFound = false;

            string userName = "TestUser03";

            var fakeStorage = new FakeStorage();
            var hub = new ChappHub(fakeStorage);

            var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
            // var all = new Mock<HubCallerContext>();

            var mockContext = new Mock<HubCallerContext>();
            mockContext.Setup(m => m.ConnectionId).Returns("3");

            hub.Context = mockContext.Object;
            hub.Clients = mockClients.Object;

            dynamic others = new ExpandoObject();
            others.usersOnline = new Action<string[]>((names) =>
            {
                usersonlineSent = true;
                nameFound = !names.Contains(userName);
            });

            mockClients.Setup(m => m.Others).Returns((ExpandoObject)others);

            hub.Logout(userName);

            // Make sure that users online was sent to other users
            Assert.IsTrue(usersonlineSent, "Users online was never called!");
            // Make sure that the user name of logged out user is not in online users list
            Assert.IsTrue(nameFound, "Username was not found from the usersonline list");

            // Check that the logout username was sent to store
            Assert.AreEqual(userName, fakeStorage.LogoutName, "Storage: username did not match");
        }
    }
}
