﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Syringe.Core.Services;
using Syringe.Core.Tests;
using Syringe.Tests.StubsMocks.Web;
using Syringe.Web.Controllers;
using Syringe.Web.Controllers.Attribute;
using Syringe.Web.Mappers;
using Syringe.Web.Models;

namespace Syringe.Tests.Unit.Web.Controllers
{
    [TestFixture]
    public class TestControllerTests
    {
        private TestController _testController;
        private Mock<ITestService> _testServiceMock;
        private Mock<ITestFileMapper> _testFileMapperMock;

        [SetUp]
        public void Setup()
        {
            _testServiceMock = new Mock<ITestService>();
            _testFileMapperMock = new Mock<ITestFileMapper>();

            _testFileMapperMock.Setup(x => x.BuildTests(It.IsAny<IEnumerable<Test>>(), It.IsAny<int>(), It.IsAny<int>()));
            _testFileMapperMock.Setup(x => x.BuildVariableViewModel(It.IsAny<TestFile>())).Returns(new List<VariableViewModel>());
            _testServiceMock.Setup(x => x.GetTestFile(It.IsAny<string>())).Returns(new TestFile());
            _testServiceMock.Setup(x => x.DeleteTest(It.IsAny<int>(), It.IsAny<string>()));
            _testServiceMock.Setup(x => x.CreateTest(It.IsAny<string>(), It.IsAny<Test>()));

            _testController = new TestController(_testServiceMock.Object, _testFileMapperMock.Object);
        }

        [Test]
        public void Edit_should_return_correct_view_and_model()
        {
            // given
            const string expectedTestFileName = "gimme variables please";
            const int expectedPosition = 1;

            var expectedTestFile = new TestFile();
            _testServiceMock
                .Setup(x => x.GetTestFile(expectedTestFileName))
                .Returns(expectedTestFile);

            var expectedViewModel = new TestViewModel();
            _testFileMapperMock
                .Setup(x => x.BuildTestViewModel(expectedTestFile, expectedPosition, 1))
                .Returns(expectedViewModel);

            // when
            var viewResult = _testController.Edit(expectedTestFileName, expectedPosition) as ViewResult;

            // then
            Assert.AreEqual("Edit", viewResult.ViewName);

            var model = viewResult.Model as TestViewModel;
            Assert.That(model, Is.EqualTo(expectedViewModel));
        }

        [Test]
        public void Edit_should_be_decorated_with_httpGet_and_EditableTestsRequired()
        {
            // given + when
            var editMethod = typeof(TestController).GetMethod("Edit", new[] { typeof(string), typeof(int), typeof(int) });

            // then
            Assert.IsTrue(editMethod.IsDefined(typeof(HttpGetAttribute), false));
            Assert.IsTrue(editMethod.IsDefined(typeof(EditableTestsRequiredAttribute), false));
        }

        [Test]
        public void Edit_should_redirect_to_view_when_validation_succeeded()
        {
            // given
            _testController.ModelState.Clear();

            // when
            var redirectToRouteResult = _testController.Edit(new TestViewModel()) as RedirectToRouteResult;

            // then
            _testFileMapperMock.Verify(x => x.BuildTestObject(It.IsAny<TestViewModel>()), Times.Once);
            _testServiceMock.Verify(x => x.EditTest(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Test>()), Times.Once);
            Assert.AreEqual("View", redirectToRouteResult.RouteValues["action"]);
        }

        [Test]
        public void Edit_should_be_decorated_with_httpPost_and_ValidateInput_and_EditableTestsRequired()
        {
            // given + when
            var editMethod = typeof(TestController).GetMethod("Edit", new[] { typeof(TestViewModel) });

            // then
            Assert.IsTrue(editMethod.IsDefined(typeof(HttpPostAttribute), false));
            Assert.IsTrue(editMethod.IsDefined(typeof(EditableTestsRequiredAttribute), false));
            Assert.IsTrue(editMethod.IsDefined(typeof(ValidateInputAttribute), false));
        }

        [Test]
        public void Edit_should_return_correct_view_and_model_when_validation_failed_on_post()
        {
            // given
            _testController.ModelState.AddModelError("error", "error");

            // when
            var viewResult = _testController.Edit(new TestViewModel()) as ViewResult;

            // then
            _testFileMapperMock.Verify(x => x.BuildTestObject(It.IsAny<TestViewModel>()), Times.Never);
            _testServiceMock.Verify(x => x.EditTest(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Test>()), Times.Never);
            Assert.AreEqual("Edit", viewResult.ViewName);
            Assert.IsInstanceOf<TestViewModel>(viewResult.Model);
        }

