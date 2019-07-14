# Power Planner apps
Source code of the mobile apps for Power Planner... they're open source!

![](http://powerplanner.net/Images/PowerPlannerSuite.png)


## Overview

Power Planner is a cross-platform academic app for students, complete with online sync, homework, schedules, grade calculation, and more.

The apps in this repro include the following platforms...

* UWP (C#)
* Android (C# Xamarin native)
* iOS (C# Xamarin native)

The apps all share a common C# data library, which does all of the syncing, storage, and other model/view model logic.

Each platform-specific app simply needs to build views on top of the shared view model.

## Prerequisites

* Visual Studio 2017 with Xamarin and UWP SDKs
* Mac needed to build/run the iOS version

## Getting started

1. For the first time, after cloning, generate the secrets
    1. In the top-level directory, open PowerShell and run `.\ApplySecrets.ps1`
        1. This will generate a blank `secrets.json` file (ignored from git), and generates the corresponding secret files needed to compile the app
        1. If you have actual secrets to use, update the `secrets.json` file with the secrets and re-run `.\ApplySecrets.ps1`
        1. Note that the app still should compile and run without actual secrets, but things like accessing the server won't work (offline accounts should work though).
1. Open the `PowerPlannerApps.sln` solution and you should be able to build the projects!


## Architecture
