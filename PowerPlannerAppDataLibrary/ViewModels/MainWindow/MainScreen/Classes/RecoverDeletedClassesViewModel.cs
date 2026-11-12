using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Extensions;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years
{
    internal class RecoverDeletedClassesViewModel : PopupComponentViewModel
    {
        private VxState<PastDeletedItem[]> _deletedClasses = new VxState<PastDeletedItem[]>(null);
        private VxState<string> _error = new VxState<string>(null);
        private VxState<PastDeletedItem> _recoveringItem = new VxState<PastDeletedItem>(null);

        public RecoverDeletedClassesViewModel(BaseViewModel parent) : base(parent)
        {
            Title = R.S("RecoverDeletedItems_Title");
        }

        protected override async void Initialize()
        {
            await RefreshItemsAsync();
        }

        private async System.Threading.Tasks.Task RefreshItemsAsync()
        {
            try
            {
                var resp = await MainScreenViewModel.CurrentAccount.PostAuthenticatedAsync<GetDeletedClassesRequest, GetDeletedClassesResponse>(
                    Website.ClientApiUrl + "getdeletedclasses",
                    new GetDeletedClassesRequest
                    {
                        SemesterIdentifier = MainScreenViewModel.CurrentSemester.Identifier
                    });

                if (resp.Error != null)
                {
                    _error.Value = resp.Error;
                }
                else
                {
                    _deletedClasses.Value = resp.DeletedItems;
                }
            }
            catch (Exception ex)
            {
                if (!ExceptionHelper.IsHttpWebIssue(ex))
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }

                _error.Value = R.S("String_OfflineExplanation");
            }
        }

        protected override View Render()
        {
            if (_error.Value != null)
            {
                return RenderGenericPopupContent(new TextBlock
                {
                    Text = _error.Value
                });
            }

            if (_deletedClasses.Value == null)
            {
                return RenderGenericLoadingContent();
            }

            if (_deletedClasses.Value.Length == 0)
            {
                return RenderGenericPopupContent(new TextBlock
                {
                    Text = R.S("RecoverDeletedItems_NothingFound")
                });
            }

            var linLayout = new LinearLayout();

            foreach (var d in _deletedClasses.Value)
            {
                linLayout.Children.Add(new TextBlock
                {
                    Text = d.Name,
                    WrapText = false,
                    FontSize = Theme.Current.SubtitleFontSize
                });

                linLayout.Children.Add(new TextBlock
                {
                    Text = $"Updated: {d.Updated.ToString()}",
                    WrapText = false
                }.CaptionStyle());

                linLayout.Children.Add(new TextBlock
                {
                    Text = $"Created: {d.Updated.ToString()}",
                    WrapText = false
                }.CaptionStyle());

                linLayout.Children.Add(new TextBlock
                {
                    Text = $"Children: {d.CountOfChildren}. {d.ChildrenPreview}",
                    WrapText = false
                }.CaptionStyle());

                linLayout.Children.Add(new TextButton
                {
                    Text = d == _recoveringItem.Value ? R.S("String_Recovering") : R.S("String_Recover"),
                    IsEnabled = _recoveringItem.Value == null, // Only allow recovering if nothing else is currently recovering
                    Click = () => RecoverDeletedItem(d),
                    Margin = new Thickness(0, 0, 0, 12),
                    HorizontalAlignment = HorizontalAlignment.Left
                });
            }

            return RenderGenericPopupContent(linLayout);
        }

        private async void RecoverDeletedItem(PastDeletedItem item)
        {
            _recoveringItem.Value = item;

            try
            {
                var resp = await MainScreenViewModel.CurrentAccount.RecoverDeletedItemAsync(item);
                if (resp.Error != null)
                {
                    await new PortableMessageDialog(resp.Error, R.S("Error")).ShowAsync();
                }
                else
                {
                    // Successfully restored, so refresh the list (there may be new semesters under a recovered year that user could restore)
                    await RefreshItemsAsync();
                }

                _recoveringItem.Value = null;

            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                _error.Value = ex.Message;
            }
        }
    }
}
