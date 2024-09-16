using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace common
{
	//! @addtogroup firmupdate
	//! @{

	/// <summary>
	/// 受信イベントデータ
	/// </summary>
	public class EventDataArgs : EventArgs
	{
		/// <summary>受信データ</summary>
		public byte[]? Data; 
	}

	/// <summary>
	/// TCPクライアントクラス
	/// </summary>
	internal class TcpClientClass()
	{
		/// <summary>イベントハンドラ(受信)</summary>
		public event EventHandler<EventDataArgs>? ReceiveEvent;
		/// <summary>イベントハンドラ(接続)</summary>
		public event EventHandler<EventArgs>? connectedEvent;
		/// <summary>イベントハンドラ(切断)</summary>
		public event EventHandler<EventArgs>? disconnectedEvent;

		/// <summary>接続先IP</summary>
		string _ipadr = "";
		/// <summary>ポート</summary>
		int _port = 0;
		/// <summary>TCPクライアント</summary>
		TcpClient? _client;
		/// <summary>ストリーム</summary>
		NetworkStream? _stream;
		/// <summary>動作中</summary><remarks>true=接続中 false=切断中</remarks>
		bool _isRunning = false;
		/// <summary>受信スレッド</summary>
		Thread? _receiveThread = null;

		/// <summary>
		/// IPアドレスとポート番号を設定
		/// </summary>
		/// <param name="ipadr">IPアドレス</param>
		/// <param name="port">ポート</param>
		public void SetIPPort(string ipadr, int port)
		{
			_ipadr = ipadr;
			_port = port;
		}

		/// <summary>
		/// データ送信
		/// </summary>
		/// <param name="data">送信データ</param>
		public void SendData(byte[] data)
		{
			try
			{
				if (_stream != null)
				{
					if (_stream.CanWrite == true)
					{
						// データ送信
						_stream.Write(data, 0, data.Length);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Disconnect();
			}
		}

		/// <summary>
		/// 受信
		/// </summary>
		private void Receive()
		{
			byte[] bytes = new byte[256];
			int i;
			try
			{
				// クライアントからのデータを受信する
				// 0バイトだと切断発生
				while ((i = _stream.Read(bytes, 0, bytes.Length)) != 0)
				{
					if (bytes != null && bytes.Length > 0)
					{
						// 受信イベントを発生させる
						if (ReceiveEvent != null)
						{
							ReceiveEvent(this, new EventDataArgs() { Data = (byte[])bytes });
						}
					}
					if (_isRunning == false)
					{
						break;
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				if (_isRunning == true)
				{
					Disconnect();
				}
			}
		}

		/// <summary>
		/// 切断
		/// </summary>
		public void Disconnect()
		{
			if (_receiveThread != null)
			{
				_isRunning = false;
				if (_client != null)
				{
					_client.Close();
					_client = null;
				}
				_receiveThread.Join();
				_receiveThread = null;
			}
			if (_stream != null)
			{
				_stream.Close();
				_stream.Dispose();
				_stream = null;
			}
			if (_client != null)
			{
				_client.Close();
				_client = null;
			}

			_isRunning = false;
		}

		/// <summary>
		/// 接続
		/// </summary>
		public void Connect()
		{
			try
			{
				// 接続
				AddressFamily addressFamily = AddressFamily.InterNetwork;
				_client = new TcpClient(addressFamily);
				_client.Connect(_ipadr, _port);
				_stream = _client.GetStream();// ネットワークストリーム取得
				_stream.ReadTimeout = 10000;
				_stream.WriteTimeout = 10000;
				_isRunning = true;

				if (connectedEvent != null)
				{
					connectedEvent(this, new EventArgs());

				}
				// 受信用スレッドを開始する
				_receiveThread = new Thread(new ThreadStart(Receive));
				_receiveThread.Start();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				if (_stream != null)
				{
					_stream.Close();
					_stream.Dispose();
				}
				if (_client != null)
				{
					_client.Close();
				}
				if (_isRunning)
				{
					_isRunning = false;
				}

			}

		}
	}

	//! @}
}
