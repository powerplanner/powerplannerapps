using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolsPortable;
using ToolsPortable.Indexing;

namespace PowerPlannerAppDataLibrary.Indexing
{
    public class MegaItemsIndexer : BindableBase
    {
        private readonly MyIndexer<BaseViewItemMegaItem> _indexer = new();

        public MegaItemsIndexer(Guid localAccountId, ViewItemSemester semester)
        {
            Initialize(localAccountId, semester);
        }

        private async void Initialize(Guid localAccountId, ViewItemSemester semester)
        {
        }

        public Task IndexAsync(IEnumerable<BaseViewItemMegaItem> megaItems)
        {
            return Task.Run(delegate
            {
                Index(megaItems);
            });
        }

        private void Index(IEnumerable<BaseViewItemMegaItem> megaItems)
        {
            foreach (var item in megaItems)
            {
                _indexer.Index(item.Name, item, 0);
                _indexer.Index(item.Details, item, 1);

                if (item is ViewItemTaskOrEvent taskOrEvent)
                {
                    _indexer.Index(taskOrEvent.Class.Name, item, 2);
                }
            }
        }

        public string Query { get; private set; }
        public readonly MyObservableList<BaseViewItemMegaItem> Results = new();

        private bool _areResultsLoading;
        public bool AreResultsLoading
        {
            get => _areResultsLoading;
            set => SetProperty(ref _areResultsLoading, value, nameof(AreResultsLoading));
        }

        private long _queryAttempt = 0;

        public async void UpdateQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                Results.Clear();
                AreResultsLoading = false;
            }

            if (Query != query)
            {
                Query = query;
                AreResultsLoading = true;
                var thisQueryAttempt = _queryAttempt + 1;
                _queryAttempt = thisQueryAttempt;

                await Task.Delay(1000);

                if (_queryAttempt == thisQueryAttempt)
                {
                    try
                    {
                        var results = await Task.Run(delegate
                        {
                            return _indexer.GetMatches(query, 50, CancellationToken.None);
                        });

                        if (_queryAttempt == thisQueryAttempt)
                        {
                            Results.MakeListLike(results);
                            AreResultsLoading = false;
                        }
                    }
                    catch { }
                }
            }
        }
    }
}
