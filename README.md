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

* Visual Studio 2019 with Xamarin and UWP SDKs
* If building iOS version, Mac needed to build/run the iOS version

## Getting started

**Detailed step-by-step instructions are [available here](https://github.com/powerplanner/powerplannerapps/wiki/Getting-started-(new-developer))**. The instructions below are cliff notes meant for devs experienced with Windows and Xamarin development.

1. Be sure to **clone submodules** too. If you didn't, `git submodule update --init --recursive`
1. For the first time after cloning, generate the secrets
    1. In the top-level directory, open PowerShell and run `.\ApplySecrets.ps1`
        1. This will generate a blank `secrets.json` file (ignored from git), and generates the corresponding secret files needed to compile the app
        1. If you have actual secrets to use, update the `secrets.json` file with the secrets and re-run `.\ApplySecrets.ps1`
        1. Note that the app still should compile and run without actual secrets, but things like accessing the server won't work (offline accounts should work though).
1. Open the `PowerPlannerApps.sln` solution and you should be able to build the projects! See below for how to build each project.

### Building UWP

1. Set the start up project to `PowerPlannerUWP (Universal Windows)`, and ensure the build config is set to `Debug` and architecture is one of `x86`, `x64`, or `ARM` (Windows Phone only)
1. Click **Local Machine** to deploy!

![](assets\readme\BuildingUwpConfig.jpg)


### Building Android

1. Set the start up project to `PowerPlannerDroid`, and ensure the build config is set to `Debug (Droid)` (important that it must be the `(Droid)` config) and architecture is `Any CPU`
1. Click **Local Machine** to deploy!


### Building iOS

1. Set the start up project to `PowerPlanneriOS`, and ensure the build config is set to `Debug` and architecture is `Any CPU`
1. Click **Local Machine** to deploy!


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


#### Helpful binding logic

Views drastically depend on being able to **bind** to view model properties, so that the view magically updates when a property changes.

Anything that needs bindable properties should extend from `BindableBase`. Each property should then have a private and public property, as seen below, and when setting the private field, be sure to use the `SetProperty` method (provided by `BindableBase`), and reference the name of the property that changed (by using `nameof` to ensure that if you rename the property, refactoring will update everywhere).

```csharp
public class Grade : BindableBase
{
    private double _gradeReceived;
    public double GradeReceived
    {
        get => _gradeReceived;
        set => SetProperty(ref _gradeReceived, value, nameof(GradeReceived));
    }
}
```

Sometimes there are **computed properties** that are dependent on other properties. There's another helper method in `BindableBase` for that... `CachedComputation`. Use it as follows...

```csharp
public class Grade : BindableBase
{
    private double _gradeReceived;
    public double GradeReceived
    {
        get => _gradeReceived;
        set => SetProperty(ref _gradeReceived, value, nameof(GradeReceived));
    }
    
    // ...
    
    public double Percentage => CachedComputation(delegate
    {
        return GradeReceived / GradeTotal;
    }, new string[] { nameof(GradeReceived), nameof(GradeTotal) });
}
```

Notice that you have to explicitly reference (via `nameof`) the dependent properties you want to listen to. When one of those properties changes, the computation will run again, and if the result changes, it will trigger a property change event for that property.


## Localization

The Android and UWP apps are currently fully localized. Localized strings are found in `PowerPlannerAppDataLibrary/Strings`. iOS has not been updated to take advantage of the localized strings (text is hardcoded right now).

The multilingual app toolkit by Microsoft is used to help auto-generate translations. The process for adding a new string is as follows...

1. Add the new English string in `PowerPlannerAppDataLibrary/Strings/Resources.resx`
1. Build the `PowerPlannerAppDataLibrary` project
1. Notice that the `PowerPlannerAppDataLibrary/MultilingualResources` files have been updated... but they don't have translations yet
1. If you're using the multilingual app toolkit, right click on one of those multilingual `.xlf` files and select Multilingual App Toolkit -> Generate machine translations. This will only generate translations for new strings.
1. Now, open the `.xlf` file (the diff view in VS works well), find the newly added strings, review them, and set their `<target state="final">`.
1. Build `PowerPlannerAppDataLibrary` once again, and notice that the `PowerPlannerAppDataLibrary/Strings/Resources.*.resx` files have been updated

To access a localized string programmatically...

```csharp
Title = PowerPlannerResources.GetString("ViewGradePage.Title");
```

### UWP-specific localization considerations

UWP supports localization within the XAML markup, using `x:Uid`. For example, the `Label` property of the following control is localized...

```xaml
<AppBarButton
    x:Uid="AppBarButtonSave"
    x:Name="ButtonSave"
    Icon="Save"
    Label="Save"
    Click="ButtonSave_Click"/>
```

![image](https://user-images.githubusercontent.com/13246069/61190767-5251c680-a656-11e9-8bc2-d5d868648011.png)

The resources can use `.` to set properties, like the `.Label` causes the label property to be localized with the value in the resources.


### Android-specific localization considerations

In Android, you can also localize directly in the XML layout views. But this uses custom syntax part of a custom Android layout binding language.

```xml
<TextView
  android:layout_width="wrap_content"
  android:layout_height="wrap_content"
  android:text="{Settings_GradeOptions_GpaType_StandardExplanation.Text}"
  android:textSize="12sp"
  android:textColor="#000000"
  android:layout_marginTop="4dp"/>
```

Simply place the resource string's id within `{}`. I can't remember whether localization is supported on any text property, or only specific ones like TextView.text... it might be supported on any.


## Helpful code snippets

### Showing a dialog cross-platform

```csharp
new PortableMessageDialog("my content", "my title").Show(); // or ShowAsync() if you want to wait for it to be closed
```
