using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Helpers
{
    public static class ReconcileDictionary
    {
        public static void Reconcile<TValue>(
            Dictionary<string, TValue> existing,
            Dictionary<string, TValue> desired,
            out HashSet<string> toRemove,
            out Dictionary<string, TValue> toUpdate,
            out Dictionary<string, TValue> toAdd)
        {
            toRemove = [];
            toUpdate = [];
            toAdd = [];

            // Remove items that are no longer present
            foreach (var key in existing.Keys)
            {
                if (!desired.ContainsKey(key))
                {
                    toRemove.Add(key);
                }
            }

            // Update existing items and add new items
            foreach (var kvp in desired)
            {
                if (existing.TryGetValue(kvp.Key, out var existingValue))
                {
                    if (!object.Equals(kvp.Value, existingValue))
                    {
                        toUpdate[kvp.Key] = kvp.Value;
                    }
                }
                else
                {
                    toAdd[kvp.Key] = kvp.Value;
                }
            }
        }
    }
}
