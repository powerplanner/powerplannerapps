# UI Stability Investigation Report

## The Reported Exception

```
Arg_TargetInvocationException Cannot access disposed object with JniIdentityHashCode=205872007.
ObjectDisposed_ObjectName_Name, Vx.Droid.Views.DroidListView+VxDroidRecyclerAdapter

System.Reflection.TargetInvocationException
    System.Reflection.MethodBaseInvoker.InvokeDirectByRefWithFewArgs
    [external code]
    ToolsPortable.WeakEventHandler`1[[System.Collections.Specialized.NotifyCollectionChangedEventArgs, ...]].Handler
Inner exception System.ObjectDisposedException handled at System.Reflection.MethodBaseInvoker.InvokeDirectByRefWithFewArgs
    Java.Interop.JniPeerMembers.AssertSelf
    Java.Interop.JniPeerMembers+JniInstanceMethods.InvokeNonvirtualVoidMethod
    AndroidX.RecyclerView.Widget.RecyclerView+Adapter.NotifyItemRangeInserted
    InterfacesDroid.Adapters.ObservableRecyclerViewAdapter.ItemsSource_CollectionChanged
```

## Root Cause Analysis

### The Primary Bug: `ObservableRecyclerViewAdapter` never unsubscribes from `CollectionChanged` when the native Android adapter is disposed

The root cause is a lifecycle mismatch between the **managed .NET object** (`ObservableRecyclerViewAdapter` / `VxDroidRecyclerAdapter`) and the **Java-side peer object** (the Android `RecyclerView.Adapter`).

**Here is the exact sequence that causes the crash:**

1. A `DroidListView` is created, which creates a `VxDroidRecyclerAdapter` (extends `ObservableRecyclerViewAdapter`) and attaches it to a `RecyclerView`.
2. `ApplyProperties` assigns an `IReadOnlyList<object>` that implements `INotifyCollectionChanged` to `ItemsSource`.
3. In the `ItemsSource` setter ([ObservableRecyclerViewAdapter.cs, line 107](Vx.Droid/Adapters/ObservableRecyclerViewAdapter.cs#L107)), a `WeakEventHandler` is created and subscribed to the collection's `CollectionChanged` event.
4. The view is removed from the visual tree (e.g., user navigates away). Android **disposes the Java peer** of the `RecyclerView.Adapter` (the JNI reference is released).
5. The **managed C# object** (`VxDroidRecyclerAdapter`) is still alive because:
   - The `WeakEventHandler` holds a `WeakReference` to the adapter, but the adapter hasn't been GC'd yet — it's still reachable from the `DroidListView` field `_adapter`.
   - The collection (`ItemsSource`) holds a strong reference to the `_collectionChangedHandler` delegate.
6. The underlying **data collection changes** (an item is added/removed).
7. The collection fires `CollectionChanged`.
8. The `WeakEventHandler.Handler` is invoked. It checks `_targetReference.Target` — the managed adapter is still alive (not GC'd), so it proceeds.
9. It calls `_methodInfo.Invoke(target, ...)` which invokes `ItemsSource_CollectionChanged` on the adapter.
10. `ItemsSource_CollectionChanged` calls `NotifyItemRangeInserted` — a Java interop call.
11. **The Java peer is already disposed** → `ObjectDisposedException` → wrapped in `TargetInvocationException`.

### Why `WeakEventHandler` doesn't prevent this crash

The `WeakEventHandler` ([WeakEventHandler.cs](portablelibraries/ToolsPortable/ToolsPortable/WeakEventHandler.cs)) is designed to prevent **memory leaks** by allowing the subscriber to be garbage-collected. However, it does **not** prevent **use-after-dispose** — which is a fundamentally different problem on Android/Xamarin.

On Android with .NET bindings, there are **two lifetimes** for every Java-wrapping object:

| Lifetime | Controlled by | Ended by |
|---|---|---|
| Java peer (JNI reference) | Android framework | View detached from window, RecyclerView clears adapter, GC on Java side |
| Managed C# object | .NET GC | No more strong references |

The `WeakEventHandler` only tracks the **managed object** lifetime. The managed adapter object can be alive (not GC'd) while its Java peer is already disposed. This is the window in which the crash occurs.

The `WeakEventHandler` does try to catch `ObjectDisposedException` ([line 46-52](portablelibraries/ToolsPortable/ToolsPortable/WeakEventHandler.cs#L46)):
```csharp
catch (TargetInvocationException ex) when (ContainsObjectDisposedException(ex))
{
    _targetReference = null;
    _methodInfo = null;
    ExceptionHelper.ReportHandledException(ex);
}
```
This means the exception **is caught and reported** (not crashing the app), but the problematic operation (`NotifyItemRangeInserted` on a disposed adapter) has already been attempted, which may leave Android's `RecyclerView` in an inconsistent state.

---

## Architectural Issues Found

### Issue 1: No Disposal/Cleanup Lifecycle in the Vx Framework

**Severity: Critical**

The entire `Vx` framework has **no `IDisposable` implementation** anywhere — not on `NativeView`, not on `VxComponent`, not on `View`. There is no formal mechanism to tell a native view "you are being removed, clean up your resources."

- `DroidListView` creates a `VxDroidRecyclerAdapter` but **never** clears its `ItemsSource` when the view is removed.
- `ObservableRecyclerViewAdapter` has no `Dispose()`, no `OnDetachedFromRecyclerView()` override, and no cleanup method.
- `DroidNativeComponent` ([DroidNativeComponent.cs](Vx.Droid/DroidNativeComponent.cs)) has no `OnDetachedFromWindow` override — it doesn't notify its `VxComponent` that it's been detached.

**Contrast with `InflatedViewWithBinding`** ([InflatedViewWithBinding.cs, line 162](Vx.Droid/DroidViews/InflatedViewWithBinding.cs#L162)) — the legacy binding system **does** properly handle `OnDetachedFromWindow` by calling `BindingApplicator.BindingHost.Detach()`. The newer Vx framework does not have an equivalent.

### Issue 2: `VxComponent.Detach()` Is Only Called Reactively on Crash

**Severity: High**

`VxComponent.Detach()` ([VxComponent.cs, line 469](Vx/Views/VxComponent.cs#L469)) unsubscribes from states, properties, and collections — exactly the cleanup that's needed. But it's **only called when an `ObjectDisposedException` is already caught** during `RenderActual()` ([line 447-455](Vx/Views/VxComponent.cs#L447)):

```csharp
catch (ObjectDisposedException ex)
{
    if (VxPlatform.Current == Platform.Android)
    {
        Detach();
    }
    else
    {
        throw ex;
    }
}
```

This is a **reactive** fix — cleanup happens only after a crash has already occurred. `Detach()` should be called **proactively** when the component is removed from the visual tree.

### Issue 3: `ObservableRecyclerViewAdapter.ItemsSource` Setter Leaks Event Subscriptions

**Severity: Critical (Direct cause of reported crash)**

In [ObservableRecyclerViewAdapter.cs, lines 93-113](Vx.Droid/Adapters/ObservableRecyclerViewAdapter.cs#L93):

```csharp
public IReadOnlyList<object> ItemsSource
{
    set
    {
        var old = ItemsSource;
        if (old is INotifyCollectionChanged)
            (old as INotifyCollectionChanged).CollectionChanged -= _collectionChangedHandler;

        _itemsSource = value;

        if (value is INotifyCollectionChanged)
        {
            _collectionChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(
                ItemsSource_CollectionChanged).Handler;
            (value as INotifyCollectionChanged).CollectionChanged += _collectionChangedHandler;
        }

        NotifyDataSetChanged();
    }
}
```

Problems:
1. **No cleanup when the adapter is removed from the RecyclerView.** The subscription persists until either the managed object is GC'd (WeakEventHandler cleans up) or `ItemsSource` is reassigned.
2. **A new `WeakEventHandler` is created each time `ItemsSource` is set**, even to the same collection. The old handler delegate stored in `_collectionChangedHandler` is used to unsubscribe, but since a **new delegate wrapper** was created each time, this is fragile.
3. **There is no `OnDetachedFromRecyclerView` override** to clear the subscription when the adapter is removed.

### Issue 4: `DroidListView` Doesn't Clean Up Its Adapter

**Severity: High**

`DroidListView` ([DroidListView.cs](Vx.Droid/Views/DroidListView.cs)) creates `_adapter` in the constructor and sets `ItemsSource` in `ApplyProperties`, but:
- When the `DroidListView`'s `RecyclerView` is removed from the window, **nothing** sets `_adapter.ItemsSource = null`.
- There's no override of any cleanup/detach method.
- The `_adapter` field keeps the managed adapter alive, preventing `WeakEventHandler` from cleaning up.

### Issue 5: `DroidNativeComponent` Doesn't Signal Detachment to `VxComponent`

**Severity: High**

`DroidNativeComponent` ([DroidNativeComponent.cs](Vx.Droid/DroidNativeComponent.cs)) extends `FrameLayout` but does not override `OnDetachedFromWindow()`. When Android removes this view from the hierarchy:

- The `VxComponent` is never told it's no longer displayed.
- `VxComponent.Detach()` is never called.
- All state/property/collection subscriptions remain active.
- The component continues to `MarkDirty()` and attempt `RenderActual()` even after removal.

This means **any `VxComponent`** whose subscribed data changes after the view is removed will attempt to re-render into a dead view tree.

### Issue 6: `MarkDirty()` Dispatches to UI Thread Without Disposal Guard

**Severity: Medium**

In [VxComponent.cs, line 399](Vx/Views/VxComponent.cs#L399):
```csharp
protected void MarkDirty()
{
    lock (this)
    {
        if (_dirty) return;
        _dirty = true;
    }
    PortableDispatcher.GetCurrentDispatcher().Run(RenderActual);
}
```

`PortableDispatcher.Run()` posts `RenderActual` to the UI thread's message queue. If the view is disposed between when `MarkDirty()` is called and when `RenderActual` executes, the render will operate on disposed native views. The `try/catch (ObjectDisposedException)` in `RenderActual` handles this reactively, but:
- The `AndroidDispatcher.RunAsync` ([AndroidDispatcher.cs](Vx.Droid/App/AndroidDispatcher.cs)) calls `Post()` which silently swallows exceptions in the fallback path (`catch { }`), potentially hiding other issues.
- There's no `_isDetached` flag to short-circuit re-render attempts.

### Issue 7: iOS `iOSListView` Has a Parallel Issue with Strong Event References

**Severity: Medium**

In [iOSListView.cs, lines 56-62](Vx.iOS/Views/iOSListView.cs#L56), the `CollectionChanged` subscription uses a **strong direct delegate**:
```csharp
_prevList.CollectionChanged += Items_CollectionChanged;
```
This is never cleaned up when the view is removed. On iOS this is less likely to crash (no JNI peer disposal), but it **leaks the `iOSListView`** — the collection holds a strong reference to the view, preventing GC until the collection is replaced or GC'd.

### Issue 8: `NativeView.HandleInnerComponent` Doesn't Propagate Detachment

**Severity: Medium**

When a `VxComponent` is reconciled in `NativeView.Apply()` ([NativeView.cs, line 72](Vx/Core/NativeView.cs#L72)), `HandleInnerComponent` copies properties from the new virtual component to the `_originalComponent` and calls `RenderOnDemand()`. However:
- When the component is **removed** (parent clears children), the `_originalComponent` is never notified.
- The `_originalComponent` continues its subscriptions until GC.

### Issue 9: `ReconcileList` Doesn't Notify Removed Views of Disposal

**Severity: Medium**

`NativeView.ReconcileList` ([NativeView.cs, line 106](Vx/Core/NativeView.cs#L106)) manages inserting/removing/replacing child views. When views are removed via `remove(i)` or `clear()`:
- On Android, this calls `viewGroup.RemoveViewAt(i)` or `viewGroup.RemoveAllViews()`.
- This removes the native Android view, but **none of the managed Vx objects** (`NativeView`, `VxComponent`, `DroidView`) are notified.
- Any event subscriptions within those removed views remain active.

---

## Summary: Chain of Events Leading to the Crash

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. DroidListView created with VxDroidRecyclerAdapter            │
│    └─ Adapter subscribes to collection.CollectionChanged        │
│       via WeakEventHandler                                      │
├─────────────────────────────────────────────────────────────────┤
│ 2. User navigates away                                          │
│    └─ RecyclerView removed from view hierarchy                  │
│    └─ Android GCs the Java peer of the adapter                  │
│    └─ Managed C# adapter object is still alive (not GC'd)      │
│    └─ NO cleanup happens:                                       │
│       • No OnDetachedFromRecyclerView override                  │
│       • No IDisposable.Dispose                                  │
│       • No Detach call                                          │
│       • ItemsSource not set to null                             │
├─────────────────────────────────────────────────────────────────┤
│ 3. Data layer updates the collection (add/remove items)         │
│    └─ CollectionChanged fires                                   │
│    └─ WeakEventHandler.Handler invoked                          │
│    └─ Managed target is alive → proceeds with Invoke            │
│    └─ ItemsSource_CollectionChanged calls                       │
│       NotifyItemRangeInserted                                   │
│    └─ JNI call on disposed Java peer                            │
│    └─ ObjectDisposedException !!                                │
└─────────────────────────────────────────────────────────────────┘
```

