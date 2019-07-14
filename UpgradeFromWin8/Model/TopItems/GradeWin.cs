using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpgradeFromWin8.Model.TopItems
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model.TopItems")]
    public class GradeWin : BaseHomeworkExamGradeWin, IComparable
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Grade; }
        }

        private double _gradeReceived;
        [DataMember]
        public double GradeReceived
        {
            get { return _gradeReceived; }
            set { SetProperty(ref _gradeReceived, value, "GradeReceived", "GradePercent", "IsGraded", "SubTitle"); }
        }

        private double _gradeTotal;
        [DataMember]
        public double GradeTotal
        {
            get { return _gradeTotal; }
            set { SetProperty(ref _gradeTotal, value, "GradeTotal", "GradePercent", "SubTitle"); }
        }

        private bool _isDropped;
        [DataMember]
        public bool IsDropped
        {
            get { return _isDropped; }
            set { SetProperty(ref _isDropped, value, "IsDropped", "SubTitle"); }
        }

        private double _individualWeight;
        [DataMember]
        public double IndividualWeight
        {
            get { return _individualWeight; }
            set { SetProperty(ref _individualWeight, value, "IndividualWeight"); }
        }

        #region DisplayProperties

        public string SubTitle
        {
            get
            {
                string answer = "";

                if (this.IsDropped)
                    answer = "DROPPED - ";

                if (this.GradeReceived == Grade.UNGRADED)
                    return answer + "----  -  --/" + this.GradeTotal;

                if (this.GradeTotal == 0)
                    return answer + "Extra Credit - " + this.GradeReceived;

                return answer + this.GradePercent.ToString("0.##%") + "  -  " + this.GradeReceived + "/" + this.GradeTotal;
            }
        }

        /// <summary>
        /// Between 0.0 and 1.0 (other than UNGRADED)
        /// </summary>
        public double GradePercent
        {
            get
            {
                if (GradeReceived == Grade.UNGRADED)
                    return Grade.UNGRADED;

                return GradeReceived / GradeTotal;
            }
        }

        private bool wasChanged;
        public bool WasChanged
        {
            get { return wasChanged; }
            set { SetProperty(ref wasChanged, value, "WasChanged"); setWhatIfUpdated(); }
        }

        public bool DreamMode
        {
            get { return GradeReceived == Grade.UNGRADED; }
        }

        private bool appIsInWhatIfMode;
        public bool AppIsInWhatIfMode
        {
            get { return appIsInWhatIfMode; }
            set { SetProperty(ref appIsInWhatIfMode, value, "AppIsInWhatIfMode"); setWhatIfUpdated(); }
        }

        private void setWhatIfUpdated()
        {
            OnPropertyChanged("SubTitleColorArray");
        }

        /// <summary>
        /// If the grade is in the WhatIf mode, or if the grade was changed, it'll return the class's color. Otherwise it'll return gray.
        /// </summary>
        public byte[] SubTitleColorArray
        {
            get
            {
                if (AppIsInWhatIfMode || WasChanged)
                {
                    if (Parent != null && Parent.Parent != null)
                        return (Parent.Parent as ClassWin).RawColor;
                }

                //gray
                return new byte[] { 190, 190, 190 };
            }
        }

        /// <summary>
        /// Returns true if the item is graded.
        /// </summary>
        public bool IsGraded
        {
            get { return GradeReceived != Grade.UNGRADED; }
        }

        /// <summary>
        /// Returns true if the item IsGraded AND if the item isn't dropped
        /// </summary>
        public bool DoesCount
        {
            get { return IsGraded && !IsDropped; }
        }

        public bool IsExtraCredit
        {
            get { return GradeTotal == 0; }
        }

        #endregion


        public new int CompareTo(object obj)
        {
            if (obj is GradeWin)
            {
                GradeWin other = obj as GradeWin;

                //oldest grades are shown first
                int compare = Date.Date.CompareTo(other.Date.Date);

                if (compare == 0)
                {
                    //dropped items go after
                    if (IsDropped && !other.IsDropped)
                        return 1;

                    //undropped items go before anything dropped
                    if (!IsDropped && other.IsDropped)
                        return -1;

                    //more recently created items appear first if on same day
                    return DateCreated.CompareTo((obj as GradeWin).DateCreated);
                }

                return compare;
            }

            return 0;
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.GradeReceived.ToString()] = GradeReceived;
            into[PropertyNames.GradeTotal.ToString()] = GradeTotal;
            into[PropertyNames.IsDropped.ToString()] = IsDropped;
            into[PropertyNames.IndividualWeight.ToString()] = IndividualWeight;

            base.serialize(into);
        }

        protected override BaseItem serialize()
        {
            Grade into = new Grade()
            {
                GradeReceived = GradeReceived,
                GradeTotal = GradeTotal,
                IsDropped = IsDropped,
                IndividualWeight = IndividualWeight
            };

            base.serialize(into);

            return into;
        }

        public override object GetPropertyValue(PropertyNames p)
        {
            switch (p)
            {
                case PropertyNames.GradeReceived:
                    return GradeReceived;

                case PropertyNames.GradeTotal:
                    return GradeTotal;

                case PropertyNames.IsDropped:
                    return IsDropped;

                case PropertyNames.IndividualWeight:
                    return IndividualWeight;
            }

            return base.GetPropertyValue(p);
        }

        protected override void deserialize(BaseItem item, List<PropertyNames> changedProperties)
        {
            Grade from = item as Grade;

            if (changedProperties != null)
            {
                if (GradeReceived != from.GradeReceived)
                    changedProperties.Add(PropertyNames.GradeReceived);

                if (GradeTotal != from.GradeTotal)
                    changedProperties.Add(PropertyNames.GradeTotal);

                if (IsDropped != from.IsDropped)
                    changedProperties.Add(PropertyNames.IsDropped);

                if (IndividualWeight != from.IndividualWeight)
                    changedProperties.Add(PropertyNames.IndividualWeight);
            }

            GradeReceived = from.GradeReceived;
            GradeTotal = from.GradeTotal;
            IsDropped = from.IsDropped;
            IndividualWeight = from.IndividualWeight;

            base.deserialize(from, changedProperties);
        }

        public GradeWin CopyForWhatIf(WeightCategoryWin newParent)
        {
            GradeWin g = new GradeWin()
            {
                GradeReceived = GradeReceived,
                GradeTotal = GradeTotal,
                Name = Name,
                Details = Details,
                Date = Date,
                Updated = Updated,
                Parent = newParent,
                IndividualWeight = IndividualWeight,
                IsDropped = IsDropped,
                AppIsInWhatIfMode = !IsGraded && !IsDropped //if it hasn't been graded and isn't dropped, it's in dream mode
            };

            return g;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
