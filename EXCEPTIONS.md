Android examples...

    EXAMPLE #1 (frequently happening on user devices)

    Arg_TargetInvocationException Cannot access disposed object with JniIdentityHashCode=205872007. ObjectDisposed_ObjectName_Name, Vx.Droid.Views.DroidListView+VxDroidRecyclerAdapter
    
    System.Reflection.TargetInvocationException
        System.Reflection.MethodBaseInvoker.InvokeDirectByRefWithFewArgs
        [external code]
        ToolsPortable.WeakEventHandler`1[[System.Collections.Specialized.NotifyCollectionChangedEventArgs, System.ObjectModel, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a]].Handler
    Inner exception System.ObjectDisposedException handled at System.Reflection.MethodBaseInvoker.InvokeDirectByRefWithFewArgs
        Java.Interop.JniPeerMembers.AssertSelf
        Java.Interop.JniPeerMembers+JniInstanceMethods.InvokeNonvirtualVoidMethod
        AndroidX.RecyclerView.Widget.RecyclerView+Adapter.NotifyItemRangeInserted
        InterfacesDroid.Adapters.ObservableRecyclerViewAdapter.ItemsSource_CollectionChanged
        unknown
        [external code]
        System.Reflection.MethodBaseInvoker.InvokeDirectByRefWithFewArgs

iOS examples...

    EXAMPLE #1 (maybe less common?)

    Object reference not set to an instance of an object

    System.NullReferenceException
        Microsoft.Data.Sqlite.SqliteConnection.Open
        [external code]
        PowerPlannerAppDataLibrary.ViewItemsGroups.AgendaViewItemsGroup+<LoadBlocking>d__23.MoveNext
        [external code]
        PowerPlannerAppDataLibrary.ViewItemsGroups.AgendaViewItemsGroup+<CreateLoadTask>d__19.MoveNext
        [external code]
        PowerPlannerAppDataLibrary.DataLayer.AccountDataStore+<GetAllUpcomingItemsForWidgetAsync>d__101.MoveNext
        [external code]
        PowerPlanneriOS.Helpers.WidgetsHelper+<>c+<<UpdatePrimaryWidgetAsync>b__4_0>d.MoveNext

Windows examples...

    EXAMPLE #1 (somewhat frequent)

    Operations that change non-concurrent collections must have exclusive access. A concurrent update was performed on this collection and corrupted its state. The collection's state is no longer correct.

    System.InvalidOperationException
        System.ThrowHelper.ThrowInvalidOperationException_ConcurrentOperationsNotSupported
        [external code]
        lambda_method656
        [external code]
        PowerPlannerUWP.Extensions.AppointmentsHelper+AppointmentsResetHelper+<LoadAllDataBlocking>d__6.MoveNext
        [external code]
        PowerPlannerUWP.Extensions.AppointmentsHelper+AppointmentsResetHelper+<ResetAllAsync>d__2.MoveNext