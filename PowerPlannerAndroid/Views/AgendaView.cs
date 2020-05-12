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

namespace PowerPlannerAndroid.Views
{
    public class AgendaView : InterfacesDroid.Views.PopupViewHost<AgendaViewModel>
    {
        private FloatingAddItemControl _addItemControl;

        public AgendaView(ViewGroup root) : base(Resource.Layout.Agenda, root)
        {
        }

        public override void OnViewModelLoadedOverride()
        {
            _addItemControl = FindViewById<FloatingAddItemControl>(Resource.Id.FloatingAddItemControl);
            _addItemControl.OnRequestAddExam += _addItemControl_OnRequestAddExam;
            _addItemControl.OnRequestAddHomework += _addItemControl_OnRequestAddHomework;
            RecyclerView recyclerView = FindViewById<RecyclerView>(Resource.Id.RecyclerViewAgenda);
            
            //recyclerView.AddItemDecoration(new DividerItemDecoration(Context));

            // Use this setting to improve performance if you know that changes
            // in content do not change the layout size of the RecyclerView
            //recyclerView.HasFixedSize = true;

            // Use a linear layout manager
            var layoutManager = new LinearLayoutManager(Context);
            recyclerView.SetLayoutManager(layoutManager);

            // Specify the adapter
            var adapter = new AgendaHomeworkAdapter()
            {
                ItemsSource = ViewModel.ItemsWithHeaders
            };
            adapter.ItemClick += Adapter_ItemClick;
            recyclerView.SetAdapter(adapter);
        }

        private void _addItemControl_OnRequestAddHomework(object sender, EventArgs e)
        {
            ViewModel.AddHomework();
        }

        private void _addItemControl_OnRequestAddExam(object sender, EventArgs e)
        {
            ViewModel.AddExam();
        }

        private void Adapter_ItemClick(object sender, ViewItemTaskOrEvent e)
        {
            ViewModel.ShowItem(e);
        }
    }
}