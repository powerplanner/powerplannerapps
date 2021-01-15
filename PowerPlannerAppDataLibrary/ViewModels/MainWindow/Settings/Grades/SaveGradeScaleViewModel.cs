using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerSending;
using ToolsPortable;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using BareMvvm.Core;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class SaveGradeScaleViewModel : BaseMainScreenViewModelChild
    {
        public new MainScreenViewModel MainScreenViewModel { get; private set; }

        public SaveGradeScaleViewModel(BaseViewModel parent, MainScreenViewModel mainScreenViewModel, Parameter parameter) : base(parent)
        {
            // We're using a higher parent so that this popup appears above the current popup
            MainScreenViewModel = mainScreenViewModel;
            _parameter = parameter;

            // Name must be filename safe
            Name = new TextField(initialText: parameter.Name ?? "", required: true, maxLength: 50, inputValidator: new CustomInputValidator(ValidateName), ignoreOuterSpaces: true, reportValidatorInvalidInstantly: true);
        }

        private InputValidationState ValidateName(string name)
        {
            if (!StringTools.IsStringFilenameSafe(name))
            {
                var characters = name.ToCharArray().Distinct().ToArray();
                var validSpecialChars = StringTools.VALID_SPECIAL_FILENAME_CHARS.ToArray();

                var validCharacters = characters.Where(i => Char.IsLetterOrDigit(i) || validSpecialChars.Contains(i)).ToArray();
                var invalidCharacters = characters.Except(validCharacters).ToArray();

                try
                {
                    return InputValidationState.Invalid(CustomInputValidator.GetInvalidCharactersError(invalidCharacters));
                }
                catch
                {
                    return InputValidationState.Invalid("Invalid");
                }
            }

            return InputValidationState.Valid;
        }

        private Parameter _parameter;

        public class Parameter
        {
            public string Name { get; set; }

            public GradeScale[] Scales { get; set; }

            public Action OnSaved { get; set; }
        }

        public TextField Name { get; private set; }

        public async void Save()
        {
            try
            {
                if (!ValidateAllInputs())
                {
                    return;
                }

                string name = Name.Text.Trim();

                if (name.Length == 0)
                {
                    await new PortableMessageDialog(PowerPlannerResources.GetString("String_NoNameMessageBody")).ShowAsync();
                    return;
                }

                await TryHandleUserInteractionAsync("Save", async (cancellationTask) =>
                {
                    SavedGradeScalesManager manager = await SavedGradeScalesManager.GetForAccountAsync(MainScreenViewModel.CurrentAccount);
                    cancellationTask.ThrowIfCancellationRequested();

                    if (manager == null)
                        throw new NullReferenceException("manager was null");

                    await manager.SaveGradeScale(name, _parameter.Scales);

                    if (_parameter.OnSaved != null)
                        _parameter.OnSaved.Invoke();

                    // We cancel after invoking the OnSaved, since otherwise the views wouldn't be updated correctly
                    cancellationTask.ThrowIfCancellationRequested();

                    this.RemoveViewModel();

                }, "Failed to save. Your error has been reported.");
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);

                await new PortableMessageDialog("Failed to save. Your error has been reported.").ShowAsync();
                return;
            }
        }
    }
}
