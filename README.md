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

1. Set the start up project to `PowerPlannerDroid`, and ensure the build config is set to `Debug` and architecture is `Any CPU`
1. Click the deploy button to deploy! It'll typically say the name of the simulator, like "Pixel 5", on the button.


### Building iOS

Prereqs...
* Be on Mac
* Have latest XCode installed, with simulators installed and iOS SDK installed
* Install .NET 8 SDK
* Install .NET `maui` workload

Instructions...
1. In Terminal, `cd` to the `PowerPlanneriOS` directory.
1. Run `dotnet run --project PowerPlanneriOS.csproj`
1. The simulator should launch!


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


### Android-specific binding examples

```xml
    <androidx.appcompat.widget.SwitchCompat
      android:id="@+id/SwitchRepeats"
      android:text="{RepeatingEntry_CheckBoxRepeats.Content}"
      android:textSize="16sp"
      android:layout_width="match_parent"
      android:layout_height="wrap_content"
      android:padding="16dp"
      local:Binding="{Source=Repeats, Target=Checked, Mode=TwoWay}; {Source=IsRepeatsVisible, Target=Visibility, Converter=BoolToVisibilityConverter}"/>
```

Converters are auto-discovered using reflection and their class name.

There are some **implicit converters** which live in **BindingApplicator.SetTargetProperty**.


### iOS-specific binding examples

Within a ViewController, to add a generic binding that can perform any action, do the following... But note there's specific bindings for common tasks like binding text that you should use instead.

```csharp
BindingHost.SetBinding(nameof(ViewModel.IsSyncing), delegate
{
    if (ViewModel.IsSyncing)
    {
        // Do whatever you want here
    }
    else
    {
        // Do whatever you want here
    }
});
```

To bind **text**...

```csharp
BindingHost.SetLabelTextBinding(labelErrorDescription, nameof(ViewModel.Error));
```

To bind **text boxes**... (It's two-way binding by default)

```csharp
BindingHost.SetTextFieldTextBinding(myTextField, nameof(ViewModel.Name));
```

You'll notice there's also lots of other binding options for binding visibility, color, etc.

To bind **visibility** where you need the item to collapse, you can either put the item in a `BareUIVisibilityContainer` and set the `Child` to your content and then set the visibilty binding on the visibility container (iOS doesn't have the concept of visibility on elements themselves, that's why we have to add it in a container)...

```csharp
var pickerCustomTimeContainer = new BareUIVisibilityContainer()
{
    Child = stackViewPickerCustomTime // Your content that you want visible/collapsed
};
BindingHost.SetVisibilityBinding(pickerCustomTimeContainer, nameof(ViewModel.IsStartTimePickerVisible));
```

Alternatively, if you're adding content into a StackView, you can use a simpler method for toggling **visibility**... Use the `AddUnderVisibility` extension on the StackView to add the item instead of using `AddArrangedSubview`. This will automatically use the BareUIVisibilityContainer under the scenes.

```csharp
var progressBar = new UIProgressView(UIProgressViewStyle.Default)
{
    TranslatesAutoresizingMaskIntoConstraints = false
};
viewCenterContainer.AddUnderVisiblity(progressBar, BindingHost, nameof(ViewModel.IsSyncing));
```

If you just need the item to be **hidden** but not actually collapsed, you can use `SetVisibilityBinding` directly on the view rather than wrapping it in a `BareUiVisibilityContainer`.

```csharp
BindingHost.SetVisibilityBinding(buttonSettings, nameof(ViewModel.IsSyncing));
```

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


## A/B Testing

There's already code to help with performing A/B tests.

To add a new test, in `PowerPlannerAppDataLibrary\Helpers\AbTestHelper.cs`, add a new test to the `Tests` class. You can set the boolean to true or false, which is only used in debug mode, so that you can test both scenarios.

```csharp
public static class Tests
{
    public static TestItem NewTimePicker { get; set; } = new TestItem(nameof(NewTimePicker), true); // The boolean at the end is only used in debug mode, so that you can enable or disable the test. In release mode, it'll be randomly enabled/disabled.
}
```

To change your code programmatically based on the test value...

```csharp
if (AbTestHelper.Tests.NewTimePicker)
{
    // Perform code when enabled
}
else
{
    // Perform the old code
}
```

If you have UI you need to swap out, specify the name of the test and provide the enabled and disabled content...

```xml
<controls:AbTestControl TestName="NewTimePicker">
    <controls:AbTestControl.EnabledContent>
        <controls:TimePickerControl
            x:Uid="EditingClassScheduleItemView_TimePickerStart"
            Margin="6"
            HorizontalAlignment="Stretch"
            controls:TimePickerControl.Header="From"
            controls:TimePickerControl.IsEndTime="False"/>
    </controls:AbTestControl.EnabledContent>
    <controls:AbTestControl.DisabledContent>
        <TimePicker
            x:Uid="EditingClassScheduleItemView_TimePickerStart"
            Header="From"
            HorizontalAlignment="Stretch"
            Time="{Binding StartTime, Mode=TwoWay}"/>
    </controls:AbTestControl.DisabledContent>
</controls:AbTestControl>
```

And finally, to log metrics of the test, do something like...

```csharp
try
{
    TelemetryExtension.Current?.TrackEvent("NewTimePicker_TestResult", new Dictionary<string, string>()
    {
        { "Duration", ((int)Math.Ceiling(duration.TotalSeconds)).ToString() },
        { "IsEnabled", AbTestHelper.Tests.NewTimePicker.Value.ToString() }
    });
}
catch { }
```
