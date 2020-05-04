﻿using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewItems
{
    public class ViewItemSchool : BaseViewItemWithoutData, IGPACredits
    {
        public MyObservableList<ViewItemYear> Years { get; private set; } = new MyObservableList<ViewItemYear>();

        public ViewItemSchool(
            Func<DataItemYear, ViewItemYear> createYearMethod)
        {
            AddChildrenHelper(new ViewItemChildrenHelper<DataItemYear, ViewItemYear>(
                isChild: i => true,
                addMethod: Add,
                removeMethod: Remove,
                createChildMethod: createYearMethod,
                children: Years));
        }

        internal void Add(ViewItemYear year)
        {
            Years.InsertSorted(year);
        }

        internal void Remove(ViewItemYear year)
        {
            Years.Remove(year);
        }

        private double _gpa = PowerPlannerSending.Grade.UNGRADED;
        public double GPA
        {
            get { return _gpa; }
            set { SetProperty(ref _gpa, value, nameof(GPA)); }
        }

        private double _creditsEarned = PowerPlannerSending.Grade.UNGRADED;
        public double CreditsEarned
        {
            get { return _creditsEarned; }
            set { SetProperty(ref _creditsEarned, value, nameof(CreditsEarned)); }
        }

        private double _creditsAffectingGpa = PowerPlannerSending.Grade.UNGRADED;
        public double CreditsAffectingGpa
        {
            get { return _creditsAffectingGpa; }
            set { SetProperty(ref _creditsAffectingGpa, value, nameof(CreditsAffectingGpa)); }
        }

        public bool HasGrades => true;

        public void CalculateEverything()
        {
            for (int i = 0; i < Years.Count; i++)
                Years[i].CalculateEverything();

            Calculate();
        }

        private void Calculate()
        {
            GPACalculator.Answer answer = GPACalculator.Calculate(Years);

            CreditsEarned = answer.CreditsEarned;
            CreditsAffectingGpa = answer.CreditsAffectingGpa;
            GPA = answer.GPA;
        }
    }
}
