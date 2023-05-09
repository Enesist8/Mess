using Mess;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Mess
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NewChat_Click(object sender, RoutedEventArgs e)
        {
            string name = Name.Text;
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Заполните имя пользователя", "Информация");
            }
            else
            {
                AdminWindow adminWindow = new AdminWindow(name);
                adminWindow.Show();
                Name.Clear();
                IP.Clear();
                if (adminWindow.IsVisible)
                {
                }
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            string name = Name.Text;
            string ip = IP.Text;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(ip))
            {
                MessageBox.Show("Заполните имя пользователя и IP адрес чата");
            }
            else
            {
                ClientWindow clientWindow = new ClientWindow(name, ip);
                clientWindow.Show();
                Name.Clear();
                IP.Clear();
                if (clientWindow.IsVisible)
                {
                }
            }
        }
    }
}
