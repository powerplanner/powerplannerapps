using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class WidgetAgendaSettingsViewModel : PopupComponentViewModel
    {
        private AccountDataItem _account;

        public WidgetAgendaSettingsViewModel(BaseViewModel parent) : base(parent)
        {
            Title = R.S("String_AgendaWidget");

            _account = base.FindAncestor<MainWindowViewModel>().CurrentAccount;
            if (_account != null)
            {
                _showEvents = _account.MainTileSettings.ShowTasks;
                _showTasks = _account.MainTileSettings.ShowEvents;
                _skipItemsOlderThan = _account.MainTileSettings.SkipItemsOlderThan;
            }
        }

        protected override View Render()
        {
            if (_account == null)
            {
                return null;
            }

            return RenderGenericPopupContent(
                new TextBlock
                {
                    Text = R.S("Settings_WidgetAgenda_Description")
                },

                new Switch
                {
                    Title = R.S("Settings_Tiles_MainTile_ToggleTasks.Header"),
                    IsOn = VxValue.Create(ShowTasks, v => ShowTasks = v),
                    Margin = new Thickness(0, 24, 0, 0)
                },

                new Switch
                {
                    Title = R.S("Settings_Tiles_MainTile_ToggleEvents.Header"),
                    IsOn = VxValue.Create(ShowEvents, v => ShowEvents = v),
                    Margin = new Thickness(0, 12, 0, 0)
                },

                new TextBlock
                {
                    Text = R.S("Widget_SkipItemsExplanation.Text"),
                    Margin = new Thickness(0, 24, 0, 0)
                },

                new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 12, 0, 6),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = R.S("Tile_SkipItems.Text"),
                            WrapText = false,
                            VerticalAlignment = VerticalAlignment.Center
                        },

                        new NumberTextBox
                        {
                            Number = VxValue.Create(
                                SkipItemsOlderThan == int.MinValue ? (double?)null : SkipItemsOlderThan,
                                v => SkipItemsOlderThan = v == null ? int.MinValue : (int)v),
                            Margin = new Thickness(6, 0, 6, 0)
                        },

                        new TextBlock
                        {
                            Text = R.S("Tile_DaysPastToday.Text"),
                            WrapText = false,
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                },

                RenderCaption("Tile_SkipExplanation0.Text"),
                RenderCaption("Tile_SkipExplanation1.Text"),
                RenderCaption("Tile_SkipExplanation2.Text"),
                RenderCaption("Tile_SkipExplanationBlank.Text")
            );
        }

        private TextBlock RenderCaption(string resourceId)
        {
            return new TextBlock
            {
                Text = R.S(resourceId),
                FontSize = Theme.Current.CaptionFontSize,
                TextColor = Theme.Current.SubtleForegroundColor,
                Margin = new Thickness(0, 6, 0, 0)
            };
        }

        private bool _showTasks;
        public bool ShowTasks
        {
            get { return _showTasks; }
            set
            {
                if (_showTasks != value)
                {
                    _showTasks = value;
                    OnPropertyChanged(nameof(ShowTasks));
                    UpdateSettings();
                }
            }
        }

        private bool _showEvents;
        public bool ShowEvents
        {
            get { return _showEvents; }
            set
            {
                if (_showEvents != value)
                {
                    _showEvents = value;
                    OnPropertyChanged(nameof(ShowEvents));
                    UpdateSettings();
                }
            }
        }

        private int _skipItemsOlderThan = int.MinValue;
        public int SkipItemsOlderThan
        {
            get { return _skipItemsOlderThan; }
            set
            {
                if (_skipItemsOlderThan != value)
                {
                    _skipItemsOlderThan = value;
                    OnPropertyChanged(nameof(SkipItemsOlderThan));
                    UpdateSettings();
                }
            }
        }

        private async void UpdateSettings()
        {
            try
            {
                if (_account == null)
                {
                    return;
                }

                _account.MainTileSettings.ShowTasks = ShowTasks;
                _account.MainTileSettings.ShowEvents = ShowEvents;
                _account.MainTileSettings.SkipItemsOlderThan = SkipItemsOlderThan;

                await AccountsManager.Save(_account);

                await TilesExtension.Current?.UpdatePrimaryTileNotificationsAsync();
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
