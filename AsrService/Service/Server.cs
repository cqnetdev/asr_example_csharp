﻿/*********************************************************************************************
 *	
 * 文件名称:    Server.cs
 * 
 * 描    述：   TCP 通信服务端
 *
 * 作    者:    Anuo.
 *	
 * 创作日期:    2019-3-27
 *
 * 备    注:	
 *                                        
*********************************************************************************************/

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AsrService
{
    /// <summary>
    /// TCP 通信服务端
    /// </summary>
    internal class Server : IDisposable
    {
        /// <summary>
        /// IP地址
        /// </summary>
        private string _ip = "0.0.0.0";
        /// <summary>
        /// 端口
        /// </summary>
        private int _port = 8888;
        /// <summary>
        ///TCP 服务端套接字
        /// </summary>
        private Socket _socket = null;
        /// <summary>
        /// TCP 连接监听线程
        /// </summary>
        private Thread _listenThd = null;
        /// <summary>
        /// 客户端管理类
        /// </summary>
        private ClientManager _clientMgr = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Server()
        {
            _clientMgr = new ClientManager();
        }

        /// <summary>
        /// 启动
        /// </summary>
        public bool Start()
        {
            try
            {
                // 启动监听
                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(_ip), _port);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Bind(ipe);
                _socket.Listen(20);

                _listenThd = new Thread(new ThreadStart(TcpListen));
                _listenThd.IsBackground = true;
                _listenThd.Name = "Tcp Listen Thread";
                _listenThd.Start();

                return true;
            }
            catch (Exception ex)
            {
                Utils.ShowInfo(this, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            // 停止监听
            if (_listenThd != null && _listenThd.IsAlive)
            {
                try { _listenThd.Abort(); } catch { }
                _listenThd = null;
            }

            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }

            // 停止时要断开所有连接
            if (_clientMgr != null)
            {
                _clientMgr.Dispose();
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// 设置服务端信息
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="port">端口</param>
        public void SetInfo(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        /// <summary>
        /// TCP 连接监听线程
        /// </summary>
        private void TcpListen()
        {
            while (true)
            {
                try
                {
                    Socket clientSocket = _socket.Accept();

                    Guid id = Guid.NewGuid();
                    Client client = new Client(id, clientSocket);
                    _clientMgr.Add(id, client);
                }
                catch (Exception ex)
                {
                    Utils.ShowInfo(this, "[Servoce] 套接字监听异常：" + ex.Message);
                }
            }
        }
    }
}
