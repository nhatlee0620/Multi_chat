using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class Server : Form
    {
        IPEndPoint IP;
        Socket server;
        List<Socket> clientList;
        public Server()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;

            Connect();
        }

        /// <summary>
        /// Gửi tin cho tất cả Client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            foreach (Socket item in clientList)
            {
                Send(item);
            }
            Add_Message(txbMessage.Text);
            txbMessage.Clear();
        }

        /// <summary>
        /// Kết nối tới Client
        /// </summary>
        void Connect()
        {
            clientList = new List<Socket>();
            // IP: Địa chỉ của Client
            IP = new IPEndPoint(IPAddress.Any, 9999);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            
            server.Bind(IP);

            Thread Listen = new Thread(() =>{
                try
                {
                    while (true)
                    {
                        server.Listen(100);
                        Socket client = server.Accept();
                        clientList.Add(client);

                        Thread receive = new Thread(Receive);
                        receive.IsBackground = true;
                        receive.Start(client);
                    }
                }
                catch
                {
                    // Nếu 1 thằng client nào đó đóng kết nối thì khởi tạo lại
                    IP = new IPEndPoint(IPAddress.Any, 9999);
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                }
            });
            Listen.IsBackground = true;
            Listen.Start();
        }
        /// <summary>
        /// Đóng kết nối hiện thời
        /// </summary>
        void Close()
        {
            server.Close();
        }
        /// <summary>
        /// Gửi tin
        /// </summary>
        void Send(Socket client)
        {
            if (client != null && txbMessage.Text != string.Empty)
            {
                client.Send(Serialize(txbMessage.Text));
            }
        }
        /// <summary>
        /// Nhận tin
        /// </summary>
        void Receive(object obj)
        {
            Socket client = obj as Socket;
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 5000]; //1024byte * 5000
                    client.Receive(data);

                    string message = (string)Deserialize(data); // Data hiện đang là một mảng byte => Deserialize đưa về object để ép kiểu
                    
                    foreach(Socket item in clientList)
                    {
                        if(item != null && item != client)
                        {
                            item.Send(Serialize(message));
                        }
                    }
                    
                    Add_Message(message);
                }
            }
            catch
            {
                clientList.Remove(client);
                client.Close();
            }
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        }

        /// <summary>
        /// Add Message vào khung chat
        /// </summary>
        /// <param name="s"></param>
        void Add_Message(string s)
        {
            lsvMessage.Items.Add(new ListViewItem()
            {
                Text = s
            });
        }

        /// <summary>
        /// Phân mảnh 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        byte[] Serialize(object obj)
        {
            // Stream: Kết nối trực tiếp
            MemoryStream stream = new MemoryStream();
            // Format: Ghi thông tin gì trên đó
            BinaryFormatter formatter = new BinaryFormatter();

            // Phân mảnh (phân mảnh obj thành binary rồi đưa vào stream)
            formatter.Serialize(stream, obj);
            // Từ cái stream được ghi thông tin vào => Chuyển thành mảng 0110101010 => Chuyển đi
            return stream.ToArray();
        }

        /// <summary>
        /// Gom mảnh lại
        /// </summary>
        /// <returns></returns>
        object Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter formatter = new BinaryFormatter();

            // Gom mảnh
            return formatter.Deserialize(stream);
        }

        /// <summary>
        /// Đóng kết nối khi đóng Form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }
    }
}
