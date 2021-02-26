using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Agenda;
using InterfacesDroid.Views;
using PowerPlannerAndroid.Adapters;
using PowerPlannerAppDataLibrary.ViewItems;
using InterfacesDroid.Decorations;
using Android.Graphics.Drawables;
using Android.Graphics;
using InterfacesDroid.Themes;
using PowerPlannerAndroid.Views.Controls;
using AndroidX.RecyclerView.Widget;
using PowerPlannerAndroid.ViewHosts;
using AndroidX.CoordinatorLayout.Widget;
using PowerPlannerAndroid.Vx;

namespace PowerPlannerAndroid.Views
{
    public class AgendaView : MainScreenVxViewHostDescendant<AgendaViewModel>
    {
        private FloatingAddItemControl _addItemControl;
        private RecyclerView _recyclerView;

        public AgendaView(Context context) : base(context)
        {
            View = new CoordinatorLayout(context)
                .VxLayoutParams().StretchHeight().Apply()
                .VxChildren(

                    new VxRecyclerView(context)
                        .VxLayoutParams().StretchHeight().Apply()
                        .VxPadding(0, 0, 0, 90)
                        .VxClipToPadding(false)
                        .VxReference(ref _recyclerView),

                    // No items text
                    new LinearLayout(context)
                        .VxLayoutParams().Gravity(GravityFlags.CenterVertical).Margins(16, 0, 16, 0).ApplyForCoordinatorLayout()
                        .VxOrientation(Android.Widget.Orientation.Vertical)
                        .VxVisibility(Binding(nameof(ViewModel.HasNoItems)))
                        .VxChildren(

                            new VxTextView(context, VxTextStyle.Large)
                                .VxTextLocalized("Agenda_NoItemsHeader.Text")
                                .VxGravity(GravityFlags.CenterHorizontal),

                            new TextView(context)
                                .VxTextLocalized("Agenda_NoItemsDescription.Text")
                                .VxGravity(GravityFlags.CenterHorizontal)

                        ),

                    new FloatingAddItemControl(context)
                        .VxLayoutParams().StretchHeight().Apply()
                        .VxReference(ref _addItemControl)

                );
        }

        public override void OnViewModelLoadedOverride()
        {
            _addItemControl.OnRequestAddEvent += _addItemControl_OnRequestAddEvent;
            _addItemControl.OnRequestAddTask += _addItemControl_OnRequestAddTask;
            
            //recyclerView.AddItemDecoration(new DividerItemDecoration(Context));

            // Use this setting to improve performance if you know that changes
            // in content do not change the layout size of the RecyclerView
            //recyclerView.HasFixedSize = true;

            // Use a linear layout manager
            var layoutManager = new LinearLayoutManager(Context);
            _recyclerView.SetLayoutManager(layoutManager);

            // Specify the adapter
            var adapter = new AgendaTasksOrEventsAdapter()
            {
                ItemsSource = ViewModel.ItemsWithHeaders
            };
            adapter.ItemClick += Adapter_ItemClick;
            _recyclerView.SetAdapter(adapter);
        }

        private void _addItemControl_OnRequestAddTask(object sender, EventArgs e)
        {
            ViewModel.AddTask();
        }

        private void _addItemControl_OnRequestAddEvent(object sender, EventArgs e)
        {
            ViewModel.AddEvent();
        }

        private void Adapter_ItemClick(object sender, ViewItemTaskOrEvent e)
        {
            ViewModel.ShowItem(e);
        }
    }
}