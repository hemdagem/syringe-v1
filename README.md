[![Build status](https://ci.appveyor.com/api/projects/status/l8lcjqu5q0ld1je9?svg=true)](https://ci.appveyor.com/project/yetanotherchris/syringe-4kmo4)
[![Documentation Status](https://readthedocs.org/projects/syringe/badge/?version=latest)](http://syringe.readthedocs.io/en/latest/?badge=latest)
[![Coverage Status](https://coveralls.io/repos/github/TotalJobsGroup/syringe/badge.svg?branch=master)](https://coveralls.io/github/TotalJobsGroup/syringe?branch=master)
[![Average time to resolve an issue](http://isitmaintained.com/badge/resolution/TotalJobsGroup/Syringe.svg)](http://isitmaintained.com/project/TotalJobsGroup/Syringe "Average time to resolve an issue") 
[![Percentage of issues still open](http://isitmaintained.com/badge/open/TotalJobsGroup/Syringe.svg)](http://isitmaintained.com/project/TotalJobsGroup/Syringe "Percentage of issues still open")
[![release](https://img.shields.io/badge/release-1.0.599-brightgreen.svg?style=flat)](https://github.com/totaljobsgroup/syringe/releases/latest)

# Syringe
Syringe is a .NET automated HTTP testing tool for headless, Javascript-ignorant tests.

![Syringe](https://raw.githubusercontent.com/TotalJobsGroup/Syringe/master/logo.png)


## Syringe's purpose in the universe

Syringe is a HTTP runner - if you can reach a server endpoint via HTTP, Syringe will be able to test it. It's purpose is:

1. To check canary pages (HTML, XML, JSON, TXT etc.)
2. To perform advanced smoke testing - get a HTML page (or other text-based resource) and assert that it contains some text.
3. Basic end-to-end HTTP tests. For example logging into a website and checking a page.

## Help
Our help files are hosted at [ReadTheDocs](http://syringe.readthedocs.io/en/latest/?badge=latest).

## Installation

### Pre-requisites

Make sure you have IIS enabled. 

##### Chocolatey

* Install chocolatey
* Install nuget command line : `choco install nuget.commandline`
* Powershell 4+: `choco install powershell4`

##### Mongodb: 
```
    # Work around for bug in the mongodb Chocolately package
    $env:systemdrive = "C:\ProgramData\chocolatey\lib\mongodata"
    choco install mongodb
```

##### Configure an OAuth2 provider

Syringe uses OAuth2 for its security. It currently supports Github, Google and Microsoft OAuth2 providers.

* [Register an Syringe OAuth2 app in Github](https://github.com/settings/developers). The callback url should be `http://localhost:1980`
* Edit the configuration.json file in the service directory to use the OAuth2 client id/secret.


## Downloading & Installing from Binaries

You can get the latest release of Syringe **[HERE](https://github.com/TotalJobsGroup/Syringe/releases)**

### Installing Website

Once you have extracted the release, in Powershell run `.\iis.ps1`

### Start the service

The Syringe REST API runs as Windows service, which can also be run as a command line app. This API is used to run all tests and is the data repository, it runs its own embedded HTTP server.

* Run `.\Syringe.Service.exe` 
* Browse to http://localhost:1980 and login.

## Building from source

Once you've cloned the repository, run `setup.ps`, this will:

* Build the solution, restoring the nuget packages  
* Create an IIS site
* Create C:\syringe folder with an example file.

Follow the "Configure OAuth" and "Start the service" steps above
