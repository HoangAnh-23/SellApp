using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using SellApp.Model;

namespace SellApp
{
    public partial class OrderDetail : Window
    {
        private SellappContext myContext = new SellappContext();

        public OrderDetail()
        {
            InitializeComponent();
            LoadInvoiceList();
        }

        // Load danh sách hóa đơn vào DataGrid
        private void LoadInvoiceList()
        {
            // Lấy dữ liệu từ cơ sở dữ liệu và sắp xếp
            var invoicesFromDb = myContext.Orders
                .OrderByDescending(order => order.OrderDate)
                .ToList(); // Tải dữ liệu vào bộ nhớ

            // Xử lý STT trong bộ nhớ
            var invoices = invoicesFromDb
                .Select((order, index) => new
                {
                    Stt = (index + 1).ToString(), // Đánh số thứ tự bắt đầu từ 1
                    order.OrderId,
                    order.Customer,  // Tên khách hàng
                    OrderDate = order.OrderDate.ToString("HH:mm - dd/MM/yyyy") // Định dạng thời gian
                })
                .ToList();

            // Kiểm tra xem có hóa đơn nào không
            if (invoices.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu hóa đơn.");
            }
            else
            {
                dgInvoiceList.ItemsSource = invoices;
            }
        }




        // Khi người dùng chọn một hóa đơn từ DataGrid
        private void dgInvoiceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Kiểm tra xem người dùng có chọn một hóa đơn không
            var selectedInvoice = dgInvoiceList.SelectedItem as dynamic;
            if (selectedInvoice == null)
            {
                MessageBox.Show("Bạn chưa chọn hóa đơn.");
                return;
            }

            int selectedOrderId = selectedInvoice.OrderId; // Lấy OrderId của hóa đơn được chọn

            // Lấy danh sách chi tiết hóa đơn từ cơ sở dữ liệu
            var invoiceDetails = myContext.Orderdetails
                .Where(detail => detail.OrderId == selectedOrderId) // Lọc theo OrderId
                .Select(detail => new
                {
                    detail.Stt,  // Số thứ tự
                    detail.ProductName,  // Tên sản phẩm
                    detail.UnitBill,  // Đơn vị
                    detail.UnitPrice,  // Đơn giá
                    detail.Quantity,  // Số lượng
                    detail.TotalPrice  // Thành tiền
                }).ToList();

            // Kiểm tra xem có chi tiết hóa đơn không
            if (invoiceDetails.Count == 0)
            {
                MessageBox.Show("Không có chi tiết hóa đơn cho hóa đơn này.");
            }

            // Gán dữ liệu vào DataGrid chi tiết hóa đơn
            dgInvoiceDetail.ItemsSource = invoiceDetails;

            // Tính tổng tiền
            decimal totalAmount = invoiceDetails.Sum(detail => detail.TotalPrice);
            lblTotalAmount.Content = $"{totalAmount * 1000:#,##0} VND";
        }


        private void btnUse(object sender, RoutedEventArgs e)
        {
            var selectedInvoiceDetails = dgInvoiceDetail.ItemsSource as List<dynamic>;
            if (selectedInvoiceDetails == null || selectedInvoiceDetails.Count == 0)
            {
                MessageBox.Show("Không có sản phẩm nào để sử dụng.");
                return;
            }

            // Truyền danh sách sản phẩm sang MainWindow
            MainWindow mainWindow = Application.Current.Windows
                .OfType<MainWindow>()
                .FirstOrDefault();

            if (mainWindow != null)
            {
                mainWindow.LoadInvoiceToCart(selectedInvoiceDetails);
                mainWindow.Show();
                this.Close(); // Đóng cửa sổ OrderDetail
            }
            else
            {
                MessageBox.Show("Không tìm thấy cửa sổ bán hàng.");
            }
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
