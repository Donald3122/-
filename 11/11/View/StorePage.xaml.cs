using _11.Models;
using _11.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace _11
{
    public sealed partial class StorePage : Page
    {
        public List<Product> Products { get; set; }
        public List<CartProduct> Products1 { get; set; }
        public StoreViewModel viewModel { get; set; }

        public StorePage()
        {
            this.InitializeComponent();
            _ = LoadProductsAsync();
                
        }
        public async void RefreshPage()
        {
            await LoadProductsAsync(); // Метод для загрузки продуктов
        }
        private async Task LoadProductsAsync()
        {
            Products = await DatabaseHelper.GetAllProducts();
            ProductGrid.ItemsSource = Products;
            Products1 = await DatabaseHelper.GetCartProducts();


        }

        private async void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var selectedProduct = (Product)ProductGrid.SelectedItem;
            if (selectedProduct != null)
            {
                // Добавляем выбранный продукт в корзину
                await DatabaseHelper.AddToCart(selectedProduct.Id);
                // Выводим сообщение для подтверждения
                var dialog = new ContentDialog
                {
                    Title = "Продукт добавлен",
                    Content = $"{selectedProduct.Name} добавлен в корзину.",
                    CloseButtonText = "ОК"
                };
                await dialog.ShowAsync();
            }
        }
    }
}
