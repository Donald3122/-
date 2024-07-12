using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using _11.Models;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Popups;

namespace _11.Models
{
    public static class DatabaseHelper
    {
        private static readonly string DbPath = "products.db";

        public static async Task<bool> InitializeDatabase()
        {
            bool databaseExists = File.Exists(DbPath);
            bool cartTableExists = false;

            using (var connection = new SqliteConnection($"Filename={DbPath}"))
            {
                await connection.OpenAsync();

            }

            return cartTableExists;
        }
        public static async Task AddToCart(int productId)
        {
            using (var connection = new SqliteConnection($"Filename={DbPath}"))
            {
                await connection.OpenAsync();

                // Проверка, есть ли продукт уже в корзине
                var checkCartCommand = connection.CreateCommand();
                checkCartCommand.CommandText = "SELECT Quantity FROM Cart1 WHERE Id = @ProductId";
                checkCartCommand.Parameters.AddWithValue("@ProductId", productId);
                var quantity = (long?)await checkCartCommand.ExecuteScalarAsync();

                if (quantity.HasValue)
                {
                    // Если продукт уже есть в корзине, увеличиваем количество
                    var updateCartCommand = connection.CreateCommand();
                    updateCartCommand.CommandText = "UPDATE Cart1 SET Quantity = Quantity + 1 WHERE Id = @ProductId";
                    updateCartCommand.Parameters.AddWithValue("@ProductId", productId);
                    await updateCartCommand.ExecuteNonQueryAsync();
                }
                else
                {
                    // Если продукта нет в корзине, добавляем его
                    var product = await GetProductById(productId); 

                    if (product != null)
                    {
                        var insertCartCommand = connection.CreateCommand();
                        insertCartCommand.CommandText = @"
                    INSERT INTO Cart1 (Id, Name, Price, ImagePath, Quantity)
                    VALUES (@ProductId, @Name, @Price, @ImagePath, 1)";
                        insertCartCommand.Parameters.AddWithValue("@ProductId", product.Id);
                        insertCartCommand.Parameters.AddWithValue("@Name", product.Name);
                        insertCartCommand.Parameters.AddWithValue("@Price", product.Price);
                        insertCartCommand.Parameters.AddWithValue("@ImagePath", product.ImagePath);
                        await insertCartCommand.ExecuteNonQueryAsync();
                    }
                }
            }
        }
        public static async Task<List<CartProduct>> GetCartProducts()
        {
            
            List<CartProduct> cartProducts = new List<CartProduct>();

            using (var connection = new SqliteConnection($"Filename={DbPath}"))
            {
                await connection.OpenAsync();
                Global.a=0;
                Global.b = 0;
                var selectCommand = connection.CreateCommand();
                selectCommand.CommandText = @"
                SELECT Id, Name, Price, ImagePath, Quantity
                FROM Cart1";

                var reader = await selectCommand.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    CartProduct product = new CartProduct
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Price = reader.GetDecimal(2),
                        ImagePath = reader.GetString(3),
                        Quantity = reader.GetInt32(4)
                    };
                    Global.a += product.Price*product.Quantity;
                    Global.b += product.Quantity;
                    cartProducts.Add(product);
                    
                }
            }
            
            return cartProducts;
        }
        

        public static async Task<Product> GetProductById(int productId)
        {
            Product product = null;

            using (var connection = new SqliteConnection($"Filename={DbPath}"))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Name, Price, ImagePath FROM Products WHERE Id = @ProductId";
                command.Parameters.AddWithValue("@ProductId", productId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        product = new Product
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Price = reader.GetDecimal(2),
                            ImagePath = reader.GetString(3)
                        };
                    }
                }
            }

            return product;
        }

        public static async Task RemoveFromCart(int productId)
        {
            using (var connection = new SqliteConnection($"Filename={DbPath}"))
            {
                await connection.OpenAsync();

                // Проверка, есть ли продукт уже в корзине
                var checkCartCommand = connection.CreateCommand();
                checkCartCommand.CommandText = "SELECT Quantity FROM Cart1 WHERE Id = @ProductId";
                checkCartCommand.Parameters.AddWithValue("@ProductId", productId);
                var quantity = (long?)await checkCartCommand.ExecuteScalarAsync();

                if (quantity.HasValue)
                {
                    if (quantity.Value > 1)
                    {
                        // Если продукт в корзине и количество больше 1, уменьшаем количество
                        var updateCartCommand = connection.CreateCommand();
                        updateCartCommand.CommandText = "UPDATE Cart1 SET Quantity = Quantity - 1 WHERE Id = @ProductId";
                        updateCartCommand.Parameters.AddWithValue("@ProductId", productId);
                        await updateCartCommand.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        // Если продукт в корзине и количество равно 1, удаляем продукт из корзины
                        var deleteCartCommand = connection.CreateCommand();
                        deleteCartCommand.CommandText = "DELETE FROM Cart1 WHERE Id = @ProductId";
                        deleteCartCommand.Parameters.AddWithValue("@ProductId", productId);
                        await deleteCartCommand.ExecuteNonQueryAsync();
                    }
                }
            }
        }
       
        


        public static async Task<List<Product>> GetAllProducts()
        {
            var products = new List<Product>();

            using (var connection = new SqliteConnection($"Filename={DbPath}"))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Name, Price, ImagePath FROM Products";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var product = new Product
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Price = reader.GetDecimal(2),
                            ImagePath = reader.GetString(3)
                        };

                        products.Add(product);
                    }
                }
            }
            
            return products;
        }

    }
}


