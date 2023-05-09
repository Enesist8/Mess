using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace Mess
{
    public partial class ClientWindow : Window
    {
        //StreamReader – это класс который позволяет считывать символы из потока байтов в определенной кодировке.
        //StreamWriter - Предназначен для вывода символов в байтовый поток.
        string ip;
        int port;
        TcpClient client;
        StreamReader b;
        StreamWriter a;
        string name;
        public ClientWindow(string name, string ip)
        {
            InitializeComponent();
            this.ip = ip;
            port = 8888;
            this.name = name;
            Task.Run(Connect);
            Task.Run(chat);
        }

        private void Connect()
        {
            try
            {
                client = new TcpClient();
                client.Connect(this.ip, port);
                b = new StreamReader(client.GetStream());
                a = new StreamWriter(client.GetStream());
                a.AutoFlush = true;
                a.WriteLine($"/connect: {name}");
            }
            catch (Exception ex)
            {
                // Invoke () — используется для синхронного вызова
                // каждого из методов, поддерживаемых объектом делегата;
                // это означает, что вызывающий код должен ожидать завершения вызова,
                // прежде чем продолжить свою работу. 

                //Show — показывает окно.
                MessageBox.Show(ex.Message.ToString());
                Dispatcher.Invoke(() =>
                {
                    Close();
                });
            }
        }

        private void Disconnect()
        {
            try
            {
                a.WriteLine($"/disconnect: {name}");
            }
            catch
            {

            }
        }

        private void chat()
        {
            while (true)
            {
                try
                {
                    if (client != null && client.Connected == true)
                    {
                        string line = b.ReadLine();
                        if (line != null)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                if (line.Contains("/connect: ") && !string.IsNullOrEmpty(line.Replace("/connect: ", "")))
                                {
                                    string clientName = line.Replace("/connect: ", "");
                                    if (!UsersListBox.Items.Contains($"[{clientName}]"))
                                    {
                                        UsersListBox.Items.Add($"[{clientName}]");
                                    }
                                }
                                else if (line.Contains("/disconnect: ") && !string.IsNullOrEmpty(line.Replace("/disconnect: ", "")))
                                {
                                    string clientName = line.Replace("/disconnect: ", "");
                                    if (UsersListBox.Items.Contains($"[{clientName}]"))
                                    {
                                        UsersListBox.Items.Remove($"[{clientName}]");
                                    }
                                }
                                else if (line == "/exit")
                                {
                                    MessageBox.Show("Сервер чата остановлен");
                                    Close();
                                }
                                else
                                {
                                    // Split - Разделяет строку на подстроки в соответствии с указанной строкой-разделителем
                                    // AppendText() - метод открывает текстовый файл для добавления строк и создает объект StreamWriter с кодировкой UTF8,
                                    // который используется для добавления строк.
                                    string[] lineSplit = line.Split(':');
                                    ChatTextBox.AppendText($"[{DateTime.Now}] [{lineSplit[0]}]: {lineSplit[1]}\n");
                                }
                            });
                        }
                        else
                        {
                            client.Close();
                            Close();
                        }
                    }

                }
                catch (Exception ex)
                {

                }
            }
        }

        private void DiscconectButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text;
            if (string.IsNullOrEmpty(message))
            {
                MessageBox.Show("Нельзя отправить пустое сообщение");
            }
            // ToLower () - это строковый метод.
            // Он преобразует каждый символ в нижний регистр (если есть строчный символ).
            // Если символ не имеет строчного эквивалента, он остается неизменным. 
            else if (message.ToLower() == "/disconnect")
            {
                Close();
            }
            else if (message[0] == '/')
            {
                MessageBox.Show("Нет такой команды");
            }
            else
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Dispatcher.Invoke(() =>
                        {
                            a.WriteLine($"{name}: {message}");
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                });
            }
            MessageTextBox.Clear();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Task.Run(Disconnect);
            Application.Current?.MainWindow.Show();
        }
    }
}
