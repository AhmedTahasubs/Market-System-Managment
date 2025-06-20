﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using CanteenLogic;
using CanteenLogic.Entities;

namespace CanteenApp
{
    public partial class User : Form
    {
        public User()
        {
            InitializeComponent();
        }
        public Button SelectedButton
        {
            get { return Logoutbtn; }
        }
        private void LoadProductsInDataGrid()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.DataSource = ProductsManager.GetProducts();
            dataGridView1.ReadOnly = true;
            dataGridView1.Columns["Id"].Visible = false;
            dataGridView1.Columns["CategoryId"].Visible = false;
            dataGridView1.Columns["Title"].HeaderText = "Product Name";
            dataGridView1.Columns["Price"].HeaderText = "Price (EGP)";
            dataGridView1.Columns["UnitsInStock"].HeaderText = "Available Stock";
            dataGridView1.Columns["IsEmpty"].Visible = false;
        }

        private void User_Load(object sender, EventArgs e)
        {
            LoadProductsInDataGrid();
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                LoadProductsInDataGrid();
            }
            else
            {
                var products = ProductsManager.GetProductsByName(searchText);

                if (products != null && products.Count > 0)
                {
                    dataGridView1.DataSource = null; // Clear previous data source
                    dataGridView1.DataSource = products;
                    dataGridView1.ReadOnly = true;
                    dataGridView1.Columns["Id"].Visible = false;
                    dataGridView1.Columns["CategoryId"].Visible = false;
                    dataGridView1.Columns["Title"].HeaderText = "Product Name";
                    dataGridView1.Columns["Price"].HeaderText = "Price (EGP)";
                    dataGridView1.Columns["UnitsInStock"].HeaderText = "Available Stock";
                    dataGridView1.Columns["IsEmpty"].Visible = false;
                }
                else
                {
                    dataGridView1.DataSource = null;
                    MessageBox.Show("No products found matching the search criteria.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        List<OrderItem> cartItems = new();
        private void AddOrderItemToCart(int productId, int quantity)
        {
            var product = ProductsManager.GetProductById(productId);
            if (product == null) return;

            var existing = cartItems.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null)
            {
                int totalQty = existing.Quantity + quantity;
                if (totalQty > product.UnitsInStock)
                {
                    MessageBox.Show("Not enough stock available.");
                    return;
                }
                existing.Quantity = totalQty;
            }
            else
            {
                if (quantity > product.UnitsInStock)
                {
                    MessageBox.Show("Not enough stock.");
                    return;
                }

                cartItems.Add(new OrderItem
                {
                    ProductId = productId,
                    Product = product,
                    Quantity = quantity
                });
            }

            RefreshCartGrid();
            UpdateTotalLabel();
        }
        private void RefreshCartGrid()
        {
            dataGridView2.DataSource = null;
            dataGridView2.DataSource = cartItems.Select(i => new
            {
                Product = i.Product.Title,
                i.Quantity,
                UnitPrice = i.Product.Price,
                Total = i.TotalPrice
            }).ToList();
        }
        private void UpdateTotalLabel()
        {
            int total = cartItems.Sum(i => i.TotalPrice);
            lblTotal.Text = $"Total: {total} EGP";
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Get the selected product from the clicked row
                var row = dataGridView1.Rows[e.RowIndex];
                int productId = Convert.ToInt32(row.Cells["Id"].Value);
                string productName = row.Cells["Title"].Value.ToString();
                int price = Convert.ToInt32(row.Cells["Price"].Value);
                int stock = Convert.ToInt32(row.Cells["UnitsInStock"].Value);

                // Open your popup form and pass the product info
                var addForm = new FormAddQuantity(productId, productName, price, stock);
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    int quantity = addForm.SelectedQuantity;

                    //Add to cart or update cart grid(your logic here)
                    AddOrderItemToCart(productId, quantity);
                }
            }

        }
        private void btnPlaceOrder_Click(object sender, EventArgs e)
        {

        }

        private void btnPlaceOrder_Click_1(object sender, EventArgs e)
        {
            if (cartItems.Count == 0)
            {
                MessageBox.Show("Cart is empty.");
                return;
            }
            string customerName = txtCustomerName.Text.Trim();
            if (!Regex.IsMatch(customerName, @"^[a-zA-Z\s]+$"))
            {
                // Invalid: contains characters other than letters or space
                MessageBox.Show("Customer name must contain only letters.");
                return;
            }
            if (string.IsNullOrEmpty(customerName))
            {
                MessageBox.Show("Please enter customer name.");
                return;
            }
            var order = new Order
            {
                CustomerName = customerName,
                OrderDate = DateTime.Now
            };

            int orderId = OrdersManager.AddOrder(order);

            foreach (var item in cartItems)
            {
                item.OrderId = orderId;
                OrderItemsManager.AddOrderItem(item);
                ProductsManager.DecreaseStock(item.ProductId, item.Quantity); // optional
            }

            MessageBox.Show("Order placed!");
            cartItems.Clear();
            RefreshCartGrid();
            UpdateTotalLabel();
            txtCustomerName.Clear();
            LoadProductsInDataGrid();
        }

        private void Logoutbtn_Click(object sender, EventArgs e)
        {
            Hide();
            Login login = new Login();
            login.Show();
        }

        private void txtCustomerName_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
