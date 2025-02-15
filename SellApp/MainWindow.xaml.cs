using SellApp.Model;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System;
using MySqlX.XDevAPI.Common;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
namespace SellApp
{

    public partial class MainWindow : Window
    {
        public string Debt { get; set; } = "0";

        private List<Product> products = new List<Product>();
        private List<Orderdetail> orderDetails = new List<Orderdetail>(); // Danh sách chi tiết đơn hàng
        private SellappContext myContext = new SellappContext();

        private ObservableCollection<Orderdetail> invoiceDetails = new ObservableCollection<Orderdetail>();



        public MainWindow()
        {
            InitializeComponent();
            lblTotalAmount.Content = "0 VND";
            lblChange.Content = "0 VND";
            LoadProducts();
        }

        private void LoadProducts()
        {
            if (dgData == null) return;
            var productData = myContext.Products
                .Select(s => new
                {
                    s.ProductId,
                    s.ProductName,
                    s.Barcode,
                    s.Unit,
                    s.Price,
                    s.Note,
                })
                .ToList();

            dgData.ItemsSource = productData;
        }

        private void Window_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Kiểm tra nếu TextBox đang focus không phải là PleName, Ple, PCUs, hoặc CustomerPaid
            if (Keyboard.FocusedElement is TextBox focusedTextBox &&
                (focusedTextBox == PleName || focusedTextBox == Ple || focusedTextBox == PCUs || focusedTextBox == CustomerPaid))
            {
                return;
            }

            // Thêm ký tự vào txtSearch ngay cả khi nó chưa focus
            if (txtSearch != null)
            {
                // Nếu chưa focus, focus vào txtSearch và thêm ký tự
                Application.Current.Dispatcher.Invoke(() =>
                {
                    txtSearch.Focus();

                    // Xóa placeholder nếu cần
                    if (txtSearch.Text == "Tìm kiếm sản phẩm ...")
                    {
                        txtSearch.Text = string.Empty;
                    }

                    // Thêm ký tự đầu tiên ngay lập tức
                    txtSearch.Text += e.Text;

                    // Di chuyển con trỏ về cuối văn bản
                    txtSearch.CaretIndex = txtSearch.Text.Length;
                }, System.Windows.Threading.DispatcherPriority.Input);

                // Ngăn sự kiện được xử lý tiếp
                e.Handled = true;
            }
        }




        private void dgData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedProduct = dgData.SelectedItem;
            if (selectedProduct == null) return;

            dynamic product = selectedProduct;

