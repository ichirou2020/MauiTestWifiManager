using common;
using Plugin.MauiWifiManager;
using System.Runtime.CompilerServices;
using System.Text;

namespace MauiTestWifiManager
{
    /// <summary>
    /// メインページ
    /// </summary>
    public partial class MainPage : ContentPage
    {
        /// <summary>
        /// TCPクライアント
        /// </summary>
        private TcpClientClass _tcpClient = new TcpClientClass();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            _tcpClient.SetIPPort("192.168.4.1", 1999);                      // IPアドレスとポート番号を設定
            _tcpClient.ReceiveEvent += _tcpClient_ReceiveEvent;
            _tcpClient.connectedEvent += _tcpClient_ConnectedEvent;
            _tcpClient.disconnectedEvent += _tcpClient_DisconnectedEvent;   
        }

        /// <summary>
        /// 切断イベント(未実装)
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="e">イベント</param>
        /// <exception cref="NotImplementedException"></exception>
        private void _tcpClient_DisconnectedEvent(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 接続イベント
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="e">イベント</param>
        private void _tcpClient_ConnectedEvent(object? sender, EventArgs e)
        {
            Dispatcher.Dispatch(() =>
            {
                ResultTcpLabel.Text = "接続";
                SendButtn.IsEnabled = true;
            });
        }

        /// <summary>
        /// データ受信イベント
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="e">イベント</param>
        private void _tcpClient_ReceiveEvent(object? sender, EventDataArgs e)
        {
            var tmp = Encoding.UTF8.GetString(e.Data);  // 受信データを文字列に変換

            Dispatcher.Dispatch(() =>
            {
                ResultTextLabel.Text = $"{tmp}";        // 受信データを表示
            });
        }

		/// <summary>
		/// 権限確認ボタンクリック
		/// </summary>
		/// <param name="sender">送信元</param>
		/// <param name="e">イベント</param>
		private async void CheckPermissionButtono_Clicked(object sender, EventArgs e)
		{
			PermissionStatus status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
			if (status == PermissionStatus.Granted || DeviceInfo.Current.Platform == DevicePlatform.WinUI)
			{
				await Task.Delay(1000);

				// scan結果が入ってくる
				var response = await CrossWifiManager.Current.ScanWifiNetworks();
			}
			else
			{
				await DisplayAlert("No location permisson", "Please provide location permission", "OK");
			}
			//await GetWifiList();

		}

        /// <summary>
        /// Wifiアクセスポイントに接続ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConnectAccessPointButtn_Clicked(object sender, EventArgs e)
        {

			PermissionStatus status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
			string? ssid = string.Empty;

			if (status == PermissionStatus.Granted || DeviceInfo.Current.Platform == DevicePlatform.WinUI)
			{
				CrossWifiManager.Current.DisconnectWifi("esp32_apppoint");
				var response = await CrossWifiManager.Current.ConnectWifi("esp32_apppoint", "yourPassword");
				var a = await CrossWifiManager.Current.GetNetworkInfo();
			}
			else
			{
				await DisplayAlert("No location permisson", "Please provide location permission", "OK");
			}


			Dispatcher.Dispatch(() =>
            {
				ResultAccessPoitLabel.Text = "Wifi接続";
				ConnectTcpButtn.IsEnabled = true;

			});

        }

		/// <summary>
		/// TCP接続ボタンクリック
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ConnectTcpButtn_Clicked(object sender, EventArgs e)
        {
            _tcpClient.Connect();	// 接続(本当はtry catchで失敗を引っかけること/元のConnectも失敗時未テスト)
        }

		/// <summary>
		/// データ送信ボタンクリック
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SendButtn_Clicked(object sender, EventArgs e)
        {
            byte[] data = Encoding.UTF8.GetBytes(SendTextEditor.Text);
            _tcpClient.SendData(data);
        }

		/// <summary>
		/// アプリ終了時
		/// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

			// TCP切断(これをやらないと終了しても受信プロセスが残る)
			_tcpClient.Disconnect();
        }
    }

}
