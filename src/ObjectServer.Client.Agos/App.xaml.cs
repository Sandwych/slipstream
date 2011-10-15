using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using ObjectServer.Client.Agos.Controls;

namespace ObjectServer.Client.Agos
{


    /// <summary>
    /// Main <see cref="Application"/> class.
    /// </summary>
    public partial class App : Application
    {
        private readonly static object globalBusyLock = new object();

        private BusyIndicator busyIndicator;
        private IObjectServerClient clientService;

        /// <summary>
        /// Creates a new <see cref="App"/> instance.
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // This will enable you to bind controls in XAML files to WebContext.Current
            // properties
            // This will automatically authenticate a user when using windows authentication
            // or when the user chose "Keep me signed in" on a previous login attempt
            //WebContext.Current.Authentication.LoadUser(this.Application_UserLoaded, null);

            // Show some UI to the user while LoadUser is in progress
            this.InitializeRootVisual();
            this.PrepareToLogin();
        }

        private CompositionContainer container;
        internal CompositionContainer Container
        {
            get
            {
                //TODO 多线程处理
                if (container == null)
                {
                    var catalog1 = new AssemblyCatalog(typeof(App).Assembly);
                    var catalog2 = new AssemblyCatalog(typeof(ObjectServerClient).Assembly);
                    var catalog = new AggregateCatalog(catalog1, catalog2);

                    this.container = new CompositionContainer(catalog);

                    this.container.ComposeParts();
                }

                return this.container;
            }
        }

        public bool IsBusy
        {
            get
            {
                return this.busyIndicator.IsBusy;
            }
            set
            {
                if (value && this.busyIndicator.IsBusy)
                {
                    return;
                }

                lock (globalBusyLock)
                {
                    this.busyIndicator.IsBusy = value;
                }
            }
        }

        public MainPage MainPage
        {
            get { return (MainPage)this.busyIndicator.Content; }
        }

        public IObjectServerClient ClientService
        {
            get
            {
                return this.clientService;
            }
            set
            {
                Debug.Assert(value != null);
                this.clientService = value;
            }
        }

        public void PrepareToLogin()
        {
            this.clientService = null;
            this.MainPage.NavigateToLoginPage();
        }

        /// <summary>
        /// why it is not work
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="callback"></param>
        public void DoBusyWork(string msg, Action callback)
        {
            try
            {
                this.IsBusy = true;

                callback.Execute();
            }
            catch
            {
                throw;
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Initializes the <see cref="Application.RootVisual"/> property. The
        /// initial UI will be displayed before the LoadUser operation has completed
        /// (The LoadUser operation will cause user to be logged automatically if
        /// using windows authentication or if the user had selected the "keep
        /// me signed in" option on a previous login).
        /// </summary>
        protected virtual void InitializeRootVisual()
        {
            this.busyIndicator = new BusyIndicator();
            this.busyIndicator.Content = this.Container.GetExportedValue<MainPage>();
            this.busyIndicator.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            this.busyIndicator.VerticalContentAlignment = VerticalAlignment.Stretch;

            this.RootVisual = this.busyIndicator;

            this.busyIndicator.BusyContent = "正在处理，请稍候...";
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // If the app is running outside of the debugger then report the exception using
            // a ChildWindow control.
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                // NOTE: This will allow the application to continue running after an exception has been thrown
                // but not handled. 
                // For production applications this error handling should be replaced with something that will 
                // report the error to the website and stop the application.
                e.Handled = true;
                ErrorWindow.CreateNew(e.ExceptionObject);
            }
        }
    }
}