---

## Recommended Fixes

### Fix 1 (Critical): Add `OnDetachedFromRecyclerView` to `ObservableRecyclerViewAdapter`

```csharp
// In ObservableRecyclerViewAdapter.cs
public override void OnDetachedFromRecyclerView(RecyclerView recyclerView)
{
    base.OnDetachedFromRecyclerView(recyclerView);

    // Clear subscription to prevent disposed-object access
    if (_itemsSource is INotifyCollectionChanged oldNotify)
    {
        oldNotify.CollectionChanged -= _collectionChangedHandler;
    }
    _collectionChangedHandler = null;
    _itemsSource = null;
}
```

### Fix 2 (Critical): Add `OnDetachedFromWindow` to `DroidNativeComponent`

```csharp
// In DroidNativeComponent.cs
protected override void OnDetachedFromWindow()
{
    base.OnDetachedFromWindow();
    // Signal to the VxComponent that it should clean up
    Component?.Detach();
}

// May also need re-attach handling:
protected override void OnAttachedToWindow()
{
    base.OnAttachedToWindow();
    // Re-subscribe if needed (for RecyclerView recycling scenarios)
}
```

Note: This needs careful handling for RecyclerView recycling where views are temporarily detached and reattached. A simple boolean flag `_permanentlyDetached` or a delayed detach could help distinguish recycling from permanent removal.