            // Kiểm tra xem sản phẩm đã có trong hóa đơn chưa
            var existingItem = invoiceDetails.FirstOrDefault(p => p.ProductId == product.ProductId);
            if (existingItem != null)
            {
                // Nếu sản phẩm đã tồn tại, tăng số lượng
                existingItem.Quantity++;
            }
            else
            {
                // Nếu chưa tồn tại, thêm sản phẩm mới vào hóa đơn
                var newItem = new Orderdetail
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    UnitBill = product.Unit,
                    UnitPrice = product.Price,
                    Quantity = 1
                };
                invoiceDetails.Add(newItem);
            }
            dgHoaDon.ItemsSource = invoiceDetails;
            // Cập nhật DataGrid hóa đơn bên phải
            RefreshInvoiceGrid();

            // Cập nhật tổng tiền ngay lập tức sau khi thay đổi
            UpdateTotalAmount();
        }


        private void RefreshInvoiceGrid()
        {
            // Tính toán lại số thứ tự (STT) cho các sản phẩm trong hóa đơn
            int index = 1;
            foreach (var item in invoiceDetails)
            {
                item.Stt = index++;
            }

            // Không cần tái tạo ObservableCollection, chỉ cần gán trực tiếp
            dgHoaDon.ItemsSource = null;
            dgHoaDon.ItemsSource = invoiceDetails;
        }


        // Khi người dùng nhấn vào TextBox (focus)
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                // Xóa nội dung khi người dùng nhấn vào TextBox nếu nội dung là mặc định
                if (textBox.Text == "Hàng thêm" || textBox.Text == "Giá")
                {
                    textBox.Text = string.Empty;  // Xóa nội dung
                    textBox.Foreground = Brushes.Black;  // Đặt màu chữ là đen để dễ nhìn
                }
            }
        }

        // Khi người dùng rời khỏi TextBox (lost focus)
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                // Nếu TextBox trống, hiển thị lại văn bản mặc định
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    if (textBox.Name == "PleName")
                    {
                        textBox.Text = "Hàng thêm";  // Văn bản mặc định cho PleName
                    }
                    else if (textBox.Name == "Ple")
                    {
                        textBox.Text = "Giá";  // Văn bản mặc định cho Ple
                    }

                    textBox.Foreground = Brushes.Gray;  // Đặt màu chữ là xám cho văn bản gợi ý
                }
            }
        }

        // Phương thức AddItemToInvoice
        private void AddItemToInvoice(object sender, RoutedEventArgs e)
        {
            // Lấy giá trị từ TextBox Ple
            if (!decimal.TryParse(Ple.Text, out decimal unitPrice) || unitPrice <= 0)
            {
                MessageBox.Show("Vui lòng nhập giá !", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Lấy tên hàng từ TextBox PleName
            string productName = string.IsNullOrWhiteSpace(PleName.Text) || PleName.Text == "Hàng thêm"
                ? "Hàng lẻ" // Mặc định
                : PleName.Text;

            // Kiểm tra xem sản phẩm đã tồn tại trong danh sách hay chưa
            var existingItem = invoiceDetails.FirstOrDefault(item => item.ProductName == productName);

            if (existingItem != null)
            {
                // Nếu là hàng lẻ, cộng dồn giá vào đơn giá cũ (không thay đổi số lượng)
                if (productName == "Hàng lẻ")
                {
                    existingItem.UnitPrice += unitPrice;  // Cộng dồn vào giá cũ
                    existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice; // Tính lại tổng tiền
                }
                else
                {
                    // Nếu là hàng có tên, cộng dồn giá vào đơn giá cũ (không thay đổi số lượng)
                    existingItem.UnitPrice += unitPrice;  // Cộng dồn vào giá cũ
                    existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice; // Tính lại tổng tiền
                }
            }
            else
            {
                // Nếu sản phẩm chưa tồn tại, thêm mới
                var newItem = new Orderdetail
                {
                    ProductName = productName, // Lấy từ TextBox PleName
                    UnitPrice = unitPrice,     // Lấy từ TextBox Ple
                    Quantity = 1,              // Số lượng mặc định là 1
                    TotalPrice = unitPrice,    // Thành tiền = đơn giá * số lượng
                    ProductId = invoiceDetails.Any() ? invoiceDetails.Max(i => i.ProductId) + 1 : 1, // Đảm bảo ProductId là duy nhất
                    Stt = invoiceDetails.Any() ? invoiceDetails.Max(i => i.Stt) + 1 : 1 // STT tự động tăng hoặc bắt đầu từ 1 nếu danh sách rỗng
                };

                // Thêm vào danh sách ObservableCollection
                invoiceDetails.Add(newItem);
            }

            // Cập nhật tổng tiền và tiền thối
            UpdateTotalAmount();
            UpdateChangeAmount();

            // Đặt lại giá trị mặc định trong TextBox sau khi submit
            ResetTextBoxes();

            // Làm mới DataGrid để hiển thị lại các giá trị
            if (invoiceDetails.Count == 1) // Kiểm tra nếu DataGrid chưa có hàng nào
            {
                dgHoaDon.ItemsSource = null; // Tạm thời xóa dữ liệu
                dgHoaDon.ItemsSource = invoiceDetails; // Cập nhật lại danh sách
            }
            else
            {
                dgHoaDon.Items.Refresh();  // Nếu đã có hàng, chỉ làm mới hiển thị
            }
        }

        // Phương thức để đặt lại giá trị mặc định cho TextBox sau khi submit
        private void ResetTextBoxes()
        {
            PleName.Text = "Hàng thêm";  // Đặt lại giá trị mặc định
            Ple.Text = "Giá";  // Đặt lại giá trị mặc định

            // Đặt màu chữ là xám cho văn bản gợi ý
            PleName.Foreground = Brushes.Gray;
            Ple.Foreground = Brushes.Gray;
        }


        private void ChangeQuantity(int productId, int amount)
        {
            var product = invoiceDetails.FirstOrDefault(p => p.ProductId == productId);
            if (product != null)
            {
                product.Quantity += amount;

                // Kiểm tra nếu số lượng <= 0 thì xóa sản phẩm
                if (product.Quantity <= 0)
                {
                    invoiceDetails.Remove(product);
                }
            }

            // Cập nhật lại DataGrid và tổng tiền
            RefreshInvoiceGrid();
            UpdateTotalAmount();
            UpdateChangeAmount(); // Cập nhật "Trả lại"
        }

        private void IncreaseQuantity(int productId)
        {
            var product = invoiceDetails.FirstOrDefault(p => p.ProductId == productId);
            if (product != null)
            {
                product.Quantity++;
            }

            // Cập nhật lại DataGrid, tổng tiền và trả lại
            RefreshInvoiceGrid();
            UpdateTotalAmount();  // Cập nhật tổng tiền
            UpdateChangeAmount();  // Cập nhật Trả lại
        }

        private void DecreaseQuantity(int productId)
        {
            var product = invoiceDetails.FirstOrDefault(p => p.ProductId == productId);
            if (product != null && product.Quantity > 1)
            {
                product.Quantity--;
            }
            else if (product != null && product.Quantity == 1)
            {
                // Nếu số lượng bằng 1, xóa sản phẩm khỏi hóa đơn
                invoiceDetails.Remove(product);
            }

            // Cập nhật lại DataGrid, tổng tiền và trả lại
            RefreshInvoiceGrid();
            UpdateTotalAmount();  // Cập nhật tổng tiền
            UpdateChangeAmount();  // Cập nhật Trả lại
        }




        // Cập nhật tổng tiền
        private void UpdateTotalAmount()
        {
            // Tính tổng tiền từ danh sách hóa đơn
            decimal totalAmount = invoiceDetails.Sum(item => item.TotalPrice); // Tổng tiền từ TotalPrice của từng sản phẩm

            // Đọc giá trị từ TextBox "Hàng lẻ" (Ple)
            //if (decimal.TryParse(Ple.Text, out decimal additionalAmount))
            //{
            //    totalAmount += additionalAmount; // Cộng thêm giá trị từ TextBox
            //}

            // Hiển thị tổng tiền
            lblTotalAmount.Content = $"{totalAmount * 1000:#,##0} VND"; // Đảm bảo hiển thị đúng

            // Cập nhật số tiền trả lại sau khi tổng tiền thay đổi
            UpdateChangeAmount();
        }


        // Cập nhật trả lại khi khách đưa tiền
        private void CustomerPaid_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateChangeAmount(); // Cập nhật Trả lại khi Khách đưa thay đổi
        }

        // Cập nhật số tiền trả lại
        private void UpdateChangeAmount()
        {
            // Đọc giá trị từ lblTotalAmount và TextBox "Khách đưa"
            if (decimal.TryParse(lblTotalAmount.Content?.ToString()?.Replace(" VND", "").Replace(",", ""), out decimal totalAmount) &&
                decimal.TryParse(CustomerPaid.Text, out decimal customerPaid))
            {
                // Tính toán số tiền trả lại
                decimal changeAmount = customerPaid * 1000 - totalAmount;

                // Hiển thị số tiền trả lại
                lblChange.Content = $"{changeAmount:#,##0} VND";
            }
            else
            {
                lblChange.Content = "0 VND"; // Giá trị mặc định khi không hợp lệ
            }
        }



        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var productId = (int)button.Tag;
                IncreaseQuantity(productId);
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var productId = (int)button.Tag;
                DecreaseQuantity(productId);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (myContext == null || myContext.Products == null || !myContext.Products.Any())
            {
                MessageBox.Show("Không có dữ liệu sản phẩm.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var searchText = txtSearch.Text.ToLower().Trim();

            // Nếu không nhập hoặc là placeholder thì tải lại danh sách sản phẩm
            if (string.IsNullOrEmpty(searchText) || searchText == "tìm kiếm sản phẩm ...")
            {
                LoadProducts(); // Lỗi có thể phát sinh từ phương thức này, cần kiểm tra cách tải lại sản phẩm
                return;
            }

            // Tìm sản phẩm trùng mã vạch
            var product = myContext.Products
                .FirstOrDefault(p => p.Barcode == searchText);

            if (product != null)
            {
                // Nếu tìm thấy, kiểm tra xem đã có trong hóa đơn chưa
                var existingItem = invoiceDetails.FirstOrDefault(p => p.ProductId == product.ProductId);
                if (existingItem != null)
                {
                    // Nếu đã tồn tại, tăng số lượng
                    existingItem.Quantity++;
                }
                else
                {
                    // Nếu chưa tồn tại, thêm sản phẩm vào hóa đơn
                    var newItem = new Orderdetail
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        UnitBill = product.Unit,
                        UnitPrice = product.Price,
                        Quantity = 1
                    };
                    invoiceDetails.Add(newItem);
                }

                // Cập nhật hóa đơn
                RefreshInvoiceGrid();
                UpdateTotalAmount();

                // Xóa nội dung tìm kiếm
                txtSearch.Text = string.Empty;
                return;
            }

            // Nếu không tìm thấy, thực hiện tìm kiếm trong danh sách sản phẩm
            var filteredProducts = myContext.Products
                .Where(p => p.ProductName.ToLower().Contains(searchText))
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.Barcode,
                    p.Unit,
                    p.Price,
                    p.Note
                })
                .ToList();

            if (filteredProducts.Any())
            {
                dgData.ItemsSource = filteredProducts;
            }
            else
            {
                dgData.ItemsSource = null;
            }
        }


        private void RemovePlaceholder(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb == null) return;

            // Kiểm tra và xóa placeholder dựa vào Text hiện tại
            if (tb.Text == "Tìm kiếm sản phẩm ..." || tb.Text == "Hàng thêm" || tb.Text == "Giá")
            {
                tb.Text = ""; // Xóa placeholder
                tb.Foreground = new SolidColorBrush(Colors.Black); // Đổi màu chữ
            }
        }


        private void AddPlaceholder(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb == null || !string.IsNullOrWhiteSpace(tb.Text)) return;

            // Kiểm tra tên TextBox để khôi phục placeholder phù hợp
            if (tb.Name == "PleName")
            {
                tb.Text = "Hàng thêm"; // Placeholder cho TextBox Hàng thêm
            }
            else if (tb.Name == "Ple")
            {
                tb.Text = "Giá"; // Placeholder cho TextBox giá
            }
            else
            {
                tb.Text = "Tìm kiếm sản phẩm ..."; // Placeholder mặc định
            }

            tb.Foreground = new SolidColorBrush(Colors.Gray); // Đổi màu chữ placeholder
        }


        private void btnSell(object sender, RoutedEventArgs e)
        {
            MainWindow sell = new MainWindow();
            sell.Show();
            this.Close();
        }

        private void btnOrder(object sender, RoutedEventArgs e)
        {
            OrderDetail order = new OrderDetail();
            order.Show();
            this.Close();
        }
        private void btnPro(object sender, RoutedEventArgs e)
        {
            ProductWindow product = new ProductWindow();
            product.Show();
            this.Close();
        }

        private void btnCom(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Xác nhận hoàn thành đơn hàng?", "Xác nhận", MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.OK)
            {
                // Tạo đối tượng Order mới (bao gồm thông tin khách hàng và thời gian hiện tại)
                var order = new Order
                {
                    Customer = PCUs.Text.Trim(),  // Thông tin khách hàng từ giao diện
                    OrderDate = DateTime.Now,              // Thời gian hiện tại
                    TotalAmount = invoiceDetails.Sum(i => i.TotalPrice),  // Tổng tiền từ danh sách chi tiết hóa đơn
                    Orderdetails = new List<Orderdetail>() // Khởi tạo danh sách Orderdetails
                };

                // Duyệt qua danh sách invoiceDetails và tạo các Orderdetail
                foreach (var item in invoiceDetails)
                {
                    var orderDetail = new Orderdetail
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        UnitBill = item.UnitBill,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity,
                        Stt = item.Stt,
                        TotalPrice = item.TotalPrice  // Giá trị thành tiền từ chi tiết
                    };

                    // Thêm Orderdetail vào danh sách Orderdetails của Order
                    order.Orderdetails.Add(orderDetail);
                }

                // Lưu đối tượng Order vào database (bao gồm Order và Orderdetails)
                try
                {
                    myContext.Orders.Add(order);  // Thêm đối tượng Order vào DbContext
                    myContext.SaveChanges();      // Lưu thay đổi vào cơ sở dữ liệu

                    // Thông báo đơn hàng đã được hoàn thành
                    MessageBox.Show("Đơn hàng đã được hoàn thành!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (DbUpdateException ex)
                {
                    MessageBox.Show($"Lỗi khi lưu dữ liệu: {ex.InnerException?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi lưu dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Xóa danh sách hóa đơn và reset các trường
                invoiceDetails.Clear();
                Ple.Text = "";
                CustomerPaid.Text = "";
                lblChange.Content = "0 VND";
                PCUs.Text = "";

                // Cập nhật lại DataGrid và tổng tiền
                RefreshInvoiceGrid();
                UpdateTotalAmount();
            }
        }



        private void SaveOrderDetails()
        {
            // Lấy giá trị từ TextBox (PCUs), nếu trống thì gán là null
            string customerInput = PCUs.Text.Trim();
            string customerValue = string.IsNullOrEmpty(customerInput) ? null : customerInput;

            // Duyệt qua danh sách các chi tiết hóa đơn (Orderdetails)
            foreach (var item in invoiceDetails)
            {
                // Tạo đối tượng Orderdetail mới từ các thông tin của từng sản phẩm trong hóa đơn
                var orderDetail = new Orderdetail
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    UnitBill = item.UnitBill,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    Stt = item.Stt,
                    TotalPrice = item.TotalPrice // Giá trị gán sẽ được lưu trong _calculatedTotalPrice
                };

                // Thêm đối tượng Orderdetail vào DbContext
                myContext.Orderdetails.Add(orderDetail);
            }

            // Tạo đối tượng Order và lưu thông tin đơn hàng
            var order = new Order
            {
                Customer = customerValue,
                OrderDate = DateTime.Now,
                TotalAmount = invoiceDetails.Sum(i => i.TotalPrice),
                Orderdetails = myContext.Orderdetails.ToList() // Gán Orderdetails vào Order
            };

            try
            {
                // Thêm đơn hàng vào DbContext
                myContext.Orders.Add(order);
                // Lưu thay đổi vào cơ sở dữ liệu
                myContext.SaveChanges(); // Chỉ gọi SaveChanges một lần

                MessageBox.Show("Dữ liệu đã được lưu thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"Lỗi khi lưu dữ liệu: {ex.InnerException?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void LoadInvoiceToCart(List<dynamic> invoiceDetails)
        {
            dgHoaDon.ItemsSource = invoiceDetails;
        }





        //private void PDeb_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Debt = PDeb.Text;
        //}

        //private void PDeb_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (decimal.TryParse(PDeb.Text, out decimal debtAmount))
        //    {
        //        PDeb.Text = $"{debtAmount:N0}";
        //    }
        //}
        // Phương thức tạo nội dung hóa đơn
        private StackPanel CreateInvoiceContent()
        {
            var panel = new StackPanel
            {
                Width = 300,
                Margin = new System.Windows.Thickness(10)
            };

            // Dòng 1: Tên cửa hàng và SĐT
            StackPanel headerRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            headerRow.Children.Add(new TextBlock
            {
                Text = "CỬA HÀNG SIM KHA",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left
            });

            headerRow.Children.Add(new TextBlock
            {
                Text = "SDT: 0973835155",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new System.Windows.Thickness(10, 0, 0, 0)
            });

            panel.Children.Add(headerRow);

            // Dòng 2: HÓA ĐƠN BÁN HÀNG
            panel.Children.Add(new TextBlock
            {
                Text = "HÓA ĐƠN BÁN HÀNG",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new System.Windows.Thickness(-35, 15, 0, 10)
            });

            // Dòng 3: Thông tin khách hàng và ngày
            string customerInfoText = !string.IsNullOrWhiteSpace(PCUs.Text)
                ? $"Tên khách hàng: {PCUs.Text}"
                : string.Empty;

            // Tạo StackPanel theo chiều dọc
            StackPanel customerInfoPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new System.Windows.Thickness(0, 5, 0, 10)
            };

            // Thêm thông tin khách hàng nếu có
            if (!string.IsNullOrEmpty(customerInfoText))
            {
                customerInfoPanel.Children.Add(new TextBlock
                {
                    Text = customerInfoText,
                    FontSize = 15,
                    FontWeight = FontWeights.Bold,
                });
            }

            // Thêm ngày vào dòng thứ hai
            customerInfoPanel.Children.Add(new TextBlock
            {
                Text = $"Ngày {DateTime.Now:dd/MM/yyyy}",
                FontSize = 15,
                FontWeight = FontWeights.Bold,
            });

            // Thêm StackPanel vào panel chính
            panel.Children.Add(customerInfoPanel);

            // Dòng 4: Tiêu đề cột
            panel.Children.Add(new TextBlock
            {
                Text = "Tên SP              SL     Đ.Giá   T.Tiền",
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Left
            });

            panel.Children.Add(new TextBlock
            {
                Text = new string('=', 50),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new System.Windows.Thickness(0, 5, 0, 5)
            });

            // Dòng 5: Nội dung hóa đơn
            foreach (var item in invoiceDetails)
            {
                StackPanel row = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                row.Children.Add(new TextBlock
                {
                    Text = $"{item.ProductName}",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Width = 100, // Tỉ lệ 4
                    TextWrapping = TextWrapping.Wrap
                });

                row.Children.Add(new TextBlock
                {
                    Text = $"{item.Quantity}",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Width = 30, // Tỉ lệ 1
                    TextAlignment = TextAlignment.Center
                });

                row.Children.Add(new TextBlock
                {
                    Text = $"{item.UnitPrice * 1000:N0}",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Width = 60, // Tỉ lệ 2
                    TextAlignment = TextAlignment.Center
                });

                row.Children.Add(new TextBlock
                {
                    Text = $"{item.TotalPrice * 1000:N0}",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Width = 110, // Tỉ lệ 3
                    TextAlignment = TextAlignment.Left
                });

                panel.Children.Add(row);
            }

            // Dòng "Hàng lẻ" nếu có
            if (!string.IsNullOrWhiteSpace(Ple.Text) && decimal.TryParse(Ple.Text, out decimal oddItemAmount))
            {
                StackPanel oddRow = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                oddRow.Children.Add(new TextBlock
                {
                    Text = "Hàng lẻ",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Width = 100, // Tỉ lệ 4
                    TextWrapping = TextWrapping.Wrap
                });

                oddRow.Children.Add(new TextBlock
                {
                    Text = "1",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Width = 30, // Tỉ lệ 1
                    TextAlignment = TextAlignment.Center
                });

                oddRow.Children.Add(new TextBlock
                {
                    Text = $"{oddItemAmount * 1000:N0}",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Width = 60, // Tỉ lệ 2
                    TextAlignment = TextAlignment.Center
                });

                oddRow.Children.Add(new TextBlock
                {
                    Text = $"{oddItemAmount * 1000:N0}",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Width = 110, // Tỉ lệ 3
                    TextAlignment = TextAlignment.Left
                });

                panel.Children.Add(oddRow);
            }

            // Dòng 6: Tổng tiền và Còn nợ
            StackPanel footerRow = new StackPanel
            {
                Orientation = Orientation.Vertical, // Sắp xếp theo chiều dọc
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new System.Windows.Thickness(-35, 10, 0, 50)
            };

            // Tính tổng tiền từ chi tiết hóa đơn
            decimal totalAmount = invoiceDetails.Sum(item => item.TotalPrice); // Tổng tiền từ TotalPrice của từng sản phẩm

            // Kiểm tra và cộng giá trị hàng lẻ vào tổng tiền
            decimal oddItemAmountFinal = 0;
            if (!string.IsNullOrWhiteSpace(Ple.Text) && decimal.TryParse(Ple.Text, out decimal pleValue))
            {
                oddItemAmountFinal = pleValue;
            }
            totalAmount += oddItemAmountFinal; // Cộng giá trị hàng lẻ vào tổng tiền

            // Dòng Tổng tiền
            footerRow.Children.Add(new TextBlock
            {
                Text = $"Tổng tiền: {totalAmount * 1000:#,##0} VND",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center, // Căn giữa
                Margin = new System.Windows.Thickness(0, 0, 0, 0) // Thêm khoảng cách phía trên
            });

            // Dòng Còn nợ (nếu có)
            //if (decimal.TryParse(PDeb.Text, out decimal debtAmount))
            //{
            //    string formattedDebt = debtAmount == Math.Floor(debtAmount)
            //        ? $"{debtAmount * 1000:#,##0} VND" // Không có phần lẻ
            //        : $"{debtAmount * 1000:#,##0} VND"; // Có phần lẻ, giữ ba chữ số sau dấu phẩy

            //    footerRow.Children.Add(new TextBlock
            //    {
            //        Text = $"Còn nợ: {formattedDebt}",
            //        FontSize = 16,
            //        FontWeight = FontWeights.Bold,
            //        HorizontalAlignment = HorizontalAlignment.Center, // Căn giữa
            //        Margin = new System.Windows.Thickness(-27, 0, 0, 0) // Thêm khoảng cách phía trên
            //    });
            //}



            panel.Children.Add(footerRow);

            // Dòng 7: Thêm ảnh và thông tin bên phải
            try
            {
                // Tạo StackPanel ngang
                StackPanel imageAndInfoPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new System.Windows.Thickness(0, 10, 0, 0)
                };

                // Thêm ảnh vào bên trái
                var image = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(@"C:\Users\hzeny\Pictures\Screenshots\z6091704004426_c06edf87fce764ccb58518172a554097.jpg")),
                    Height = 100,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                imageAndInfoPanel.Children.Add(image);

                // Thêm thông tin vào bên phải
                StackPanel rightPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new System.Windows.Thickness(40, 0, 0, 0), // Cách ảnh 1 chút
                    VerticalAlignment = VerticalAlignment.Center // Căn chỉnh xuống dưới
                };

                rightPanel.Children.Add(new TextBlock
                {
                    Text = "VIETCOMBANK",
                    FontWeight = FontWeights.Bold,
                    FontSize = 13
                });

                rightPanel.Children.Add(new TextBlock
                {
                    Text = "STK: 9973835155",
                    FontWeight = FontWeights.Bold,
                    FontSize = 13
                });

                rightPanel.Children.Add(new TextBlock
                {
                    Text = "NGUYEN THI SIM",
                    FontWeight = FontWeights.Bold,
                    FontSize = 13
                });

                // Thêm một Spacer để căn các dòng chữ xuống dưới cùng
                rightPanel.Children.Add(new UIElement()); // Đây là Spacer giúp đẩy các dòng text xuống dưới cùng

                imageAndInfoPanel.Children.Add(rightPanel);

                // Thêm StackPanel vào panel chính
                panel.Children.Add(imageAndInfoPanel);
            }
            catch
            {
                panel.Children.Add(new TextBlock
                {
                    Text = "Không thể tải ảnh.",
                    FontSize = 12,
                    Margin = new System.Windows.Thickness(0, 10, 0, 0)
                });
            }

            return panel;
        }


        // Phương thức in hóa đơn
        private void btnPrint(object sender, RoutedEventArgs e)
        {
            // Tạo nội dung hóa đơn
            var printContent = CreateInvoiceContent();

            // Hiển thị hộp thoại in
            PrintDialog printDialog = new PrintDialog();
            printDialog.PrintTicket.PageMediaSize = new System.Printing.PageMediaSize(80, 200); // Khổ K80, dài tùy hóa đơn

            if (printDialog.ShowDialog() == true)
            {
                // In trực tiếp
                printDialog.PrintVisual(printContent, "Hóa đơn bán hàng");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
