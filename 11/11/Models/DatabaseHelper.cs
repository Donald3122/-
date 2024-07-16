using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace _11.Models
{
    public static class DatabaseHelper
    {
        private static readonly string DbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "products.db");

        public static async Task<bool> InitializeDatabase()
        {
            bool databaseExists = File.Exists(DbPath);

            using (var connection = new SqliteConnection($"Filename={DbPath}"))
            {
                await connection.OpenAsync();

                // Создание таблиц, если они не существуют
                var createProductTableCmd = connection.CreateCommand();
                createProductTableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Price REAL NOT NULL,
                    ImagePath TEXT
                );";
                await createProductTableCmd.ExecuteNonQueryAsync();

                var createCartTableCmd = connection.CreateCommand();
                createCartTableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Cart1 (
                    Id INTEGER PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Price REAL NOT NULL,
                    ImagePath TEXT,
                    Quantity INTEGER NOT NULL
                );";
                await createCartTableCmd.ExecuteNonQueryAsync();

                // Проверка наличия записей в таблице Products
                var checkProductsTableCmd = connection.CreateCommand();
                checkProductsTableCmd.CommandText = "SELECT COUNT(*) FROM Products";
                var productCount = (long)await checkProductsTableCmd.ExecuteScalarAsync();

                // Если таблица Products пуста, добавить 20 записей
                if (productCount == 0)
                {
                    var products = new List<Product>
                    {
                        new Product { Name = "Product 1", ImagePath = "ms-appdata:///Local/product1.png", Price = 10.99m },
                        new Product { Name = "Product 2", ImagePath = "ms-appdata:///Local/product2.png", Price = 12.99m },
                        new Product { Name = "Product 3", ImagePath = "ms-appdata:///Local/product3.png", Price = 14.99m },
                        new Product { Name = "Product 4", ImagePath = "ms-appdata:///Local/product4.png", Price = 16.99m },
                        new Product { Name = "Product 5", ImagePath = "ms-appdata:///Local/product5.png", Price = 18.99m },
                        new Product { Name = "Product 6", ImagePath = "ms-appdata:///Local/product6.png", Price = 20.99m },
                        new Product { Name = "Product 7", ImagePath = "ms-appdata:///Local/product7.png", Price = 22.99m },
                        new Product { Name = "Product 8", ImagePath = "ms-appdata:///Local/product8.png", Price = 24.99m },
                        new Product { Name = "Product 9", ImagePath = "ms-appdata:///Local/product9.png", Price = 26.99m },
                        new Product { Name = "Product 10", ImagePath = "ms-appdata:///Local/product10.png", Price = 28.99m },
                        new Product { Name = "Product 11", ImagePath = "ms-appdata:///Local/product11.png", Price = 30.99m },
                        new Product { Name = "Product 12", ImagePath = "ms-appdata:///Local/product12.png", Price = 32.99m },
                        new Product { Name = "Product 13", ImagePath = "ms-appdata:///Local/product13.png", Price = 34.99m },
                        new Product { Name = "Product 14", ImagePath = "ms-appdata:///Local/product14.png", Price = 36.99m },
                        new Product { Name = "Product 15", ImagePath = "ms-appdata:///Local/product15.png", Price = 38.99m },
                        new Product { Name = "Product 16", ImagePath = "ms-appdata:///Local/product16.png", Price = 40.99m },
                        new Product { Name = "Product 17", ImagePath = "ms-appdata:///Local/product17.png", Price = 42.99m },
                        new Product { Name = "Product 18", ImagePath = "ms-appdata:///Local/product18.png", Price = 44.99m },
                        new Product { Name = "Product 19", ImagePath = "ms-appdata:///Local/product19.png", Price = 46.99m },
                        new Product { Name = "Product 20", ImagePath = "ms-appdata:///Local/product20.png", Price = 48.99m }
                    };

                    var insertProductCmd = connection.CreateCommand();
                    insertProductCmd.CommandText = @"
                    INSERT INTO Products (Name, Price, ImagePath)
                    VALUES (@Name, @Price, @ImagePath);
                    ";

                    foreach (var product in products)
                    {
                        insertProductCmd.Parameters.Clear();
                        insertProductCmd.Parameters.AddWithValue("@Name", product.Name);
                        insertProductCmd.Parameters.AddWithValue("@Price", product.Price);
                        insertProductCmd.Parameters.AddWithValue("@ImagePath", product.ImagePath);

                        await insertProductCmd.ExecuteNonQueryAsync();
                    }
                }
            }

            return true;
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
        public static async Task<int> Quantity()
        {
            int totalQuantity = 0;

            using (var connection = new SqliteConnection($"Filename={DbPath}"))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT SUM(Quantity) FROM Cart1";

                var result = await command.ExecuteScalarAsync();
                if (result != DBNull.Value)
                {
                    totalQuantity = Convert.ToInt32(result);
                }
            }

            return totalQuantity;
        }
        public static async Task<decimal> TotalCost()
        {
            decimal totalCost = 0;

            using (var connection = new SqliteConnection($"Filename={DbPath}"))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT SUM(Price * Quantity) FROM Cart1";

                var result = await command.ExecuteScalarAsync();
                if (result != DBNull.Value)
                {
                    totalCost = Convert.ToDecimal(result);
                }
            }

            return totalCost;
        }

        public static async Task<List<CartProduct>> GetCartProducts()
        {
            List<CartProduct> cartProducts = new List<CartProduct>();

            using (var connection = new SqliteConnection($"Filename={DbPath}"))
            {
                await connection.OpenAsync();
                
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

        public static async Task RemoveProductFromCartAsync(int productId)
        {
            using (var connection = new SqliteConnection($"Filename={DbPath}"))
            {
                await connection.OpenAsync();

                // Проверка, есть ли продукт в корзине
                var checkCartCommand = connection.CreateCommand();
                checkCartCommand.CommandText = "SELECT Quantity FROM Cart1 WHERE Id = @ProductId";
                checkCartCommand.Parameters.AddWithValue("@ProductId", productId);
                var quantity = (long?)await checkCartCommand.ExecuteScalarAsync();

                if (quantity.HasValue)
                {
                    if (quantity.Value > 1)
                    {
                        // Уменьшить количество на 1
                        var updateCartCommand = connection.CreateCommand();
                        updateCartCommand.CommandText = "UPDATE Cart1 SET Quantity = Quantity - 1 WHERE Id = @ProductId";
                        updateCartCommand.Parameters.AddWithValue("@ProductId", productId);
                        await updateCartCommand.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        // Удалить продукт из корзины
                        var deleteCartCommand = connection.CreateCommand();
                        deleteCartCommand.CommandText = "DELETE FROM Cart1 WHERE Id = @ProductId";
                        deleteCartCommand.Parameters.AddWithValue("@ProductId", productId);
                        await deleteCartCommand.ExecuteNonQueryAsync();
                    }
                }
            }
        }
    }
}
