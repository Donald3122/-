using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace _11
{
    sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.UnhandledException += App_UnhandledException; // Добавление глобального обработчика исключений
        }

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // Логирование исключений
            System.Diagnostics.Debug.WriteLine("Необработанное исключение: " + e.Exception.Message);
            System.Diagnostics.Debug.WriteLine("Стек вызовов: " + e.Exception.StackTrace);
            e.Handled = true;

            // Проверка подключения отладчика
            bool isDebuggerAttached = System.Diagnostics.Debugger.IsAttached;
            System.Diagnostics.Debug.WriteLine("Отладчик подключен: " + isDebuggerAttached);

            if (isDebuggerAttached)
            {
                System.Diagnostics.Debugger.Break();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Отладчик не подключен. Исключение не может быть обработано корректно.");
            }
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = false;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Загрузить состояние из ранее приостановленного приложения
                }

                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }

            Window.Current.Activate();
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // Сохранить состояние приложения и остановить все фоновые операции
            deferral.Complete();
        }
    }
}
