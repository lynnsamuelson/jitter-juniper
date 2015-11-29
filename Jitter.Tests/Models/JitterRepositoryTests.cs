using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jitter.Models;
using System.Collections.Generic;
using Moq;
using System.Data.Entity;
using System.Linq;

namespace Jitter.Tests.Models
{
    [TestClass]
    public class JitterRepositoryTests
    {
        private Mock<JitterContext> mock_context;
        private Mock<DbSet<JitterUser>> mock_set;
        private JitterRepository repository;

        private void ConnectMocksToDataStore(IEnumerable<JitterUser> data_store)
        {
            var data_source = data_store.AsQueryable<JitterUser>();
            // HINT HINT: var data_source = (data_store as IEnumerable<JitterUser>).AsQueryable();
            // Convince LINQ that our Mock DbSet is a (relational) Data store.
            mock_set.As<IQueryable<JitterUser>>().Setup(data => data.Provider).Returns(data_source.Provider);
            mock_set.As<IQueryable<JitterUser>>().Setup(data => data.Expression).Returns(data_source.Expression);
            mock_set.As<IQueryable<JitterUser>>().Setup(data => data.ElementType).Returns(data_source.ElementType);
            mock_set.As<IQueryable<JitterUser>>().Setup(data => data.GetEnumerator()).Returns(data_source.GetEnumerator());
            
            // This is Stubbing the JitterUsers property getter
            mock_context.Setup(a => a.JitterUsers).Returns(mock_set.Object);
        }

