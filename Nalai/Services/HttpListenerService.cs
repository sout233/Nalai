using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using Nalai.Helpers;

namespace Nalai.Services
{
    public delegate void RequestReceivedEventHandler(object sender, RequestEventArgs e);
    public class RequestEventArgs : EventArgs
    {
        public HttpListenerContext Context { get; }

        public RequestEventArgs(HttpListenerContext context)
        {
            Context = context;
        }
    }
    public class HttpListenerService
    {
        private HttpListener _listener;
        private bool _isListening = false;
        private int _port;

        // 定义事件
        public event RequestReceivedEventHandler RequestReceived;

        public HttpListenerService(int port)
        {
            _port = port;
        }

        public void StartListening()
        {
            if (!HttpListener.IsSupported)
            {
                NalaiMsgBox.Show("Windows XP SP2 or Server 2003 is not supported.", "Error");
            }
            if (_listener == null)
            {
                _listener = new HttpListener();
                string prefix = $"http://*:{_port}/";
                _listener.Prefixes.Add(prefix);
                _listener.Start();
                _isListening = true;
                ListenerThread();
            }
        }

        public void StopListening()
        {
            if (_listener != null && _isListening)
            {
                _isListening = false;
                _listener.Stop();
                _listener.Close();
                _listener = null;
            }
        }

        private void ListenerThread()
        {
            Thread listenerThread = new Thread(() =>
            {
                while (_isListening)
                {
                    try
                    {
                        HttpListenerContext context = _listener.GetContext();
                        // 触发事件
                        RequestReceived?.Invoke(this, new RequestEventArgs(context));
                    }
                    catch (HttpListenerException)
                    {
                        // Handle exceptions
                    }
                }
            });
            listenerThread.Start();
        }
    }
}