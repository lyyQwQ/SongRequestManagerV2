using SongRequestManagerV2.Bases;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SongRequestManagerV2.Networks
{
    public class Server : BSBindableBase
    {
        private const int HeaderSize = 15;
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プロパティ
        /// <summary>ポート を取得、設定</summary>
        private int port_;
        /// <summary>ポート を取得、設定</summary>
        public int Port
        {
            get => this.port_;

            set => this.SetProperty(ref this.port_, value);
        }

        /// <summary>IPアドレス を取得、設定</summary>
        private string ip_;
        /// <summary>IPアドレス を取得、設定</summary>
        public string IP
        {
            get => this.ip_;

            set => this.SetProperty(ref this.ip_, value);
        }

        /// <summary>説明 を取得、設定</summary>
        private bool isRunning_;
        /// <summary>説明 を取得、設定</summary>
        public bool IsRunning
        {
            get => this.isRunning_;

            set => this.SetProperty(ref this.isRunning_, value);
        }

        /// <summary>説明 を取得、設定</summary>
        private string message_;
        /// <summary>説明 を取得、設定</summary>
        public string Message
        {
            get => this.message_;

            set => this.SetProperty(ref this.message_, value);
        }

        /// <summary>説明 を取得、設定</summary>
        private byte[] resBytes_;
        /// <summary>説明 を取得、設定</summary>
        public byte[] ResBytes
        {
            get => this.resBytes_;

            set => this.SetProperty(ref this.resBytes_, value);
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // コマンド
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // コマンド用メソッド
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // オーバーライドメソッド
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // パブリックメソッド
        public async Task RunServer()
        {
            if (this.IsRunning) {
                return;
            }
            this.IsRunning = true;
            try {
                this._server = new TcpListener(IPAddress.Parse(this.IP), this.Port);
                this._server.Start();
                await Task.Run(() =>
                {
                    while (this.IsRunning) {
                        TcpClient client = null;
                        try {
                            client = this._server.AcceptTcpClient();
                        }
                        catch (SocketException) {
                            if (!this.IsRunning) {
                                return;
                            }
                            throw;
                        }
                        catch (ObjectDisposedException) {
                            return;
                        }

                        Logger.Debug("Connect Client.");

                        using (client)
                        using (var ns = client.GetStream()) {
                            ns.ReadTimeout = 5000;
                            ns.WriteTimeout = 5000;

                            try {
                                var header = ReadExactly(ns, HeaderSize);
                                if (header == null || header.Length < HeaderSize) {
                                    continue;
                                }

                                var enc = GetEncodingFromCode(header[10]);
                                if (enc == null) {
                                    continue;
                                }

                                int length;
                                try {
                                    length = BitConverter.ToInt32(header, 11);
                                }
                                catch {
                                    continue;
                                }

                                if (length < 0) {
                                    continue;
                                }

                                var body = ReadExactly(ns, length);
                                if (body == null) {
                                    continue;
                                }

                                var packet = new byte[HeaderSize + body.Length];
                                Buffer.BlockCopy(header, 0, packet, 0, HeaderSize);
                                if (body.Length > 0) {
                                    Buffer.BlockCopy(body, 0, packet, HeaderSize, body.Length);
                                }

                                this.ResBytes = packet;
                                this.Message = enc.GetString(body, 0, body.Length).Replace("。", "").Replace("\0", "");
                                Logger.Debug($"{this.Message}");
                            }
                            catch (Exception ex) {
                                Logger.Error(ex);
                            }
                        }
                    }
                });
                this.IsRunning = false;
            }
            catch (Exception e) {
                Logger.Error(e);
            }
        }

        public void StopServer()
        {
            this.IsRunning = false;
            try {
                this._server?.Stop();
            }
            catch {
            }
        }

        private static Encoding GetEncodingFromCode(byte code)
        {
            try {
                if (code == 0) {
                    return Encoding.UTF8;
                }
                if (code == 1) {
                    return Encoding.Unicode;
                }
                if (code == 2) {
                    return Encoding.GetEncoding("shift_jis");
                }

                return null;
            }
            catch {
                return null;
            }
        }

        private static byte[] ReadExactly(NetworkStream stream, int length)
        {
            if (length <= 0) {
                return Array.Empty<byte>();
            }

            var buffer = new byte[length];
            var offset = 0;
            while (offset < length) {
                var read = stream.Read(buffer, offset, length - offset);
                if (read <= 0) {
                    return null;
                }
                offset += read;
            }

            return buffer;
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プライベートメソッド
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // メンバ変数
        private TcpListener _server;
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // 構築・破棄
        public Server()
        {
            this.IP = "127.0.0.1";
        }
        #endregion
    }
}
