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
using Microsoft.EntityFrameworkCore;
using SellApp.Model;

namespace SellApp
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        private SellappContext myContext = new SellappContext();
        private Product selectedProduct;

        public EditWindow(Product product)
        {
            InitializeComponent();
            selectedProduct = product;

            // Binding dữ liệu từ sản phẩm vào các TextBox
            PName.Text = product.ProductName;
            PCode.Text = product.Barcode;
            PUnit.Text = product.Unit;
            Price.Text = product.Price.ToString("0.##");
            PDetail.Text = product.Note;
        }

        private void btnSaveEdit(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PName.Text) || string.IsNullOrWhiteSpace(Price.Text) || !decimal.TryParse(Price.Text, out decimal price))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên và giá sản phẩm!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Cập nhật thông tin sản phẩm
            selectedProduct.ProductName = PName.Text.Trim();
            selectedProduct.Barcode = string.IsNullOrWhiteSpace(PCode.Text) ? null : PCode.Text.Trim();
            selectedProduct.Unit = string.IsNullOrWhiteSpace(PUnit.Text) ? null : PUnit.Text.Trim();
            selectedProduct.Price = price;
            selectedProduct.Note = string.IsNullOrWhiteSpace(PDetail.Text) ? null : PDetail.Text.Trim();

            // Kiểm tra lại trạng thái của đối tượng, nếu bị Detached, phải gắn lại vào DbContext
            if (myContext.Entry(selectedProduct).State == EntityState.Detached)
            {
                myContext.Products.Attach(selectedProduct);  // Gắn lại đối tượng vào DbContext nếu nó không còn được theo dõi
            }

            // Cập nhật đối tượng trong cơ sở dữ liệu
            myContext.Products.Update(selectedProduct);
            myContext.SaveChanges();

            //MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

            this.DialogResult = true; // Trả về true để thông báo đã thành công
            this.Close();
        }

    }

}
