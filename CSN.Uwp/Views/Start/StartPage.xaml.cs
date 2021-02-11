namespace CSN.Uwp.Views.Start
{
    using CSN.Common.Extensions;
    using CSN.Uwp.Core;
    using Microsoft.Toolkit.Uwp.UI.Animations;
    using System;
    using System.Numerics;
    using Windows.UI.Composition;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Navigation;

    /// <summary>
    /// Start page
    /// </summary>
    public sealed partial class StartPage : CorePage
    {
        #region Constructor

        public StartPage()
        {
            InitializeComponent();
            DataContext = (StartPageViewModel)App.Current.Services.GetService(typeof(StartPageViewModel));
            LogoViewBox.SizeChanged += OnLogoViewBoxSizeChanged;
        }

        private void OnLogoViewBoxSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.LogoViewBox.SizeChanged -= OnLogoViewBoxSizeChanged;
            InitializeCompositionAnimation();
        }

        #endregion

        #region Properties

        public override CorePageViewModel ViewModel 
        { 
            get => (StartPageViewModel)DataContext; 
            set => DataContext = value; 
        }

        #endregion

        #region Private methods

        private void InitializeCompositionAnimation()
        {
            var scaleFactor = 1.6f;
            var animationSet = new AnimationSet(this.LogoViewBox);
            var visual = animationSet.Visual;
            visual.CenterPoint = new Vector3(LogoViewBox.ActualSize.X / 2, LogoViewBox.ActualSize.Y / 2, 0);
            var scaleVector = new Vector3(scaleFactor, scaleFactor, 1.0f);

            var compositor = visual.Compositor;

            if (compositor != null)
            {
                var animation = compositor.CreateVector3KeyFrameAnimation();
                animation.Duration = TimeSpan.FromMilliseconds(7000);
                animation.DelayTime = TimeSpan.FromMilliseconds(0);
                animation.IterationBehavior = AnimationIterationBehavior.Forever;
                animation.InsertKeyFrame(0.5f, scaleVector);
                animation.InsertKeyFrame(1f, new Vector3(1f, 1f, 1.0f));

                animationSet.AddCompositionAnimation("Scale", animation);
                animationSet.StartAsync().NotAwaited();
            }
        }

        //protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        //{
        //    base.OnNavigatingFrom(e);
        //}

        #endregion
    }
}
