using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using PowerPlannerSending;
using PowerPlannerUWP.Views.GradeViews;
using PowerPlannerUWPLibrary;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClassWhatIfView : MainScreenContentViewHostGeneric
    {
        public new ClassWhatIfViewModel ViewModel
        {
            get { return base.ViewModel as ClassWhatIfViewModel; }
            set { base.ViewModel = value; }
        }

        public ClassWhatIfView()
        {
            this.InitializeComponent();

            GradeWhatIfProperties.SetIsInWhatIfMode(this, true);

            Visibility = Visibility.Collapsed;
        }

        private AppBarButton _appBarAddGrade;
        private AppBarButton AppBarAddGrade
        {
            get
            {
                if (_appBarAddGrade == null)
                    _appBarAddGrade = CreateAppBarButton(Symbol.Add, LocalizedResources.GetString("String_NewGrade"), appBarAddGrade_Click);

                return _appBarAddGrade;
            }
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            SetCommandBarPrimaryCommands(AppBarAddGrade);
            
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateDesiredGPAText();
            UpdateDesiredGradeText();

            Visibility = Visibility.Visible;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.DesiredGPA):
                    UpdateDesiredGPAText();
                    break;

                case nameof(ViewModel.DesiredGrade):
                    UpdateDesiredGradeText();
                    break;
            }
        }

        private void UpdateDesiredGradeText()
        {
            if (ViewModel.DesiredGrade == PowerPlannerSending.Grade.UNGRADED)
            {
                TextBoxDesiredGrade.Text = "";
            }
            else
            {
                TextBoxDesiredGrade.Text = (ViewModel.DesiredGrade * 100).ToString();
            }
        }

        private void UpdateDesiredGPAText()
        {
            if (ViewModel.DesiredGPA == PowerPlannerSending.Grade.UNGRADED)
            {
                TextBoxDesiredGPA.Text = "";
            }
            else
            {
                TextBoxDesiredGPA.Text = ViewModel.DesiredGPA.ToString();
            }
        }

        private void appBarAddGrade_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddGrade();
        }

        private void TextBoxDesiredGrade_TextChanged(object sender, TextChangedEventArgs e)
        {
            double desiredGrade;

            if (!double.TryParse(TextBoxDesiredGrade.Text, out desiredGrade))
            {
                // TODO: Highlight text box red
                return;
            }

            desiredGrade = desiredGrade / 100;

            ViewModel.DesiredGrade = desiredGrade;
        }

        private void TextBoxDesiredGPA_TextChanged(object sender, TextChangedEventArgs e)
        {
            double gpa;

            if (double.TryParse(TextBoxDesiredGPA.Text, out gpa))
            {
                ViewModel.DesiredGPA = gpa;
            }
            else
            {
                ViewModel.DesiredGPA = PowerPlannerSending.Grade.UNGRADED;
            }
        }

        private void ButtonExpandCollapseDescription_Click(object sender, RoutedEventArgs e)
        {
            if (RichTextBlockDescription.MaxLines == 2)
            {
                RichTextBlockDescription.MaxLines = int.MaxValue;
                //BorderClickForMoreInfo.Visibility = Visibility.Collapsed;
            }

            else
            {
                RichTextBlockDescription.MaxLines = 2;
                //BorderClickForMoreInfo.Visibility = Visibility.Visible;
            }
        }

        private void WeightCategoryListViewItem_OnRequestViewGrade(object sender, BaseViewItemMegaItem e)
        {
            ViewModel.ShowItem(e);
        }
    }
}