        [Test]
        public void Copy_should_return_correct_redirection_to_view()
        {
            // given
            const int expectedPosition = 422;
            const string expectedFileName = "doobeedoo.dont.touch.me.there";

            // when
            var redirectToRouteResult = _testController.Copy(expectedPosition, expectedFileName) as RedirectToRouteResult;

            // then
            _testServiceMock.Verify(x => x.CopyTest(expectedPosition, expectedFileName), Times.Once);

            Assert.That(redirectToRouteResult, Is.Not.Null);
            Assert.That(redirectToRouteResult.RouteValues["action"], Is.EqualTo("View"));
            Assert.That(redirectToRouteResult.RouteValues["filename"], Is.EqualTo(expectedFileName));
        }

        [Test]
        public void Copy_should_be_decorated_with_httpPost_and_EditableTestsRequired()
        {
            // given + when
            var copyMethod = typeof(TestController).GetMethod("Copy", new[] { typeof(int), typeof(string) });

            // then
            Assert.IsTrue(copyMethod.IsDefined(typeof(HttpPostAttribute), false));
            Assert.IsTrue(copyMethod.IsDefined(typeof(EditableTestsRequiredAttribute), false));
        }

        [Test]
        public void Delete_should_return_correct_redirection_to_view()
        {
            // given + when
            var redirectToRouteResult = _testController.Delete(It.IsAny<int>(), It.IsAny<string>()) as RedirectToRouteResult;

            // then
            _testServiceMock.Verify(x => x.DeleteTest(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            Assert.AreEqual("View", redirectToRouteResult.RouteValues["action"]);
        }

        [Test]
        public void Delete_should_be_decorated_with_httpPost_and_EditableTestsRequired()
        {
            // given + when
            var deleteMethod = typeof(TestController).GetMethod("Delete", new[] { typeof(int), typeof(string) });

            // then
            Assert.IsTrue(deleteMethod.IsDefined(typeof(HttpPostAttribute), false));
            Assert.IsTrue(deleteMethod.IsDefined(typeof(EditableTestsRequiredAttribute), false));
        }

        [Test]
        public void CopyTest_should_return_and_save_test_with_copied_test()
        {
            // given

            // when
            var redirectToRouteResult = _testController.Delete(It.IsAny<int>(), It.IsAny<string>()) as RedirectToRouteResult;

            // then
            _testServiceMock.Verify(x => x.DeleteTest(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            Assert.AreEqual("View", redirectToRouteResult.RouteValues["action"]);
        }

        [Test]
        public void Add_should_return_correct_view_and_model()
        {
            // given
            const string expectedFilename = "This is Wayne's filename";
            var expectedTestFile = new TestFile();

            _testServiceMock
                .Setup(x => x.GetTestFile(expectedFilename))
                .Returns(expectedTestFile);

            var expectedVariable = new List<VariableViewModel>(2);
            _testFileMapperMock
                .Setup(x => x.BuildVariableViewModel(expectedTestFile))
                .Returns(expectedVariable);

            // when
            var viewResult = _testController.Add(expectedFilename) as ViewResult;

            // then
            Assert.AreEqual("Edit", viewResult.ViewName);
            _testFileMapperMock.Verify(x => x.BuildVariableViewModel(It.IsAny<TestFile>()), Times.Once);

            var model = viewResult.Model as TestViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model.Filename, Is.EqualTo(expectedFilename));
            Assert.That(model.AvailableVariables, Is.EqualTo(expectedVariable));
            Assert.That(model.ExpectedHttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(model.Method, Is.EqualTo(MethodType.GET));

            _testFileMapperMock.Verify(x => x.PopulateScriptSnippets(model), Times.Once);
        }

        [Test]
        public void Add_should_be_decorated_with_httpGet_and_EditableTestsRequired()
        {
            // given + when
            var addMethod = typeof(TestController).GetMethod("Add", new[] { typeof(string) });

            // then
            Assert.IsTrue(addMethod.IsDefined(typeof(HttpGetAttribute), false));
            Assert.IsTrue(addMethod.IsDefined(typeof(EditableTestsRequiredAttribute), false));
        }

        [Test]
        public void Add_should_redirect_to_view_when_validation_succeeded()
        {
            // given
            _testController.ModelState.Clear();

            // when
            var redirectToRouteResult = _testController.Add(new TestViewModel()) as RedirectToRouteResult;

            // then
            _testFileMapperMock.Verify(x => x.BuildTestObject(It.IsAny<TestViewModel>()), Times.Once);
            _testServiceMock.Verify(x => x.CreateTest(It.IsAny<string>(), It.IsAny<Test>()), Times.Once);
            Assert.AreEqual("View", redirectToRouteResult.RouteValues["action"]);
        }

        [Test]
        public void Add_should_be_decorated_with_httpPost_and_ValidateInput_and_EditableTestsRequired()
        {
            // given + when
            var editMethod = typeof(TestController).GetMethod("Add", new[] { typeof(TestViewModel) });

            // then
            Assert.IsTrue(editMethod.IsDefined(typeof(HttpPostAttribute), false));
            Assert.IsTrue(editMethod.IsDefined(typeof(EditableTestsRequiredAttribute), false));
            Assert.IsTrue(editMethod.IsDefined(typeof(ValidateInputAttribute), false));
        }

        [Test]
        public void Add_should_return_correct_view_and_model_when_validation_failed_on_post()
        {
            // given
            _testController.ModelState.AddModelError("error", "error");

            // when
            var viewResult = _testController.Add(new TestViewModel()) as ViewResult;

            // then
            _testFileMapperMock.Verify(x => x.BuildTestObject(It.IsAny<TestViewModel>()), Times.Never);
            _testServiceMock.Verify(x => x.CreateTest(It.IsAny<string>(), It.IsAny<Test>()), Times.Never);
            Assert.AreEqual("Edit", viewResult.ViewName);

        }

        [Test]
        public void AddVerification_should_return_correct_view()
        {
            // given + when
            var viewResult = _testController.AddAssertion() as PartialViewResult;

            // then
            Assert.AreEqual("EditorTemplates/AssertionViewModel", viewResult.ViewName);
            Assert.IsInstanceOf<AssertionViewModel>(viewResult.Model);
        }

        [Test]
        public void AddAssertion_should_be_decorated_with_EditableTestsRequired()
        {
            // given + when
            var addAssertionMethod = typeof(TestController).GetMethod("AddAssertion");

            // then
            Assert.IsTrue(addAssertionMethod.IsDefined(typeof(EditableTestsRequiredAttribute), false));
        }

        [Test]
        public void CapturedVariableItem_should_return_correct_view()
        {
            // given + when
            var viewResult = _testController.AddCapturedVariableItem() as PartialViewResult;

            // then
            Assert.AreEqual("EditorTemplates/CapturedVariableItem", viewResult.ViewName);
            Assert.IsInstanceOf<Syringe.Web.Models.CapturedVariableItem>(viewResult.Model);
        }

        [Test]
        public void AddCapturedVariableItem_should_be_decorated_with_EditableTestsRequired()
        {
            // given + when
            var capturedVariableItemMethod = typeof(TestController).GetMethod("AddCapturedVariableItem");

            // then
            Assert.IsTrue(capturedVariableItemMethod.IsDefined(typeof(EditableTestsRequiredAttribute), false));
        }

        [Test]
        public void AddHeaderItem_should_return_correct_view()
        {
            // given + when
            var viewResult = _testController.AddHeaderItem() as PartialViewResult;

            // then
            Assert.AreEqual("EditorTemplates/HeaderItem", viewResult.ViewName);
            Assert.IsInstanceOf<Syringe.Web.Models.HeaderItem>(viewResult.Model);
        }

        [Test]
        public void AddHeaderItem_should_be_decorated_with_EditableTestsRequired()
        {
            // given + when
            var addHeaderMethod = typeof(TestController).GetMethod("AddHeaderItem");

            // then
            Assert.IsTrue(addHeaderMethod.IsDefined(typeof(EditableTestsRequiredAttribute), false));
        }

        [Test]
        public void TestController_should_be_decorated_with_AuthorizeWhenOAuth()
        {
            // given + when + then
            Assert.IsTrue(typeof(TestController).IsDefined(typeof(AuthorizeWhenOAuthAttribute), false));
        }

        [Test]
        public void TestController_should_populate_paging_data_when_executing_action()
        {
            // given
            var request = new Mock<HttpRequestBase>();
            var queryString = new NameValueCollection
            {
                {"pageNumber", "42" },
                {"noOfResults", "5342" }
            };
            request.Setup(x => x.QueryString).Returns(queryString);

            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Request).Returns(request.Object);
            _testController.ControllerContext = new ControllerContext(context.Object, new RouteData(), _testController);

            var controller = new TestControllerWrapper(_testController);

            // when
            controller.ExecuteOnActionExecuting(null);

            // then
            Assert.That(_testController.ViewData["PageNumber"], Is.EqualTo("42"));
            Assert.That(_testController.ViewData["NoOfResults"], Is.EqualTo("5342"));
        }
    }
}
