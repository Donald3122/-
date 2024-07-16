using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace _11.Models
{
    public static class DatabaseHelper
    {
        // Путь к локальной базе данных SQLite
        private static readonly string DbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "products.db");

        // Инициализация базы данных
        public static async Task<bool> InitializeDatabase()
        {
            bool databaseExists = File.Exists(DbPath);

            using (var connection = new SqliteConnection($"Filename={DbPath}"))
            {
                await connection.OpenAsync();

                // Создание таблицы Products, если она не существует
                var createProductTableCmd = connection.CreateCommand();
                createProductTableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Price REAL NOT NULL,
                    ImagePath TEXT
                );";
                await createProductTableCmd.ExecuteNonQueryAsync();

                // Создание таблицы Cart1 (корзина), если она не существует
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

                // Если таблица Products пуста, добавляем 20 записей
                if (productCount == 0)
                {
                    var products = new List<Product>
                    {
                        new Product { Name = "Product 1", ImagePath = "ms-appdata:///Local/product1.png", Price = 10.99m },
                        new Product { Name = "Product 2", ImagePath = "ms-appdata:///Local/product2.png", Price = 12.99m },
                        // Добавить остальные продукты
                        // ...
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

        // Добавление продукта в корзину по его Id
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
                    // Если продукт уже есть в корзине, увеличиваем количество на 1
                    var updateCartCommand = connection.CreateCommand();
                    updateCartCommand.CommandText = "UPDATE Cart1 SET Quantity = Quantity + 1 WHERE Id = @ProductId";
                    updateCartCommand.Parameters.AddWithValue("@ProductId", productId);
                    await updateCartCommand.ExecuteNonQueryAsync();
                }
                else
                {
                    // Если продукта нет в корзине, добавляем его с количеством 1
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

        // Получение общего количества продуктов в корзине
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

        // Получение общей стоимости продуктов в корзине
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

        // Получение списка продуктов в корзине
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

        // Получение продукта по его Id
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

        // Удаление продукта из корзины по его Id
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
                        // Уменьшить количество продукта в корзине на 1
                        var updateCartCommand = connection.CreateCommand();
                        updateCartCommand.CommandText = "UPDATE Cart1 SET Quantity = Quantity - 1 WHERE Id = @ProductId";
                        updateCartCommand.Parameters.AddWithValue("@ProductId", productId);
                        await updateCartCommand.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        // Удалить продукт из корзины, если его количество равно 1
                        var deleteCartCommand = connection.CreateCommand();
                        deleteCartCommand.CommandText = "DELETE FROM Cart1 WHERE Id = @ProductId";
                        deleteCartCommand.Parameters.AddWithValue("@ProductId", productId);
                        await deleteCartCommand.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        // Получение списка всех продуктов
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

        // Удаление продукта из корзины по его Id (асинхронный вариант)
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