### Fix 3 (High): Make `VxComponent.Detach()` Proactive

Change `Detach()` from `protected virtual` to `public` and call it when the component's native container is removed. Add an `_isDetached` flag to guard against post-detach operations:

```csharp
private bool _isDetached;

protected void MarkDirty()
{
    lock (this)
    {
        if (_dirty || _isDetached) return;
        _dirty = true;
    }
    PortableDispatcher.GetCurrentDispatcher().Run(RenderActual);
}

private void RenderActual()
{
    if (_isDetached) return;
    // ... existing render logic
}

public virtual void Detach()
{
    _isDetached = true;
    UnsubscribeFromStates();
    UnsubscribeFromProperties();
    UnsubscribeFromCollections();
}
```

### Fix 4 (High): Guard `ItemsSource_CollectionChanged` in `ObservableRecyclerViewAdapter`

Add a disposed/detached check before calling `Notify*` methods:

```csharp
private bool _isDetached;

private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
{
    if (_isDetached) return;

    try
    {
        // ... existing switch logic
    }
    catch (ObjectDisposedException)
    {
        _isDetached = true;
        // Clean up subscription
        if (sender is INotifyCollectionChanged notifyCollection)
        {
            notifyCollection.CollectionChanged -= _collectionChangedHandler;
        }
    }
}
```

