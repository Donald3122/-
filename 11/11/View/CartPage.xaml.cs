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
       public int TotalProducts = Global.b;
       public decimal TotalCost = Global.a;
        public CartPage()
        {
            this.InitializeComponent();
            _ = LoadProductsAsync();
             
        }
        
        private async Task LoadProductsAsync()
        {
            CartProducts = await DatabaseHelper.GetCartProducts();
            ProductListView.ItemsSource = CartProducts;
            TotalProductsText.Text = $"Количество продуктов: {TotalProducts}";
            TotalCostText.Text = $"Общая стоимость ={TotalCost:C}";


        }

        private async void RemovCart_Click(object sender, RoutedEventArgs e)
        {
            var selectedProduct = (CartProduct)ProductListView.SelectedItem;
            if (selectedProduct != null)
            {
                // Удаляем выбраный продукт из корзины
                await DatabaseHelper.RemoveFromCart(selectedProduct.Id);
                // Выводим сообщение для подтверждения
                var dialog = new ContentDialog
                {
                    Title = "Продукт удален",
                    Content = $"{selectedProduct.Name} Удален из корзины.",
                    CloseButtonText = "ОК"
                };
                await dialog.ShowAsync();
            }
             
            _ = LoadProductsAsync();
        }
    }
}
