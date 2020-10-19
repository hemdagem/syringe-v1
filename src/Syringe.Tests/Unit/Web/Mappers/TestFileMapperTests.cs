﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Moq;
using NUnit.Framework;
using Syringe.Core.Services;
using Syringe.Core.Tests;
using Syringe.Core.Tests.Scripting;
using Syringe.Core.Tests.Variables;
using Syringe.Web.Mappers;
using Syringe.Web.Models;
using HeaderItem = Syringe.Web.Models.HeaderItem;
using Environment = Syringe.Core.Environment.Environment;

namespace Syringe.Tests.Unit.Web.Mappers
{
    [TestFixture]
    public class TestFileMapperTests
    {
        private Mock<IConfigurationService> _configurationServiceMock;
        private Mock<IEnvironmentsService> _environmentServiceMock;
        private TestFileMapper _mapper;

        [SetUp]
        public void Setup()
        {
            _configurationServiceMock = new Mock<IConfigurationService>();
            _environmentServiceMock = new Mock<IEnvironmentsService>();
            _mapper = new TestFileMapper(_configurationServiceMock.Object, _environmentServiceMock.Object);

            _configurationServiceMock
                .Setup(x => x.GetSystemVariables())
                .Returns(new List<Variable>());
        }

        private TestViewModel _testViewModel
        {
            get
            {
                return new TestViewModel
                {
                    Headers = new List<HeaderItem> { new HeaderItem { Key = "Key", Value = "Value" } },
                    Position = 1,
                    Filename = "Test.xml",
                    CapturedVariables = new List<CapturedVariableItem>() { new CapturedVariableItem { Name = "Description", Regex = "Regex" } },
                    PostBody = "Post Body",
                    Assertions = new List<AssertionViewModel>()
                    {
                        new AssertionViewModel { Description = "Description1", Value = "Value1", AssertionType = AssertionType.Negative, AssertionMethod = AssertionMethod.Regex },
                        new AssertionViewModel { Description = "Description2", Value = "Value2", AssertionType = AssertionType.Positive, AssertionMethod = AssertionMethod.CssSelector }
                    },
                    Description = "short d3escription",
                    Url = "url",
                    Method = MethodType.POST,
                    ExpectedHttpStatusCode = HttpStatusCode.Accepted,
                    BeforeExecuteScriptFilename = "ISomething something = new Something();",
                    RequiredEnvironments = new List<string> { "test-env-1", "another-test-env" }
                };
            }
        }

        [Test]
        public void Build_should_set_correct_properties_when_model_is_populated()
        {
            // given

            // when
            Test test = _mapper.BuildTestObject(_testViewModel);

            // then
            Assert.AreEqual(_testViewModel.Headers.Count, test.Headers.Count);
            Assert.AreEqual(_testViewModel.CapturedVariables.Count, test.CapturedVariables.Count);
            Assert.AreEqual(_testViewModel.PostBody, test.PostBody);
            Assert.AreEqual(2, test.Assertions.Count);
            Assert.AreEqual(_testViewModel.Description, test.Description);
            Assert.AreEqual(_testViewModel.Url, test.Url);
            Assert.AreEqual(_testViewModel.Method.ToString(), test.Method);
            Assert.AreEqual(_testViewModel.ExpectedHttpStatusCode, test.ExpectedHttpStatusCode);
            Assert.AreEqual(_testViewModel.BeforeExecuteScriptFilename, test.ScriptSnippets.BeforeExecuteFilename);
            Assert.AreEqual(_testViewModel.RequiredEnvironments, test.TestConditions.RequiredEnvironments);
        }