### Fix 5 (Medium): Add Cleanup to `DroidListView`

Override or hook into the view removal lifecycle to clear the adapter's data source:

```csharp
// Possible approaches:
// A) Override OnDetachedFromWindow on the RecyclerView wrapper
// B) Clear adapter when new properties assign null items
// C) Set adapter.ItemsSource = null in a cleanup callback
```

### Fix 6 (Medium): Fix iOS `iOSListView` Event Leak

Use a `WeakEventHandler` or implement cleanup:

```csharp
// Either switch to weak:
_prevList.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(
    Items_CollectionChanged).Handler;

// Or implement cleanup when the view is removed
```

### Fix 7 (Lower Priority): Introduce a Formal `IVxDisposable` Lifecycle

Create a disposal interface that the framework calls when views are permanently removed:

```csharp
public interface IVxLifecycle
{
    void OnAttached();
    void OnDetached();
}
```

Have `ReconcileList`'s `remove` and `clear` callbacks trigger `OnDetached()` on the removed `NativeView` objects, which propagate cleanup to their `VxComponent` children.

---

## Risk Assessment of Fixes

| Fix | Risk | Impact |
|-----|------|--------|
| Fix 1: `OnDetachedFromRecyclerView` | Low — clear API contract | **Eliminates the reported crash** |
| Fix 2: `DroidNativeComponent.OnDetachedFromWindow` | Medium — must handle RecyclerView recycling | Prevents all VxComponent post-disposal activity |
| Fix 3: `_isDetached` guard in VxComponent | Low — additive check | Prevents all post-detach renders |
| Fix 4: Guard in `ItemsSource_CollectionChanged` | Low — defensive coding | Belt-and-suspenders for the reported crash |
| Fix 5: DroidListView cleanup | Low | Ensures adapter doesn't hold stale references |
| Fix 6: iOS event leak | Low | Prevents memory leaks on iOS |
| Fix 7: Formal lifecycle | High — architectural change | Long-term stability improvement |

**Recommended approach:** Implement Fixes 1 + 3 + 4 first for immediate stability. Then Fix 2 with careful handling of RecyclerView recycling. Then Fix 7 as a longer-term architectural improvement.
