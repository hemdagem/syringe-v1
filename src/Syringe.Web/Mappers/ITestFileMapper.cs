﻿using System.Collections.Generic;
using System.Web.Mvc;
using Syringe.Core.Tests;
using Syringe.Web.Models;

namespace Syringe.Web.Mappers
{
    public interface ITestFileMapper
    {
        TestViewModel BuildTestViewModel(TestFile testFile, int position, int pageNo = 1);
        IEnumerable<TestViewModel> BuildTests(IEnumerable<Test> tests, int pageNumber, int noOfResults);
        Test BuildTestObject(TestViewModel testModel);
        List<VariableViewModel> BuildVariableViewModel(TestFile test);
        void PopulateScriptSnippets(TestViewModel model);
    }
}