﻿using System;
using System.IO;

namespace Syringe.Core.Configuration
{
    public class ConfigLocator : IConfigLocator
    {
        private readonly string[] _configurationDirectories;

        public ConfigLocator()
        {
            _configurationDirectories = new[]
            {
                AppDomain.CurrentDomain.BaseDirectory,
                GetAppDataFolder()
            };
        }

        internal ConfigLocator(params string[] configPathDirecties)
        {
            _configurationDirectories = configPathDirecties;
        }

        /// <summary>
        /// This will look in defined places on the computer for config files.
        /// These might live in the current directory or /AppData/Syringe/  etc
        /// </summary>
        public string ResolveConfigFile(string fileName)
        {
            foreach (string directory in _configurationDirectories)
            {
                string configToTest = Path.Combine(directory, fileName);
                if (File.Exists(configToTest))
                {
                    return configToTest;
                }
            }

            string errorMessage = $"Unable to find config file '{fileName}' in: {string.Join(" | or | ", _configurationDirectories)}";
            throw new FileNotFoundException(errorMessage, fileName);
        }

        private static string GetAppDataFolder() => Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "Syringe");
    }
}