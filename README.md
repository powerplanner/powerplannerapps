# Power Planner apps
Source code of the mobile apps for Power Planner... they're open source!

![](http://powerplanner.net/Images/PowerPlannerSuite.png)

| App store links |
|--|
| [Windows store](https://www.microsoft.com/store/apps/9wzdncrfj25v) |
| [Google Play store](https://play.google.com/store/apps/details?id=com.barebonesdev.powerplanner) |
| [Apple App store](https://itunes.apple.com/us/app/power-planner-homework-grades/id1278178608)


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

1. Be sure to **clone submodules** too. If you didn't, `git submodule update --init --recursive`
1. For the first time after cloning, generate the secrets
    1. In the top-level directory, open PowerShell and run `.\ApplySecrets.ps1`
        1. This will generate a blank `secrets.json` file (ignored from git), and generates the corresponding secret files needed to compile the app
        1. If you have actual secrets to use, update the `secrets.json` file with the secrets and re-run `.\ApplySecrets.ps1`
        1. Note that the app still should compile and run without actual secrets, but things like accessing the server won't work (offline accounts should work though).
1. Open the `PowerPlannerApps.sln` solution and you should be able to build the projects!


## Architecture

### Data layer

All three apps use a common shared data layer - `PowerPlannerAppDataLibrary`. The data layer handles...

* Connecting to the server
* Syncing between local client and server
* Storing all local content and accounts

### View model layer

All three apps also use a common view model layer, contained in `PowerPlannerAppDataLibrary`. The view model is a virtual representation of the pages that should be shown to the user. It has concepts like popups, navigation, etc.

The view model layer is written using the custom `BareMvvm.Core` project.

```csharp
public class WelcomeViewModel : BaseViewModel
{
    public WelcomeViewModel(BaseViewModel parent) : base(parent) { }

    public void Login()
    {
        ShowPopup(new LoginViewModel(this));
    }

    public void CreateAccount()
    {
        ShowPopup(new CreateAccountViewModel(this));
    }
```

Then there are platform-specific **presenter** libraries, contained in `InterfacesDroid`, `InterfacesiOS`, and `InterfacesUWP`. The presenter library binds to the view model and accordingly shows views as they're created, hides views as they're removed, etc.

```csharp
        private void UpdateContent()
        {
            KeyboardHelper.HideKeyboard(this);

            // Remove previous content
            base.RemoveAllViews();

            if (ViewModel?.Content != null)
            {
                // Create and set new content
                var view = ViewModelToViewConverter.Convert(this, ViewModel.Content);
                base.AddView(view);
            }
```

Views are **registered** to ViewModels so that the presenter knows which view to create corresponding to the current view model. They are registered in...

* `PowerPlannerUWP/App.xaml.cs`
* `PowerPlannerAndroid/App/NativeApplication.cs`
* `PowerPlanneriOS/AppDelegate.cs`

```csharp
return new Dictionary<Type, Type>()
{
    { typeof(WelcomeViewModel), typeof(WelcomeView) },
    { typeof(LoginViewModel), typeof(LoginView) },
    { typeof(MainScreenViewModel), typeof(MainScreenView) },
```