        [TestInitialize]
        public void Initialize()
        {
            mock_context = new Mock<JitterContext>();
            mock_set = new Mock<DbSet<JitterUser>>();
            repository = new JitterRepository(mock_context.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            mock_context = null;
            mock_set = null;
            repository = null;
        }

        [TestMethod]
        public void JitterContextEnsureICanCreateInstance()
        {
            JitterContext context = mock_context.Object;
            Assert.IsNotNull(context);
        }

        [TestMethod]
        public void JitterRepositoryEnsureICanCreatInstance()
        {
            Assert.IsNotNull(repository);
        }

        [TestMethod]
        public void JitterRepositoryEnsureICanGetAllUsers()
        {
            // Arrange
            var expected = new List<JitterUser>
            {
                new JitterUser {Handle = "adam1" },
                new JitterUser { Handle = "rumbadancer2"}
            };
            mock_set.Object.AddRange(expected);

            ConnectMocksToDataStore(expected);

            // Act
            var actual = repository.GetAllUsers();
            // Assert

            Assert.AreEqual("adam1", actual.First().Handle);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JitterRepositoryEnsureIHaveAContext()
        {
            // Arrange
            // Act
            var actual = repository.Context;
            // Assert
            Assert.IsInstanceOfType(actual, typeof(JitterContext));
        }

        [TestMethod]
        public void JitterRepositoryEnsureICanGenUserByHandle()
        {
            // Arrange
            var expected = new List<JitterUser>
            {
                new JitterUser {Handle = "adam1" },
                new JitterUser { Handle = "rumbadancer2"}
            };
            mock_set.Object.AddRange(expected);

            ConnectMocksToDataStore(expected);
            // Act
            string handle = "rumbadancer2";
            JitterUser actual_user = repository.GetUserByHandle(handle);
            // Assert
            Assert.AreEqual("rumbadancer2", actual_user.Handle);
        }

        [TestMethod]
        public void JitterRepositoryGetUserByHandleUserDoesNotExist()
        {
            // Arrange
            var expected = new List<JitterUser>
            {
                new JitterUser {Handle = "adam1" },
                new JitterUser { Handle = "rumbadancer2"}
            };
            mock_set.Object.AddRange(expected);

            ConnectMocksToDataStore(expected);
            // Act
            string handle = "bogus";
            JitterUser actual_user = repository.GetUserByHandle(handle);
            // Assert
            Assert.IsNull(actual_user);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void JitterRepositoryGetUserByHandleFailsMultipleUsers()
        {
            // Arrange
            var expected = new List<JitterUser>
            {
                new JitterUser {Handle = "adam1" },
                new JitterUser { Handle = "adam1"}
            };
            mock_set.Object.AddRange(expected);

            ConnectMocksToDataStore(expected);
            // Act
            string handle = "adam1";
            JitterUser actual_user = repository.GetUserByHandle(handle);
            // Assert
        }

        [TestMethod]
        public void JitterRepositoryEnsureHandleIsAvailable()
        {
            // Arrange
            var expected = new List<JitterUser>
            {
                new JitterUser {Handle = "adam1" },
                new JitterUser { Handle = "rumbadancer2"}
            };
            mock_set.Object.AddRange(expected);

            ConnectMocksToDataStore(expected);
            // Act
            string handle = "bogus";
            bool is_available = repository.IsHandleAvailable(handle);
            // Assert
            Assert.IsTrue(is_available);
        }

        [TestMethod]
        public void JitterRepositoryEnsureHandleIsNotAvailable()
        {
            // Arrange
            var expected = new List<JitterUser>
            {
                new JitterUser {Handle = "adam1" },
                new JitterUser { Handle = "rumbadancer2"}
            };
            mock_set.Object.AddRange(expected);

            ConnectMocksToDataStore(expected);
            // Act
            string handle = "adam1";
            bool is_available = repository.IsHandleAvailable(handle);
            // Assert
            Assert.IsFalse(is_available);
           
        }

        [TestMethod]
        public void JitterRepositoryEnsureHandleIsNotAvailableMultipleUsers()
        {
            // Arrange
            var expected = new List<JitterUser>
            {
                new JitterUser {Handle = "adam1" },
                new JitterUser { Handle = "adam1"}
            };
            mock_set.Object.AddRange(expected);

            ConnectMocksToDataStore(expected);
            // Act
            string handle = "adam1";
            bool is_available = repository.IsHandleAvailable(handle);
            // Assert
            Assert.IsFalse(is_available);
        }

        [TestMethod]
        public void JitterRepositoryEnsureICanSearchByHandle()
        {
            // Arrange
            var expected = new List<JitterUser>
            {
                new JitterUser { Handle = "adam1" },
                new JitterUser { Handle = "rumbadancer2"},
                new JitterUser { Handle = "treehugger" },
                new JitterUser { Handle = "treedancer"}
                
            };
            mock_set.Object.AddRange(expected);

            ConnectMocksToDataStore(expected);
            // Act
            string handle = "tree";
            List<JitterUser> expected_users = new List<JitterUser>
            {
                new JitterUser { Handle = "treedancer"},
                new JitterUser { Handle = "treehugger" }
            };
            List<JitterUser> actual_users = repository.SearchByHandle(handle);
            // Assert
            // There be :dragon:. Collection Assert Doesn't use CompareTo underneath the hood. 
            // CollectionAssert.AreEqual(expected_users, actual_users);
            Assert.AreEqual(expected_users[0].Handle, actual_users[0].Handle);
            Assert.AreEqual(expected_users[1].Handle, actual_users[1].Handle);
        }

        [TestMethod]
        public void JitterRepositoryEnsureICanSearchByFirstName()
        {
            // Arrange
            var expected = new List<JitterUser>
            {
                new JitterUser { Handle = "Adam1", FirstName = "Adam" },
                new JitterUser { Handle = "HappyToes", FirstName = "Ruby"},
                new JitterUser { Handle = "baseball25", FirstName = "Noah" },
                new JitterUser { Handle = "superhero1", FirstName = "Sam" },
                new JitterUser { Handle = "batman", FirstName = "Samuel"}

            };
            mock_set.Object.AddRange(expected);

            ConnectMocksToDataStore(expected);
            // Act
            string firstName = "Sam";
            List<JitterUser> expected_users = new List<JitterUser>
            {
                new JitterUser { Handle = "batman", FirstName = "Samuel"},
                new JitterUser { Handle = "superhero1", FirstName = "Sam" }
            };
            List<JitterUser> actual_users = repository.SearchByFirstName(firstName);
            // Assert
            //Assert.AreEqual("Sam", firstName);
            Assert.AreEqual(expected_users[0].FirstName, actual_users[0].FirstName);
            Assert.AreEqual(expected_users[1].FirstName, actual_users[1].FirstName);
        }

        [TestMethod]
        public void JitterRepositoryEnsureICanSearchByLastName()
        {
            // Arrange
            var expected = new List<JitterUser>
            {
                new JitterUser { Handle = "adam1", LastName = "Rice" },
                new JitterUser { Handle = "rumbadancer2", LastName = "Samuelson"},
                new JitterUser { Handle = "treehugger", LastName = "Olson" },
                new JitterUser { Handle = "treedancer", LastName = "Rice"}

            };
            mock_set.Object.AddRange(expected);

            ConnectMocksToDataStore(expected);
            // Act
            string lastName = "Rice";
            List<JitterUser> expected_users = new List<JitterUser>
            {
                new JitterUser { Handle = "adam1", LastName = "Rice" },
                new JitterUser { Handle = "treedancer", LastName = "Rice" }
            };
            List<JitterUser> actual_users = repository.SearchByLastName(lastName);
            
            Assert.AreEqual(expected_users[0].LastName, actual_users[0].LastName);
            Assert.AreEqual(expected_users[1].LastName, actual_users[1].LastName);
        }

        [TestMethod]
        public void JitterRepositoryEnsureICanSearchByFirstOrLastName()
        {
            // Arrange
            var expected = new List<JitterUser>
            {
                new JitterUser { Handle = "adam1", FirstName = "Adam", LastName = "Rice" },
                new JitterUser { Handle = "rumbadancer2", FirstName = "Ruby", LastName = "Samuelson"},
                new JitterUser { Handle = "treehugger", FirstName = "Samuel", LastName = "Olson" },
                new JitterUser { Handle = "treedancer", FirstName = "Wade", LastName = "Rice"}

            };
            mock_set.Object.AddRange(expected);

            ConnectMocksToDataStore(expected);
            // Act
            string searchName = "Samuel";
            List<JitterUser> expected_users = new List<JitterUser>
            {
                new JitterUser { Handle = "rumbadancer2", FirstName = "Ruby", LastName = "Samuelson"},
                new JitterUser { Handle = "treehugger", FirstName = "Samuel", LastName = "Olson" }
            };
            List<JitterUser> actual_users = repository.SearchByFirstOrLastName(searchName);

            Assert.AreEqual(expected_users[0].LastName, actual_users[0].LastName);
            Assert.AreEqual(expected_users[1].LastName, actual_users[1].LastName);
            Assert.AreEqual(expected_users[0].FirstName, actual_users[0].FirstName);
            Assert.AreEqual(expected_users[1].FirstName, actual_users[1].FirstName);


        }

    }
}
