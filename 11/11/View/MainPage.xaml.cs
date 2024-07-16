using _11.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace _11
{
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
            InitializeDatabaseAndNavigate();
        }

        private async void InitializeDatabaseAndNavigate()
        {
            try
            {
                await DatabaseHelper.InitializeDatabase();
                ContentFrame.Navigate(typeof(StorePage)); // Начальная страница - StorePage
                NavView.SelectedItem = NavView.MenuItems[0]; // Выбор первого элемента (магазин)
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Ошибка при инициализации базы данных: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Стек вызовов: " + ex.StackTrace);
                if (global::System.Diagnostics.Debugger.IsAttached) global::System.Diagnostics.Debugger.Break();
            }
        }

        private async void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                // Обработка выбора настроек (если нужно)
            }
            else
            {
                NavigationViewItem item = args.SelectedItem as NavigationViewItem;
                if (item != null)
                {
                    switch (item.Tag.ToString())
                    {
                        case "Store":
                            ContentFrame.Navigate(typeof(StorePage));
                            
                            break;
                        case "Cart":
                            ContentFrame.Navigate(typeof(CartPage));


                            break;
                        default:
                            System.Diagnostics.Debug.WriteLine("Неизвестный тег: " + item.Tag.ToString());
                            break;
                    }
                }
            }
        }
    }
}
