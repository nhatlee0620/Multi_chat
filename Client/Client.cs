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

namespace Client
{
    public partial class Client : Form
    {

        IPEndPoint IP;
        Socket client;
        public Client()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;

            Connect(); 
        }

        /// <summary>
        /// Gửi tin đi
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            Send();
            Add_Message(txbMessage.Text);
        }
        /// <summary>
        /// Kết nối tới Server
        /// </summary>
        void Connect()
        {
            // IP: Địa chỉ của Server
            IP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            try
            {
                client.Connect(IP);
            }
            catch
            {
                MessageBox.Show("Không thể kết nối đến Server", "Lỗi kết nối", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Thread listen = new Thread(Receive);
            listen.IsBackground = true;
            listen.Start();
        }
        /// <summary>
        /// Đóng kết nối hiện thời
        /// </summary>
        void Close()
        {
            client.Close();
        }
        /// <summary>
        /// Gửi tin
        /// </summary>
        void Send()
        {
            if(txbMessage.Text != string.Empty)
            {
                client.Send(Serialize(txbMessage.Text));
            }
        }
        /// <summary>
        /// Nhận tin
        /// </summary>
        void Receive()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 5000]; //1024byte * 5000
                    client.Receive(data);

                    string message = (string)Deserialize(data); // Data hiện đang là một mảng byte => Deserialize đưa về object để ép kiểu
                    Add_Message(message);
                }
            }  
            catch
            {
                Close();
            }
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
            txbMessage.Clear();
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
        private void Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }
    }
}
