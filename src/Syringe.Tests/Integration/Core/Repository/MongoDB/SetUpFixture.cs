﻿using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace Syringe.Tests.Integration.Core.Repository.MongoDB
{
    [SetUpFixture]
    public class SetUpFixture
    {
        private Process _mongoDbProcess;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Check if MongoDb is running
            if (!Process.GetProcessesByName("mongod").Any() && !Process.GetProcessesByName("com.docker.service").Any())
            {
                _mongoDbProcess = Process.Start(@"C:\Program Files\MongoDB\Server\3.0\bin\mongod.exe", @"--dbpath c:\mongodb\data\");
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // Only kill mongod if the tests started it
            _mongoDbProcess?.Kill();
        }
    }
}