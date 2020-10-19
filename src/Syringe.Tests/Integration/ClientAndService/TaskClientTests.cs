﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Syringe.Core.Tasks;
using Syringe.Core.Tests.Results.Repositories;
using Syringe.Service;
using Syringe.Service.Parallel;

namespace Syringe.Tests.Integration.ClientAndService
{
    [TestFixture]
    public class TaskClientTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ServiceStarter.StartSelfHostedOwin();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            ServiceStarter.StopSelfHostedOwin();
        }

        [SetUp]
        public void Setup()
        {
            Console.WriteLine("Wiping db results database");
            ServiceStarter.Container.GetInstance<ITestFileResultRepository>().Wipe().Wait();

            ServiceStarter.RecreateTestFileDirectory();
        }

        [Test]
        public void should_return_running_tasks()
        {
            // given
            var client = Helpers.CreateTasksClient();

            // when
            var runningTasks = client.GetTasks();

            // then
            Assert.That(runningTasks, Is.Not.Null);
        }

        [Test]
        public void should_return_running_task()
        {
            // given
            var taskRequest = new TaskRequest { Environment = "anything", Filename = "test-test.json" };
            ServiceStarter.Container.GetInstance<ITestFileQueue>().Add(taskRequest);
            var client = Helpers.CreateTasksClient();

            // when
            var task = client.GetTask(1);

            // then
            Assert.That(task, Is.Not.Null);
        }

        [Test]
        public void should_start_task()
        {
            // given
            var taskRequest = new TaskRequest { Environment = "anything", Filename = "test-test.json" };
            var client = Helpers.CreateTasksClient();

            // when
            int task = client.Start(taskRequest);

            // then
            Assert.That(task, Is.Not.Null);
            Assert.That(task, Is.GreaterThan(0));
        }
    }
}