   M S B A M L   `   `   `  ����B  >SellApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:SellAppSellApp  5   	       SellApp.ProductWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   'localclr-namespace:SellApp     5   	   -   5          � Title$  ProductWindow��5   	   $	��675��6   $
��1200��6,    � WindowState$ 	Maximized=�5	   	   .��5      �  � ColumnDefinitions 5   
   �� 5      $	��110 �6   �� 5      .+�5   
   �� $	��Top=�6   $��Center=�6.   $��2��6K   .+�5      �� -   5        $	��110��5      $	��212��6!   ���#9acd32
������ $	��Top=�6^   $��Center=�6v   $��0,0,0,0q�6�   ��5      �� 5      $��Segoe UI)�6    $��55(�66   $
��👜��6D   $��Center=�6N   5      �� 5      -   5        $	��110��5      $	��216��6!   ���#6a5acd
������ $	��Top=�6]   $��Center=�6u   $��0,5,0,0q�6�   ��5      �� 5      $��Segoe UI)�6    $��55(�66   $
��🛒��6D   $��Center=�6N   5      �� 5!      -   5        $	��110��5!      $	��216��6!   
��� ���
������ $	��Top=�6\   $��Center=�6t   $��0,5,0,0q�6�   ��5"      �� 5#      $��Segoe UI)�6    $��55(�66   $
��🧾��6D   $��Center=�6N   5%      5&      �� 5'   
   $��1��6   ���#f0f8ff$��0,0,0,0q�6:   .+�5(      ��  �� Orientation$ 
Horizontal=�6   $��	110,0,0,0q�62   .+�5)      �� -   5         ��Name$ 	txtSearch��5)      $"��Tìm kiếm sản phẩm ...��6-   $	��550��6J   $��50��6V   $��	60,20,0,0q�6b   $��22(�6u   $	��5,0q�6�   ���	#FFF0F0F0
����������	#FF2196F3$��2q�5*   K   $��Center=�5+   
   $
��Left=�6,   5,   B   �� 5/      $��Tìm Kiếm ��6   $	��150��6-   $��55��69   $��	40,20,0,0q�6E   $
��Bold"�6X   $��18(�6j   ���#dc143c6�   51      �� 52      $ 
Horizontal=�6   $��10,0,0,0q�62   .+�53      �� -   5        $��Thêm sản phẩm 🔫��53      $	��200��64   $��40��6@   $��0,10,0,0q�6L   $��18(�6^   ���#db7093
���   �
���   �$
��Bold"�6�   6�   56      &   System.Windows.Controls.DataGrid  5N      -   5          Name$ dgData��5N   '   $��1��6      AutoGenerateColumns$ False��67      CanUserAddRows$ False��6S   $��0,20,0,0q�6j   $	��470��6|      AlternatingRowBackground
 �����	   RowBackground
	 �����
 � 	Resources
 5O      =  7System.Windows.Controls.Primitives.DataGridColumnHeader �� 
TargetType �� Property �� Value*  $System.Windows.Controls.DataGridCell��%y  '        '  �     �� 5Q      "  6   .�5R      ��  � ��$ 16(�65   �� 5T       � ��$
 Bold"�67   �� 5V       � ��
 �   �67   �� 5X       � ��$ Center=�6G   �� 5Z       �@��ClearTypeHint$ Enabled=�6H   5\      �� 5`      "  6   .�5a      ��  � ��$ 18(�65   �� 5b       � ��$ Segoe UI)�67   5c      5d         Columns 5f      0  *System.Windows.Controls.DataGridTextColumn  5g      ,  &System.Windows.Controls.DataGridColumn   Header$ ID��6)      Width5  /System.Windows.Controls.DataGridLengthConverter$
 0.2* 6`   1  +System.Windows.Controls.DataGridBoundColumn   Binding 65   �� *	ProductId+ �� Mode$ OneWay=�6`   4  .System.Windows.Controls.DataGridTemplateColumn  5h      $ Tên sản phẩm��6-   $
 1.9* 6C      CellTemplate 5i      �� 5j      .R�5k   "   �� $
��Wrap=�6V   $��Stretch=�6j   $	��Top=�6�   ��6,   �� *ProductName+$ OneWay=�6�   5l      5n        5o      $ 
Đơn vị��6)   $
 0.6* 69    6F   �� *Unit+ �� TargetNullValue$  ��$ OneWay=�   ElementStyle 5p      �� 5q      " ��6$   .�5r   "   ��  � ��$ Center=�6H   �� 5s   "    � ��$ Center=�6F   5t      5v        5x      $ Giá tiền��6)   $
 0.5* 6;    6H   �� *Price+ �� StringFormat$ 
{0:#.##} k��$ OneWay=� 5y      �� 5z      " ��6$   .�5{   "   ��  � ��$ Center=�6H   �� 5|   "    � ��$ Center=�6F   5}      5        5�      $ 
Mã vạch��6)   $
 0.9* 6:    6G   �� *	Barcode+$  ��$ OneWay=� 5�      �� 5�      " ��6$   .�5�   "   ��  � ��$ Center=�6H   �� 5�   "    � ��$ Center=�6F   5�      5�        5�      $ Trọng lượng��6-   $
 0.7* 6B    5�      �� 5�      .R�5�   "   �� $
��Wrap=�6O   $��Center=�6c   $	��Top=�6�   ��6,   �� *Note+$ OneWay=�6�   5�      5�        5�      $ 	Thao tác��6-   $
 0.8* 6?    5�      �� 5�      .R�5�   "   �� $ 
Horizontal=�6-   $��Center=�6F   .+�5�   &   �� -   5        $
��🛠��5�   -   $��50��6:   $��35��6E   $	��5,0q�6Q   ���#2e8b57$
��Bold"�6s   
���   ���5�      �� *DataContext.EditCommand+ �� RelativeSource ��  �� AncestorType DataGrid��5�      �� 6:   �� 5�   &   -	   5        $	��❌��5�   -   $��50��69   $��35��6D   $	��5,0q�6P   ���#FF6347$
��Bold"�6r   
���   ���5�      �� *DataContext.DeleteCommand+ ��  DataGrid��5�      �� 6:   5�   #   5�      5�      5�      5�      5�      5�      