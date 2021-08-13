using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureClassGpaTypeViewModel : PopupComponentViewModel
    {
        [VxSubscribe]
        public ViewItemClass Class { get; private set; }

        private VxState<bool> _isEnabled = new VxState<bool>(true);

        public ConfigureClassGpaTypeViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;
            Title = PowerPlannerResources.GetString("Settings_GradeOptions_GpaType");
        }

        protected override View Render()
        {
            var layout = new LinearLayout
            {
                Children =
                {
                    RenderOption(
                        PowerPlannerResources.GetString("Settings_GradeOptions_GpaType_Standard.Text"),
                        PowerPlannerResources.GetString("Settings_GradeOptions_GpaType_StandardExplanation.Text"),
                        Class.GpaType == GpaType.Standard,
                        () => Save(GpaType.Standard)),

                    RenderOption(
                        PowerPlannerResources.GetString("Settings_GradeOptions_GpaType_PassFail.Text"),
                        PowerPlannerResources.GetString("Settings_GradeOptions_GpaType_PassFailExplanation.Text"),
                        Class.GpaType == GpaType.PassFail,
                        () => Save(GpaType.PassFail))
                }
            };

            return new ScrollView(layout);
        }

        private View RenderOption(string title, string subtitle, bool isChecked, Action checkedAction)
        {
            return new TransparentContentButton
            {
                Opacity = _isEnabled.Value ? 1 : 0.7f,
                Content = new LinearLayout
                {
                    Margin = new Thickness(Theme.Current.PageMargin),
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new LinearLayout
                        {
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = title,
                                    FontWeight = FontWeights.Bold,
                                    WrapText = false
                                },

                                new TextBlock
                                {
                                    Text = subtitle,
                                    TextColor = Theme.Current.SubtleForegroundColor
                                }
                            }
                        }.LinearLayoutWeight(1),

                        new FontIcon
                        {
                            Glyph = MaterialDesign.MaterialDesignIcons.Check,
                            Color = Theme.Current.AccentColor,
                            FontSize = 30,
                            Opacity = isChecked ? 1 : 0
                        }
                    }
                },

                Click = () =>
                {
                    if (_isEnabled.Value)
                    {
                        checkedAction();
                    }
                }
            };
        }

        private async void Save(GpaType gpaType)
        {
            try
            {
                _isEnabled.Value = false;

                var changes = new DataChanges();

                // Class changes
                var c = new DataItemClass()
                {
                    Identifier = Class.Identifier,
                    GpaType = gpaType
                };

                changes.Add(c);

                await TryHandleUserInteractionAsync("Save", async delegate
                {
                    await PowerPlannerApp.Current.SaveChanges(changes);

                }, "Failed to save. Your error has been reported.");
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                await new PortableMessageDialog("Error encountered while saving. Your error report has been sent to the developer.", "Error").ShowAsync();
            }

            finally
            {
                _isEnabled.Value = true;
            }
        }
    }
}
