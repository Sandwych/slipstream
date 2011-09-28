using System.Windows;
using System.Windows.Controls;

namespace ObjectServer.Client.Agos.Controls
{
    public class TransitionFrame : Frame
    {
        private ContentPresenter CurrentContentPresentationSite;
        private ContentPresenter PreviousContentPresentationSite;

        public TransitionFrame()
        {
            DefaultStyleKey = typeof(TransitionFrame);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PreviousContentPresentationSite = GetTemplateChild("PreviousContentPresentationSite") as ContentPresenter;
            CurrentContentPresentationSite = GetTemplateChild("CurrentContentPresentationSite") as ContentPresenter;

            if (CurrentContentPresentationSite != null)
            {
                CurrentContentPresentationSite.Content = Content;
            }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if ((CurrentContentPresentationSite != null) && (PreviousContentPresentationSite != null))
            {
                CurrentContentPresentationSite.Content = newContent;
                PreviousContentPresentationSite.Content = oldContent;

                VisualStateManager.GoToState(this, "Normal", false);
                VisualStateManager.GoToState(this, "Transition", true);
            }
        }
    }
}