        [Test]
        public void Build_should_set_assertion_properties()
        {
            // given

            // when
            Test test = _mapper.BuildTestObject(_testViewModel);

            // then
            Assertion firstAssertion = test.Assertions.First();
            Assert.That(firstAssertion.Description, Is.EqualTo("Description1"));
            Assert.That(firstAssertion.Value, Is.EqualTo("Value1"));
            Assert.That(firstAssertion.AssertionType, Is.EqualTo(AssertionType.Negative));
            Assert.That(firstAssertion.AssertionMethod, Is.EqualTo(AssertionMethod.Regex));

            Assertion lastAssertion = test.Assertions.Last();
            Assert.That(lastAssertion.Description, Is.EqualTo("Description2"));
            Assert.That(lastAssertion.Value, Is.EqualTo("Value2"));
            Assert.That(lastAssertion.AssertionType, Is.EqualTo(AssertionType.Positive));
            Assert.That(lastAssertion.AssertionMethod, Is.EqualTo(AssertionMethod.CssSelector));
        }

        [Test]
        public void BuildTestObject_should_throw_argumentnullexception_when_test_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => _mapper.BuildTestObject(null));
        }

        [Test]
        public void BuildViewModel_should_throw_argumentnullexception_when_test_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => _mapper.BuildTestViewModel(null, 0));
        }

        [Test]
        public void BuildTests_should_throw_argumentnullexception_when_test_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => _mapper.BuildTests(null, 0, 0));
        }

        [Test]
        public void BuildTests_should_return_correct_model_values_from_testfile()
        {
            // given
            const int pageNumber = 2;
            const int numberOfResults = 10;
            var testFile = new TestFile
            {
                Tests = new List<Test>
                {
                    new Test
                    {
                        Description = "Description 1",
                        Url = "http://www.google.com",
                        Assertions = new List<Assertion>() { new Assertion(), new Assertion()},
                        CapturedVariables = new List<CapturedVariable>() { new CapturedVariable(), new CapturedVariable() },
                        TestConditions = new TestConditions {RequiredEnvironments = new List<string> { "my-env" }}
                    },
                    new Test
                    {
                        Description = "Description 2",
                        Url = "http://www.arsenal.com",
                        Assertions = new List<Assertion>() { new Assertion(), new Assertion(), new Assertion()},
                        CapturedVariables = new List<CapturedVariable>() { new CapturedVariable(), new CapturedVariable(), new CapturedVariable() },
                        TestConditions = new TestConditions { RequiredEnvironments = new List<string> { "different-env" } }
                    },
                }
            };

            // when
            IEnumerable<TestViewModel> viewModels = _mapper.BuildTests(testFile.Tests, pageNumber, numberOfResults);

            // then
            Assert.NotNull(viewModels);
            Assert.AreEqual(2, viewModels.Count());

            var firstCase = viewModels.First();
            Assert.AreEqual(10, firstCase.Position);
            Assert.AreEqual("Description 1", firstCase.Description);
            Assert.AreEqual("http://www.google.com", firstCase.Url);
            Assert.AreEqual("http://www.google.com", firstCase.Url);
            Assert.That(firstCase.Assertions.Count, Is.EqualTo(2));
            Assert.That(firstCase.CapturedVariables.Count, Is.EqualTo(2));
            Assert.That(firstCase.RequiredEnvironments, Is.EqualTo(testFile.Tests.First().TestConditions.RequiredEnvironments));

            var lastCase = viewModels.Last();
            Assert.AreEqual(11, lastCase.Position);
            Assert.AreEqual("Description 2", lastCase.Description);
            Assert.That(lastCase.Assertions.Count, Is.EqualTo(3));
            Assert.That(lastCase.CapturedVariables.Count, Is.EqualTo(3));
            Assert.That(lastCase.RequiredEnvironments, Is.EqualTo(testFile.Tests.Skip(1).First().TestConditions.RequiredEnvironments));
        }

        [Test]
        public void BuildTestViewModel_should_return_correct_model_values_from_test()
        {
            // given
            const int testPosition = 1;
            _configurationServiceMock
                .Setup(x => x.GetScriptSnippetFilenames(ScriptSnippetType.BeforeExecute))
                .Returns(new[] { "snippet1.snippet", "snippet2.snippet" });

            List<Environment> environments = new List<Environment>
            {
                new Environment { Name = "Last", Order = 2},
                new Environment { Name = "First", Order = 0}
            };

            _environmentServiceMock
                .Setup(x => x.Get())
                .Returns(environments);

            var expectedTest = new Test
            {
                Description = "Short Description",
                Url = "http://www.google.com",
                Method = MethodType.GET.ToString(),
                PostBody = "PostBody",
                ExpectedHttpStatusCode = HttpStatusCode.Accepted,
                Headers = new List<Syringe.Core.Tests.HeaderItem> { new Syringe.Core.Tests.HeaderItem() },
                CapturedVariables = new List<CapturedVariable> { new CapturedVariable { Name = "CV-2" } },
                Assertions = new List<Assertion> { new Assertion("Desc", "Val", AssertionType.Negative, AssertionMethod.CssSelector) },
                ScriptSnippets = new ScriptSnippets()
                {
                    BeforeExecuteFilename = "// this is some script"
                },
                TestConditions = new TestConditions { RequiredEnvironments = new List<string> { "expected-env", "h3mang-and-d1cks" } }
            };

            var testFile = new TestFile
            {
                Filename = "some file name...YURP",
                Tests = new List<Test>
                {
                    new Test
                    {
                        CapturedVariables = new List<CapturedVariable>
                        {
                            new CapturedVariable("CV-1", "CV-1-Value")
                        }
                    },
                    expectedTest,
                    new Test()
                },
                Variables = new List<Variable>
                {
                    new Variable("V-1", "V-1-Value", null)
                }
            };

            // when
            TestViewModel actualModel = _mapper.BuildTestViewModel(testFile, testPosition);

            // then
            Assert.NotNull(actualModel);
            Assert.That(actualModel.Position, Is.EqualTo(testPosition));
            Assert.That(actualModel.Description, Is.EqualTo(expectedTest.Description));
            Assert.That(actualModel.Url, Is.EqualTo(expectedTest.Url));
            Assert.That(actualModel.PostBody, Is.EqualTo(expectedTest.PostBody));
            Assert.That(actualModel.Method, Is.EqualTo(MethodType.GET));
            Assert.That(actualModel.ExpectedHttpStatusCode, Is.EqualTo(expectedTest.ExpectedHttpStatusCode));
            Assert.That(actualModel.Filename, Is.EqualTo(testFile.Filename));

            Assert.That(actualModel.CapturedVariables.Count, Is.EqualTo(1));
            Assert.That(actualModel.Assertions.Count, Is.EqualTo(1));
            Assert.That(actualModel.Headers.Count, Is.EqualTo(1));

            AssertionViewModel assertionViewModel = actualModel.Assertions.FirstOrDefault();
            Assert.That(assertionViewModel.Description, Is.EqualTo("Desc"));
            Assert.That(assertionViewModel.Value, Is.EqualTo("Val"));
            Assert.That(assertionViewModel.AssertionType, Is.EqualTo(AssertionType.Negative));
            Assert.That(assertionViewModel.AssertionMethod, Is.EqualTo(AssertionMethod.CssSelector));

            Assert.That(actualModel.AvailableVariables.Count, Is.EqualTo(3));

            VariableViewModel capturedVar = actualModel.AvailableVariables.Find(x => x.Name == "CV-1");
            Assert.That(capturedVar, Is.Not.Null);
            Assert.That(capturedVar.Value, Is.EqualTo("CV-1-Value"));

            capturedVar = actualModel.AvailableVariables.Find(x => x.Name == "CV-2");
            Assert.That(capturedVar, Is.Not.Null);

            VariableViewModel testVariable = actualModel.AvailableVariables.Find(x => x.Name == "V-1");
            Assert.That(testVariable, Is.Not.Null);
            Assert.That(testVariable.Value, Is.EqualTo("V-1-Value"));

            Assert.That(actualModel.BeforeExecuteScriptFilename, Is.EqualTo(expectedTest.ScriptSnippets.BeforeExecuteFilename));
            Assert.That(actualModel.BeforeExecuteScriptSnippets.Count(), Is.EqualTo(2));

            Assert.That(actualModel.RequiredEnvironments, Is.EqualTo(expectedTest.TestConditions.RequiredEnvironments));
            Assert.That(actualModel.Environments, Is.EqualTo(new List<string> { "First", "Last" }));
        }

        [Test]
        public void should_populate_snippets_from_snippetreader()
        {
            // given
            var testFile = new TestFile
            {
                Tests = new[]
                {
                    new Test()
                }
            };

            _configurationServiceMock
                .Setup(x => x.GetScriptSnippetFilenames(It.IsAny<ScriptSnippetType>()))
                .Returns(new string[] { "snippet1", "snippet2" });

            // when
            TestViewModel result = _mapper.BuildTestViewModel(testFile, 0);

            // then
            Assert.That(result.BeforeExecuteScriptSnippets.Count(), Is.EqualTo(2));
            Assert.That(result.BeforeExecuteScriptSnippets, Contains.Item("snippet1"));
            Assert.That(result.BeforeExecuteScriptSnippets, Contains.Item("snippet2"));
        }

        [Test]
        public void should_include_reserved_variables_in_available_variable_list()
        {
            // given
            var testFile = new TestFile
            {
                Tests = new[]
                {
                    new Test()
                }
            };

            var reservedVariables = new List<Variable>
            {
                new Variable("some name that should exist", "super awesome description", "evil environment")
            };

            _configurationServiceMock
                .Setup(x => x.GetSystemVariables())
                .Returns(reservedVariables);

            // when
            TestViewModel result = _mapper.BuildTestViewModel(testFile, 0);

            // then
            VariableViewModel variable = result.AvailableVariables.FirstOrDefault(x => x.Name == reservedVariables[0].Name);
            Assert.That(variable, Is.Not.Null);
            Assert.That(variable.Value, Is.EqualTo(reservedVariables[0].Value));
            Assert.That(variable.Environment, Is.EqualTo(reservedVariables[0].Environment.Name));
        }

        [Test]
        public void BuildVariableViewModel_should_throw_exception_when_test_is_null()
        {
            // given

            // when + then
            Assert.Throws<ArgumentNullException>(() => _mapper.BuildVariableViewModel(null));
        }

        [Test]
        public void BuildVariableViewModel_should_return_base_variables_if_they_exist()
        {
            // given
            var testFile = new TestFile { Variables = new List<Variable> { new Variable { Name = "test", Value = "value" } } };

            // when + then
            var buildVariableViewModel = _mapper.BuildVariableViewModel(testFile);
            Assert.AreEqual(1, buildVariableViewModel.Count);
            Assert.AreEqual("test", buildVariableViewModel[0].Name);
            Assert.AreEqual("value", buildVariableViewModel[0].Value);
        }

        [Test]
        public void BuildVariableViewModel_should_return_captured_variables_if_they_exist()
        {
            // given
            var testFile = new TestFile
            {
                Tests = new List<Test>
                {
                    new Test
                    {
                        CapturedVariables = new List<CapturedVariable>
                        {
                            new CapturedVariable("name", "regex")
                        }
                    },
                    new Test()
                }
            };

            // when + then
            List<VariableViewModel> buildVariableViewModel = _mapper.BuildVariableViewModel(testFile);
            Assert.That(buildVariableViewModel.Count, Is.EqualTo(1));
            Assert.That(buildVariableViewModel[0].Name, Is.EqualTo("name"));
            Assert.That(buildVariableViewModel[0].Value, Is.EqualTo("regex"));
        }

        [Test]
        public void PopulateScriptSnippets_should_populate_items_from_config_service()
        {
            // given
            _configurationServiceMock
                .Setup(x => x.GetScriptSnippetFilenames(ScriptSnippetType.BeforeExecute))
                .Returns(new string[] { "snippet1.snippet", "snippet2.snippet" });

            var testViewModel = new TestViewModel();

            // when
            _mapper.PopulateScriptSnippets(testViewModel);

            // then
            Assert.That(testViewModel.BeforeExecuteScriptSnippets.Count(), Is.EqualTo(2));
            Assert.That(testViewModel.BeforeExecuteScriptSnippets, Contains.Item("snippet1.snippet"));
            Assert.That(testViewModel.BeforeExecuteScriptSnippets, Contains.Item("snippet2.snippet"));
        }
    }
}