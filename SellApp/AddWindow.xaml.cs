using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SellApp.Model;

namespace SellApp
{
    /// <summary>
    /// Interaction logic for AddWindow.xaml
    /// </summary>
    public partial class AddWindow : Window
    {
        private SellappContext myContext = new SellappContext();

        public AddWindow()
        {
            InitializeComponent();
        }

        private void btnSaveAdd(object sender, RoutedEventArgs e)
        {
            // Kiểm tra tính hợp lệ của dữ liệu
            if (string.IsNullOrWhiteSpace(PName.Text) || string.IsNullOrWhiteSpace(Price.Text) || !decimal.TryParse(Price.Text, out decimal price))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên và giá sản phẩm!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Kiểm tra mã vạch đã tồn tại
            if (!string.IsNullOrWhiteSpace(PCode.Text)) // Chỉ kiểm tra nếu mã vạch không rỗng
            {
                string barcode = PCode.Text.Trim();
                bool isBarcodeExist = myContext.Products.Any(p => p.Barcode == barcode);

                if (isBarcodeExist)
                {
                    MessageBox.Show("Mã vạch đã được sử dụng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }


            // Tạo đối tượng sản phẩm mới
            var newProduct = new Product
            {
                ProductName = PName.Text.Trim(),
                Barcode = string.IsNullOrWhiteSpace(PCode.Text) ? null : PCode.Text.Trim(),
                Unit = string.IsNullOrWhiteSpace(PUnit.Text) ? null : PUnit.Text.Trim(),
                Price = price,
                Note = string.IsNullOrWhiteSpace(PDetail.Text) ? null : PDetail.Text.Trim()
            };

            // Thêm sản phẩm vào cơ sở dữ liệu
            myContext.Products.Add(newProduct);
            myContext.SaveChanges();

            //MessageBox.Show("Thêm sản phẩm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }
    }

}
