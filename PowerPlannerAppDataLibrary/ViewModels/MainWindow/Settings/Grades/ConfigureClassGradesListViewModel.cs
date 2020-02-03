using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureClassGradesListViewModel : BaseMainScreenViewModelDescendant
    {
        public ViewItemClass Class { get; private set; }

        public ConfigureClassGradesListViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;
        }

        public void ConfigureCredits()
        {
            if (ConfigureClassGradesViewModel.UsePopups)
            {
                FindAncestor<PagedViewModelWithPopups>().ShowPopup(new ConfigureClassCreditsViewModel(FindAncestor<PagedViewModelWithPopups>(), Class));
            }
            else
            {
                FindAncestor<PagedViewModel>().Navigate(new ConfigureClassCreditsViewModel(FindAncestor<PagedViewModel>(), Class));
            }
        }

        public void ConfigureGradeScale()
        {
            if (ConfigureClassGradesViewModel.UsePopups)
            {
                FindAncestor<PagedViewModelWithPopups>().ShowPopup(new ConfigureClassGradeScaleViewModel(FindAncestor<PagedViewModelWithPopups>(), Class));
            }
            else
            {
                FindAncestor<PagedViewModel>().Navigate(new ConfigureClassGradeScaleViewModel(FindAncestor<PagedViewModel>(), Class));
            }
        }

        public void ConfigureWeightCategories()
        {
            if (ConfigureClassGradesViewModel.UsePopups)
            {
                FindAncestor<PagedViewModelWithPopups>().ShowPopup(new ConfigureClassWeightCategoriesViewModel(FindAncestor<PagedViewModelWithPopups>(), Class));
            }
            else
            {
                FindAncestor<PagedViewModel>().Navigate(new ConfigureClassWeightCategoriesViewModel(FindAncestor<PagedViewModel>(), Class));
            }
        }

        public void ConfigureAverageGrades()
        {
            if (ConfigureClassGradesViewModel.UsePopups)
            {
                FindAncestor<PagedViewModelWithPopups>().ShowPopup(new ConfigureClassAverageGradesViewModel(FindAncestor<PagedViewModelWithPopups>(), Class));
            }
            else
            {
                FindAncestor<PagedViewModel>().Navigate(new ConfigureClassAverageGradesViewModel(FindAncestor<PagedViewModel>(), Class));
            }
        }

        public void ConfigureRoundGradesUp()
        {
            if (ConfigureClassGradesViewModel.UsePopups)
            {
                FindAncestor<PagedViewModelWithPopups>().ShowPopup(new ConfigureClassRoundGradesUpViewModel(FindAncestor<PagedViewModelWithPopups>(), Class));
            }
            else
            {
                FindAncestor<PagedViewModel>().Navigate(new ConfigureClassRoundGradesUpViewModel(FindAncestor<PagedViewModel>(), Class));
            }
        }

        public void ConfigureGpaType()
        {
            if (ConfigureClassGradesViewModel.UsePopups)
            {
                FindAncestor<PagedViewModelWithPopups>().ShowPopup(new ConfigureClassGpaTypeViewModel(FindAncestor<PagedViewModelWithPopups>(), Class));
            }
            else
            {
                FindAncestor<PagedViewModel>().Navigate(new ConfigureClassGpaTypeViewModel(FindAncestor<PagedViewModel>(), Class));
            }
        }

        public void ConfigurePassingGrade()
        {
            if (ConfigureClassGradesViewModel.UsePopups)
            {
                FindAncestor<PagedViewModelWithPopups>().ShowPopup(new ConfigureClassPassingGradeViewModel(FindAncestor<PagedViewModelWithPopups>(), Class));
            }
            else
            {
                FindAncestor<PagedViewModel>().Navigate(new ConfigureClassPassingGradeViewModel(FindAncestor<PagedViewModel>(), Class));
            }
        }
    }
}
