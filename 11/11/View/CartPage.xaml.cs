using System;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using _11.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace _11
{
    // Страница корзины покупок
    public sealed partial class CartPage : Page
    {
        // Коллекция продуктов в корзине
        public List<CartProduct> CartProducts { get; set; }

        // Общее количество продуктов в корзине
        private int _totalProducts;
        public int TotalProducts
        {
            get => _totalProducts;
            set
            {
                _totalProducts = value;
                if (TotalProductsText != null) // Проверка на null для избежания ошибок
                {
                    TotalProductsText.Text = $"Количество продуктов: {TotalProducts}";
                }
            }
        }

        // Общая стоимость продуктов в корзине
        private decimal _totalCost;
        public decimal TotalCost
        {
            get => _totalCost;
            set
            {
                _totalCost = value;
                if (TotalCostText != null) // Проверка на null для избежания ошибок
                {
                    TotalCostText.Text = $"Общая стоимость: {TotalCost:C}";
                }
            }
        }

        // Конструктор страницы, инициализирует компоненты и загружает данные
        public CartPage()
        {
            this.InitializeComponent();
            _ = LoadProductsAsync(); // Асинхронно загружаем продукты в корзине
        }

        // Асинхронно загружает продукты в корзине и обновляет счетчики
        public async Task LoadProductsAsync()
        {
            CartProducts = await DatabaseHelper.GetCartProducts(); // Получаем продукты из базы данных
            ProductListView.ItemsSource = CartProducts; // Устанавливаем источник данных для списка продуктов

            TotalProducts = await DatabaseHelper.Quantity(); // Получаем общее количество продуктов в корзине
            TotalCost = await DatabaseHelper.TotalCost(); // Получаем общую стоимость продуктов в корзине
        }

        // Обработчик события удаления продукта из корзины
        private async void RemovCart_Click(object sender, RoutedEventArgs e)
        {
            var selectedProduct = (CartProduct)ProductListView.SelectedItem; // Выбранный продукт для удаления
            if (selectedProduct != null)
            {
                // Удаляем выбранный продукт из корзины
                await DatabaseHelper.RemoveFromCart(selectedProduct.Id);

                // Выводим диалоговое окно для подтверждения удаления
                var dialog = new ContentDialog
                {
                    Title = "Продукт удален",
                    Content = $"{selectedProduct.Name} удален из корзины.",
                    CloseButtonText = "ОК"
                };
                await dialog.ShowAsync();

                // Обновляем список продуктов и счетчики
                await LoadProductsAsync();
            }
        }
    }
}
