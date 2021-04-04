using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;

namespace Vx.Views
{
    //public class LinearLayout : VxComponentForms
    //{
    //    private ObservableCollection<View> _children = new ObservableCollection<View>();

    //    public LinearLayout()
    //    {
    //        _children.CollectionChanged += _children_CollectionChanged;
    //    }

    //    private void _children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    //    {
    //        MarkDirty();
    //    }

    //    public new IList<View> Children => _children;

    //    public StackOrientation Orientation
    //    {
    //        get => GetProperty(StackOrientation.Horizontal);
    //        set => SetProperty(value);
    //    }

    //    protected override View Render()
    //    {
    //        var grid = new Grid();
    //        var orientation = Orientation;

    //        if (orientation == StackOrientation.Horizontal)
    //        {
    //            foreach (var child in Children)
    //            {
    //                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = })
    //            }
    //        }
    //    }
    //}

    //public class LinearLayout : Grid
    //{
    //    public LinearLayout()
    //    {
    //        ChildrenReordered += LinearLayout_ChildrenReordered;
    //        ChildAdded += LinearLayout_ChildAdded;
    //    }

    //    private void LinearLayout_ChildrenReordered(object sender, EventArgs e)
    //    {

    //    }

    //    private void LinearLayout_ChildAdded(object sender, ElementEventArgs e)
    //    {

    //    }

    //    public StackOrientation Orientation
    //    {
    //        get => (StackOrientation)GetValue(OrientationProperty);
    //        set => SetValue(OrientationProperty, value);
    //    }

    //    public static readonly BindableProperty OrientationProperty = BindableProperty.Create(nameof(Orientation), typeof(StackOrientation), typeof(LinearLayout), StackOrientation.Vertical, propertyChanged: OrientationChanged);

    //    private static void OrientationChanged(BindableObject sender, object oldVal, object newVal)
    //    {

    //    }

    //    private void OrientationChanged()
    //    {

    //    }

    //    public static readonly BindableProperty WeightProperty = BindableProperty.CreateAttached("Weight", typeof(double), typeof(LinearLayout), 0, propertyChanged: WeightChanged);

    //    public static double GetWeight(BindableObject target)
    //    {
    //        return (double)target.GetValue(WeightProperty);
    //    }

    //    public static void SetWeight(BindableObject target, double value)
    //    {
    //        target.SetValue(WeightProperty, value);
    //    }

    //    private static void WeightChanged(BindableObject sender, object oldVal, object newVal)
    //    {

    //    }

    //    private void Reset()
    //    {
    //        List<GridLength> lengths = new List<GridLength>();

    //        if (Orientation == StackOrientation.Horizontal)
    //        {

    //        }
    //    }
    //}
}
