using Microsoft.QueryStringDotNET;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Helpers
{
    public enum LaunchSurface
    {
        // Order matters
        Normal,
        SecondaryTile,
        Calendar,
        Toast,
        Uri,
        JumpList,
        PrimaryTile
    }

    public static class ArgumentsHelper
    {
        internal const string KEY_ACTION = "action";
        internal const string KEY_LAUNCH_SURFACE = "launchSurface";

        public static BaseArguments Parse(string queryString)
        {
            QueryString qs = QueryString.Parse(queryString);

            string val;
            ArgumentsAction action = ArgumentsAction.Unknown;

            if (!(qs.TryGetValue(KEY_ACTION, out val) && Enum.TryParse(val, out action)))
                return null;

            BaseArguments answer = null;

            switch (action)
            {
                case ArgumentsAction.ViewPage:
                    answer = new ViewPageArguments();
                    break;

                case ArgumentsAction.ViewSchedule:
                    answer = new ViewScheduleArguments();
                    break;

                case ArgumentsAction.ViewClass:
                    answer = new ViewClassArguments();
                    break;

                case ArgumentsAction.QuickAdd:
                    answer = new QuickAddArguments();
                    break;

                case ArgumentsAction.QuickAddToCurrentAccount:
                    answer = new QuickAddToCurrentAccountArguments();
                    break;

                case ArgumentsAction.QuickAddHomeworkToCurrentAccount:
                    answer = new QuickAddHomeworkToCurrentAccountArguments();
                    break;

                case ArgumentsAction.QuickAddExamToCurrentAccount:
                    answer = new QuickAddExamToCurrentAccountArguments();
                    break;

                case ArgumentsAction.OpenAccount:
                    answer = new OpenAccountArguments();
                    break;

                case ArgumentsAction.ViewHomework:
                    answer = new ViewHomeworkArguments();
                    break;

                case ArgumentsAction.ViewExam:
                    answer = new ViewExamArguments();
                    break;

                case ArgumentsAction.ViewHoliday:
                    answer = new ViewHolidayArguments();
                    break;
            }

            if (answer != null)
            {
                if (answer.TryParse(qs))
                    return answer;
            }

            return null;
        }


        public static BaseArguments CreateArgumentsForView(DataItemMegaItem item, Guid localAccountId, LaunchSurface launchSurface = LaunchSurface.Normal)
        {
            BaseArgumentsWithAccountAndItem args;

            if (item.MegaItemType == PowerPlannerSending.MegaItemType.Homework)
                args = new ViewHomeworkArguments();

            else if (item.MegaItemType == PowerPlannerSending.MegaItemType.Exam)
                args = new ViewExamArguments();

            else
                return new OpenAccountArguments()
                {
                    LocalAccountId = localAccountId
                };

            args.LocalAccountId = localAccountId;
            args.ItemId = item.Identifier;
            args.LaunchSurface = launchSurface;

            return args;
        }

        public static BaseArguments CreateArgumentsForView(ViewItemTaskOrEvent item, Guid localAccountId)
        {
            BaseArgumentsWithAccountAndItem args;

            if (item.Type == TaskOrEventType.Task)
                args = new ViewHomeworkArguments();

            else if (item.Type == TaskOrEventType.Event)
                args = new ViewExamArguments();

            else
                return new OpenAccountArguments()
                {
                    LocalAccountId = localAccountId
                };

            args.LocalAccountId = localAccountId;
            args.ItemId = item.Identifier;

            return args;
        }
    }

    public abstract class BaseArguments
    {
        internal BaseArguments() { }

        public string SerializeToString()
        {
            QueryString qs = new QueryString();

            qs.Set(ArgumentsHelper.KEY_ACTION, Action.ToString());

            if (LaunchSurface != LaunchSurface.Normal)
            {
                qs.Set(ArgumentsHelper.KEY_LAUNCH_SURFACE, ((int)LaunchSurface).ToString());
            }

            InjectValues(qs);

            return qs.ToString();
        }

        protected virtual void InjectValues(QueryString qs) { }

        internal abstract ArgumentsAction Action { get; }

        public LaunchSurface LaunchSurface { get; set; } = LaunchSurface.Normal;

        internal virtual bool TryParse(QueryString qs)
        {
            string str = null;
            if (qs.TryGetValue(ArgumentsHelper.KEY_LAUNCH_SURFACE, out str))
            {
                LaunchSurface launchSurface;
                if (Enum.TryParse<LaunchSurface>(str, out launchSurface))
                {
                    LaunchSurface = launchSurface;
                }
            }
            return true;
        }
    }

    public abstract class BaseArgumentsWithAccount : BaseArguments
    {
        public const string KEY_ACCOUNT = "account";

        public Guid LocalAccountId { get; set; }

        protected override void InjectValues(QueryString qs)
        {
            qs.Add(KEY_ACCOUNT, LocalAccountId.ToString());
        }

        internal override bool TryParse(QueryString qs)
        {
            string val;
            Guid id;

            if (!(qs.TryGetValue(KEY_ACCOUNT, out val) && Guid.TryParse(val, out id)))
                return false;

            LocalAccountId = id;

            return base.TryParse(qs);
        }
    }

    public class OpenAccountArguments : BaseArgumentsWithAccount
    {
        internal override ArgumentsAction Action
        {
            get
            {
                return ArgumentsAction.OpenAccount;
            }
        }
    }

    public abstract class BaseArgumentsWithAccountAndItem : BaseArgumentsWithAccount
    {
        public const string KEY_ITEM = "item";

        public Guid ItemId { get; set; }

        protected override void InjectValues(QueryString qs)
        {
            qs.Add(KEY_ITEM, ItemId.ToString());

            base.InjectValues(qs);
        }

        internal override bool TryParse(QueryString qs)
        {
            string val;
            Guid id;

            if (!(qs.TryGetValue(KEY_ITEM, out val) && Guid.TryParse(val, out id)))
                return false;

            ItemId = id;

            return base.TryParse(qs);
        }
    }

    public class ViewPageArguments : BaseArgumentsWithAccount
    {
        private const string KEY_PAGE = "page";

        public enum Pages
        {
            Agenda
        }

        internal override ArgumentsAction Action
        {
            get
            {
                return ArgumentsAction.ViewPage;
            }
        }

        public Pages Page { get; set; }

        protected override void InjectValues(QueryString qs)
        {
            qs.Add(KEY_PAGE, Page.ToString());

            base.InjectValues(qs);
        }

        internal override bool TryParse(QueryString qs)
        {
            string pageString;
            if (qs.TryGetValue(KEY_PAGE, out pageString))
            {
                Pages pageValue;
                if (Enum.TryParse<Pages>(pageString, out pageValue))
                {
                    Page = pageValue;
                    return base.TryParse(qs);
                }
            }

            return false;
        }
    }

    public class ViewScheduleArguments : BaseArgumentsWithAccount
    {
        internal override ArgumentsAction Action
        {
            get
            {
                return ArgumentsAction.ViewSchedule;
            }
        }
    }

    public class ViewClassArguments : BaseArgumentsWithAccountAndItem
    {
        internal override ArgumentsAction Action
        {
            get
            {
                return ArgumentsAction.ViewClass;
            }
        }
    }

    public class ViewHomeworkArguments : BaseArgumentsWithAccountAndItem
    {
        internal override ArgumentsAction Action
        {
            get
            {
                return ArgumentsAction.ViewHomework;
            }
        }
    }

    public class ViewExamArguments : BaseArgumentsWithAccountAndItem
    {
        internal override ArgumentsAction Action
        {
            get
            {
                return ArgumentsAction.ViewExam;
            }
        }
    }

    public class ViewHolidayArguments : BaseArgumentsWithAccountAndItem
    {
        internal override ArgumentsAction Action
        {
            get
            {
                return ArgumentsAction.ViewHoliday;
            }
        }
    }

    public class QuickAddArguments : BaseArgumentsWithAccount
    {
        internal override ArgumentsAction Action
        {
            get
            {
                return ArgumentsAction.QuickAdd;
            }
        }
    }

    public class QuickAddToCurrentAccountArguments : BaseArguments
    {
        internal override ArgumentsAction Action
        {
            get
            {
                return ArgumentsAction.QuickAddToCurrentAccount;
            }
        }
    }

    public class QuickAddHomeworkToCurrentAccountArguments : BaseArguments
    {
        internal override ArgumentsAction Action
        {
            get
            {
                return ArgumentsAction.QuickAddHomeworkToCurrentAccount;
            }
        }
    }

    public class QuickAddExamToCurrentAccountArguments : BaseArguments
    {
        internal override ArgumentsAction Action
        {
            get
            {
                return ArgumentsAction.QuickAddExamToCurrentAccount;
            }
        }
    }

    internal enum ArgumentsAction
    {
        Unknown,
        ViewSchedule,
        ViewClass,
        QuickAdd,
        QuickAddHomeworkToCurrentAccount,
        QuickAddExamToCurrentAccount,
        ViewHomework,
        ViewExam,
        OpenAccount,
        ViewHoliday,
        ViewPage,
        QuickAddToCurrentAccount
    }
}
