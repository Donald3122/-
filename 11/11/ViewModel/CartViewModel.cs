using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using _11.Models;

namespace _11.ViewModels
{
    public class CartViewModel:ViewModelBase
    {
        private List<CartProduct> _cartproducts;

        public List<CartProduct> CartProducts
        {
            get { return _cartproducts; }
            set
            {
                _cartproducts = value;
                OnPropertyChanged();
            }
        }

        public async Task LoadProductsAsync()
        {
            CartProducts = new List<CartProduct>(await DatabaseHelper.GetCartProducts());
        }
    }
}
