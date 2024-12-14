using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace ZigSimTools
{
    public class UdpReceiver
    {
        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler Disconnected;

        private int _port;
        private UdpClient _udpClient;
        private Task _receiveTask;
        private bool _isReceiving = false;

        public UdpReceiver(int port)
        {
            _port = port;
        }

        public void StartReceiving()
        {
            if (_isReceiving)
            {
                return;
            }

            Debug.Log("start receiving'");
            _udpClient = new UdpClient(_port);
            _isReceiving = true;
            _receiveTask = DataReceiveTask();
        }

        public async Task StopReceiving()
        {
            if (!_isReceiving)
            {
                return;
            }

            _isReceiving = false;

            if (_receiveTask != null && _receiveTask.Status == TaskStatus.Running)
            {
                await _receiveTask;
            }
            _udpClient.Close();
            OnDisconnectrd(EventArgs.Empty);
        }

        private async Task DataReceiveTask()
        {
            await Task.Run(() =>
            {
                while (_isReceiving)
                {
                    try
                    {
                        //Debug.Log("start to listen");
                        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, _port);
                        byte[] data = _udpClient.Receive(ref remoteEP);
                        string text = System.Text.Encoding.ASCII.GetString(data);
                        OnMessageReceived(new MessageEventArgs(text));
                    }
                    catch (SocketException e)
                    {
                        Debug.LogError($"SocketException: {e.Message}");
                    }
                }
            });
        }

        protected virtual void OnMessageReceived(MessageEventArgs e)
        {
            //Debug.Log("received data'");
            MessageReceived?.Invoke(this, e);
        }

        protected virtual void OnDisconnectrd(EventArgs e)
        {
            Disconnected?.Invoke(this, e);
        }
    }

    public class MessageEventArgs : EventArgs
    {
        public string Message { get; }

        public MessageEventArgs(string message) : base()
        {
            Message = message;
        }
    }
}