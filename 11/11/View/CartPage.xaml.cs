using System;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using _11.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace _11
{
    public sealed partial class CartPage : Page
    {
        public List<CartProduct> CartProducts { get; set; }

        private int _totalProducts;
        public int TotalProducts
        {
            get => _totalProducts;
            set
            {
                _totalProducts = value;
                if (TotalProductsText != null) // Проверка на null
                {
                    TotalProductsText.Text = $"Количество продуктов: {TotalProducts}";
                }
            }
        }

        private decimal _totalCost;
        public decimal TotalCost
        {
            get => _totalCost;
            set
            {
                _totalCost = value;
                if (TotalCostText != null) // Проверка на null
                {
                    TotalCostText.Text = $"Общая стоимость: {TotalCost:C}";
                }
            }
        }

        public CartPage()
        {
            this.InitializeComponent();
            _ = LoadProductsAsync();
        }

        public async Task LoadProductsAsync()
        {
            CartProducts = await DatabaseHelper.GetCartProducts();
            ProductListView.ItemsSource = CartProducts;

            TotalProducts = await DatabaseHelper.Quantity();
            TotalCost = await DatabaseHelper.TotalCost();
        }

        private async void RemovCart_Click(object sender, RoutedEventArgs e)
        {
            var selectedProduct = (CartProduct)ProductListView.SelectedItem;
            if (selectedProduct != null)
            {
                // Удаляем выбранный продукт из корзины
                await DatabaseHelper.RemoveFromCart(selectedProduct.Id);

                // Выводим сообщение для подтверждения
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
