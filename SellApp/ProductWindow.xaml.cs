using Microsoft.EntityFrameworkCore;
using SellApp.Model;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SellApp;
using System.Windows.Input;

namespace SellApp
{
    public partial class ProductWindow : Window
    {
        private List<Product> products = new List<Product>();
        SellappContext myContext = new SellappContext();

        public ProductWindow()
        {
            InitializeComponent();
            LoadProducts();
        }

        private void LoadProducts()
        {
            if (dgData == null) return;

            // Sử dụng trực tiếp class Product
            var productData = myContext.Products.ToList();
            dgData.ItemsSource = productData;
            
        }

        private void Window_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Kiểm tra nếu không có TextBox nào đang focus
            if (!(Keyboard.FocusedElement is TextBox focusedTextBox))
            {
                // Focus vào TextBox mặc định (txtSearch)
                txtSearch.Focus();

                // Xóa placeholder nếu cần
                if (txtSearch.Text == "Tìm kiếm sản phẩm ...")
                {
                    txtSearch.Text = string.Empty;
                }

                // Thêm ký tự vào TextBox
                txtSearch.Text += e.Text;

                // Di chuyển con trỏ về cuối văn bản
                txtSearch.CaretIndex = txtSearch.Text.Length;

                // Ngăn sự kiện được xử lý tiếp
                e.Handled = true;
            }
            else
            {
                // Nếu TextBox khác đã được focus, để WPF xử lý như bình thường
                e.Handled = false;
            }
        }


        private void btnEdit(object sender, RoutedEventArgs e)
        {
            if (dgData.SelectedItem is Product selectedProduct)
            {
                var editWindow = new EditWindow(selectedProduct);
                bool? result = editWindow.ShowDialog();

                if (result == true) // Nếu chỉnh sửa thành công
                {
                    // Làm mới lại DataGrid từ cơ sở dữ liệu (tải lại từ DbContext)
                    dgData.ItemsSource = myContext.Products.ToList();  // Tải lại danh sách sản phẩm
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn sản phẩm để sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void btnAdd(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddWindow();
            addWindow.ShowDialog(); // Hiển thị cửa sổ thêm
            LoadProducts();         // Cập nhật lại danh sách sản phẩm
        }


        private void btnDelete(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem có dòng nào trong DataGrid được chọn không
            var selectedProduct = dgData.SelectedItem as Product;
            if (selectedProduct == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Xác nhận việc xóa sản phẩm
            var result = MessageBox.Show("Bạn có chắc chắn muốn xóa sản phẩm này không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // Tìm sản phẩm trong cơ sở dữ liệu
            var productToDelete = myContext.Products.FirstOrDefault(p => p.ProductId == selectedProduct.ProductId);
            if (productToDelete == null)
            {
                MessageBox.Show("Không tìm thấy sản phẩm cần xóa.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Xóa sản phẩm khỏi cơ sở dữ liệu
            myContext.Products.Remove(productToDelete);
            myContext.SaveChanges();

            // Tải lại danh sách sản phẩm trên DataGrid
            LoadProducts();
            //MessageBox.Show("Xóa sản phẩm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RemovePlaceholder(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text == "ID" || tb.Text == "Tên sản phẩm" || tb.Text == "Mã vạch" || tb.Text == "Đơn vị" || tb.Text == "Giá Tiền" || tb.Text == "Ghi chú" || tb.Text == "Tìm kiếm sản phẩm ...")
            {
                tb.Text = "";
                tb.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void AddPlaceholder(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                if (tb.Name == "PID")
                    tb.Text = "ID";
                else if (tb.Name == "PName")
                    tb.Text = "Tên sản phẩm";
                else if (tb.Name == "PCode")
                    tb.Text = "Mã vạch";
                else if (tb.Name == "PUnit")
                    tb.Text = "Đơn vị";
                else if (tb.Name == "PPrice")
                    tb.Text = "Giá Tiền";
                else if (tb.Name == "PDetail")
                    tb.Text = "Ghi chú";
                else if (tb.Name == "txtSearch")
                    tb.Text = "Tìm kiếm sản phẩm ...";

                tb.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Kiểm tra null hoặc trường hợp không có sản phẩm trong danh sách
            if (myContext.Products == null || !myContext.Products.Any())
            {
                MessageBox.Show("Không có dữ liệu sản phẩm.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var searchText = txtSearch.Text.ToLower().Trim();

            if (string.IsNullOrEmpty(searchText) || searchText == "tìm kiếm sản phẩm ...")
            {
                // Nếu ô tìm kiếm trống, tải lại toàn bộ sản phẩm
                LoadProducts();
                return;
            }

            // Lọc sản phẩm dựa trên văn bản tìm kiếm
                var filteredProducts = myContext.Products
    .Where(p => p.ProductName.ToLower().Contains(searchText) || 
                p.Barcode == searchText)
    .ToList();


            // Kiểm tra kết quả lọc
            if (!filteredProducts.Any())
            {
                return;
            }

            // Cập nhật DataGrid với kết quả tìm kiếm
            dgData.ItemsSource = filteredProducts;
        }

        private void btnSell(object sender, RoutedEventArgs e)
        {
            MainWindow sell = new MainWindow();
            sell.Show();
            this.Close();
        }

        private void btnPro(object sender, RoutedEventArgs e)
        {
            ProductWindow product = new ProductWindow();
            product.Show();
            this.Close();
        }

        private void btnOrder(object sender, RoutedEventArgs e)
        {
            OrderDetail order = new OrderDetail();
            order.Show();
            this.Close();
        }
    }
}

