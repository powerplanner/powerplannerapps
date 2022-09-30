using System;
using BareMvvm.Core.ViewModels;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.VxTests
{
    public class VxScrollViewerTestViewModel : PopupComponentViewModel
    {
        public VxScrollViewerTestViewModel(BaseViewModel parent) : base(parent)
        {
        }

        protected override View Render()
        {
            return new ScrollView
            {
                Content = new LinearLayout
                {
                    Margin = new Thickness(NookInsets.Left, 0, NookInsets.Right, NookInsets.Bottom),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Lorem ipsum dolor sit amet. Qui autem laudantium quo eligendi temporibus qui asperiores dolore aut odit natus ea quaerat quia! Et alias veritatis et recusandae error qui aspernatur enim.\n\nQui quasi voluptatem ea nisi inventore et itaque eveniet eos voluptatum quia sit animi odio aut voluptatem perspiciatis. Eum aperiam blanditiis est soluta omnis ad rerum minus est quae voluptatem qui repellendus nostrum! Qui suscipit sunt est provident recusandae sit accusamus praesentium non possimus fugit aut minus quia 33 ratione modi?\n\nAut dolores inventore ab autem voluptates aut corrupti provident ut temporibus veritatis. Ut corrupti totam id fugit autem cum voluptatibus molestias aut animi numquam ut aperiam rerum! Sit quod quas ut fuga pariatur qui nihil harum ab sint impedit.\n\nLorem ipsum dolor sit amet. Qui autem laudantium quo eligendi temporibus qui asperiores dolore aut odit natus ea quaerat quia! Et alias veritatis et recusandae error qui aspernatur enim.\n\nQui quasi voluptatem ea nisi inventore et itaque eveniet eos voluptatum quia sit animi odio aut voluptatem perspiciatis. Eum aperiam blanditiis est soluta omnis ad rerum minus est quae voluptatem qui repellendus nostrum! Qui suscipit sunt est provident recusandae sit accusamus praesentium non possimus fugit aut minus quia 33 ratione modi?\n\nAut dolores inventore ab autem voluptates aut corrupti provident ut temporibus veritatis. Ut corrupti totam id fugit autem cum voluptatibus molestias aut animi numquam ut aperiam rerum! Sit quod quas ut fuga pariatur qui nihil harum ab sint impedit.",
                            Margin = new Thickness(Theme.Current.PageMargin)
                        }
                    }
                }
            };
        }
    }
}

