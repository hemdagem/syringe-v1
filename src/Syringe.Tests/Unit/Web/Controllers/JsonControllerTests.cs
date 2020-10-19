﻿using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Syringe.Core.Security;
using Syringe.Core.Services;
using Syringe.Core.Tasks;
using Syringe.Core.Tests;
using Syringe.Web.Controllers;

namespace Syringe.Tests.Unit.Web.Controllers
{
    [TestFixture]
    public class JsonControllerTests
    {
        private Mock<ITasksService> _tasksClient;
        private Mock<ITestService> _testsClient;
        private Mock<IUserContext> _userContext;
        private JsonController jsonController;
        [SetUp]
        public void Setup()
        {
            _tasksClient = new Mock<ITasksService>();
            _testsClient = new Mock<ITestService>();
            _userContext = new Mock<IUserContext>();

            _tasksClient.Setup(x => x.Start(It.IsAny<TaskRequest>())).Returns(10);
            _tasksClient.Setup(x => x.GetTask(It.IsAny<int>())).Returns(new TaskDetails());
            _testsClient.Setup(x => x.GetTestFile(It.IsAny<string>())).Returns(new TestFile());
            jsonController = new JsonController(_tasksClient.Object, _testsClient.Object, _userContext.Object);
        }

        [Test]
        public void Run_should_return_correct_json()
        {
            // given + when
            var actionResult = jsonController.Run(It.IsAny<string>()) as JsonResult;

            // then
            _tasksClient.Verify(x => x.Start(It.IsAny<TaskRequest>()), Times.Once);
            Assert.AreEqual("{ taskId = 10 }", actionResult.Data.ToString());
        }

        [Test]
        public void GetTests_should_return_correct_json()
        {
            // given + when
            var actionResult = jsonController.GetTests(It.IsAny<string>()) as ContentResult;

            // then
            _testsClient.Verify(x => x.GetTestFile(It.IsAny<string>()), Times.Once);
            Assert.AreEqual("{\"Tests\":[],\"Filename\":null,\"Variables\":[],\"EngineVersion\":0}", actionResult.Content);
        }
    }
}
