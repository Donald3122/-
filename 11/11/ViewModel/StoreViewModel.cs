using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _11.Models;
namespace _11.ViewModels
{
    public class StoreViewModel : ViewModelBase 
    {
        private List<Product> _products;

        public List<Product> Products
        {
            get { return _products; }
            set
            {
                _products = value;
                OnPropertyChanged(); 
            }
        }

        public async Task LoadProductsAsync()
        {
            Products = new List<Product>(await DatabaseHelper.GetAllProducts());
        }
    }
}
