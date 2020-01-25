using Plugin.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PowerPlannerAppDataLibrary.Helpers
{
    public static class AbTestHelper
    {
        private static long[] EnabledAccountIds = new long[]
        {
            906267,
            1030231,
            91353
        };

        private static long[] DisabledAccountIds = new long[]
        {
            1725994
        };

        public static bool ShouldIgnoreFromTelemetry()
        {
            long accountId = Extensions.TelemetryExtension.Current?.CurrentAccountId ?? 0;

            if (EnabledAccountIds.Contains(accountId) || DisabledAccountIds.Contains(accountId))
            {
                return true;
            }

            return false;
        }

        public static class Tests
        {
            public static TestItem NewTimePicker { get; set; } = new TestItem(nameof(NewTimePicker), true);
        }

        public static bool IsEnabled(string testName)
        {
            TestItem testItem = (TestItem)typeof(Tests).GetRuntimeProperty(testName).GetValue(null);
            return testItem.Value;
        }

        public class TestItem
        {
            private bool? _debugValue;
            private string _testName;
            public TestItem(string testName, bool? debugValue)
            {
                _debugValue = debugValue;
                _testName = testName;
            }

            public bool Value
            {
                get
                {
#if DEBUG
                    if (_debugValue != null)
                    {
                        return _debugValue.Value;
                    }
#endif

                    if (Extensions.TelemetryExtension.Current != null && Extensions.TelemetryExtension.Current.CurrentAccountId > 0)
                    {
                        if (EnabledAccountIds.Contains(Extensions.TelemetryExtension.Current.CurrentAccountId))
                        {
                            return true;
                        }
                        else if (DisabledAccountIds.Contains(Extensions.TelemetryExtension.Current.CurrentAccountId))
                        {
                            return false;
                        }
                    }

                    int val = CrossSettings.Current.GetValueOrDefault("AbTest." + _testName, 0);

                    if (val == 0)
                    {
                        val = new Random().Next(1, 3); // Upper is exclusive

                        CrossSettings.Current.AddOrUpdateValue("AbTest." + _testName, val);
                    }

                    return val == 2;
                }
            }

            public static implicit operator bool(TestItem testItem) => testItem.Value;
        }
    }
}
