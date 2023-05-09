using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;

namespace Mess
{

    public partial class AdminWindow : Window
    {
        string name;
        static TcpListener listener = new TcpListener(IPAddress.Any, 8888);
        static List<Soedinenie> clients = new List<Soedinenie>();

        public AdminWindow(string name)
        {
            InitializeComponent();
            this.name = name;
        }

        static async void SendToAllClients(string message)
        {
            await Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    try
                    {
                        if (clients[i].Client.Connected)
                        {
                            var sw = new StreamWriter(clients[i].Client.GetStream());
                            sw.AutoFlush = true;
                            sw.WriteLine(message);
                        }
                        else
                        {
                            clients.RemoveAt(i);
                        }

                    }
                    catch
                    {

                    }
                }
            });
        }

        private void ChatListener()
        {
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Task.Factory.StartNew(() =>
                {
                    StreamReader sr = new StreamReader(client.GetStream());
                    while (client.Connected)
                    {
                        string line = sr.ReadLine();
                        if (line.Contains("/connect: ") && !string.IsNullOrEmpty(line.Replace("/connect: ", "")))
                        {
                            string clientName = line.Replace("/connect: ", "");
                            if (clients.FirstOrDefault(s => s.Name == clientName) == null && clientName != name)
                            {
                                clients.Add(new Soedinenie(client, clientName));
                                StreamWriter sw = new StreamWriter(client.GetStream());
                                sw.AutoFlush = true;
                                sw.WriteLine($"/connect: {name}");
                                for (int i = 0; i < clients.Count; i++)
                                {
                                    try
                                    {
                                        if (clients[i].Client.Connected)
                                        {

                                            sw.WriteLine($"/connect: {clients[i].Name}");
                                            StreamWriter sw2 = new StreamWriter(clients[i].Client.GetStream());
                                            sw2.AutoFlush = true;
                                            sw2.WriteLine($"/connect: {clientName}");
                                        }
                                        else
                                        {
                                            clients.RemoveAt(i);
                                        }

                                    }
                                    catch
                                    {

                                    }
                                }
                                Dispatcher.Invoke(() =>
                                {
                                    Users.Items.Add($"[{clientName}]");
                                    chateri.Items.Add($"[{DateTime.Now}] подключение: [{clientName}]");
                                });
                                break;
                            }
                            else
                            {
                                StreamWriter sw = new StreamWriter(client.GetStream());
                                sw.AutoFlush = true;
                                sw.WriteLine("/name_already_exists");
                                client.Client.Disconnect(false);
                            }
                        }
                    }

                    while (client.Connected)
                    {
                        try
                        {
                            sr = new StreamReader(client.GetStream());
                            string line = sr.ReadLine();
                            if (line.Contains("/disconnect: ") && !string.IsNullOrWhiteSpace(line.Replace("/disconnect: ", "")))
                            {
                                string clientName = line.Replace("/disconnect: ", "");
                                Soedinenie connectedClient = clients.FirstOrDefault(s => s.Name == clientName);
                                if (connectedClient != null)
                                {
                                    clients.Remove(connectedClient);
                                    Dispatcher.Invoke(() =>
                                    {
                                        Users.Items.Remove($"[{clientName}]");
                                        chateri.Items.Add($"[{DateTime.Now}] отключение: [{clientName}]");
                                    });
                                    SendToAllClients(line);
                                    break;
                                }
                            }
                            else
                            {

                                Dispatcher.Invoke(() =>
                                {
                                    string[] lineSplit = line.Split(':');
                                    ChatTextBox.AppendText($"[{DateTime.Now}] [{lineSplit[0]}]: {lineSplit[1]}\n");
                                });
                                SendToAllClients(line);
                            }
                        }
                        catch
                        {

                        }
                    }

                });
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            chateri.Items.Add($"[{DateTime.Now}] подключение: [{name}]");
            Users.Items.Add($"[{name}]");
            try
            {
                listener.Start();
                Task.Run(ChatListener);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }
        }

        private void Discconect_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            string message = Message.Text;
            if (string.IsNullOrWhiteSpace(message))
            {
                MessageBox.Show("Нельзя отправить пустое сообщение");
            }
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
                SendToAllClients($"{name}: {message}");


                ChatTextBox.AppendText($"[{DateTime.Now}] [{name}]: {message}\n");

            }
            Message.Clear();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SendToAllClients("/exit");
            Application.Current?.MainWindow.Show();
        }

        private void ChangeShowButton_Click(object sender, RoutedEventArgs e)
        {
            if (Logi.Content.ToString() == "Посмотреть логи чата")
            {
                UsersColumn.Width = new GridLength(0);
                LogsColumn.Width = new GridLength(0.4, GridUnitType.Star);
                Logi.Content = "Посмотреть пользователей чата";
            }
            else
            {
                LogsColumn.Width = new GridLength(0);
                UsersColumn.Width = new GridLength(0.4, GridUnitType.Star);
                Logi.Content = "Посмотреть логи чата";
            }

        }
    }
}
