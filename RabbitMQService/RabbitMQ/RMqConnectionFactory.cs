using System;
using RabbitMQ.Client;
using Polly.Retry;
using System.Net.Sockets;
using Polly.CircuitBreaker;
using RabbitMQ.Client.Exceptions;
using Polly;
using System.Collections.Generic;
using System.Threading.Channels;

namespace RabbitMQService.RabbitMQ
{
    public class RMqConnectionFactory
    {
        private IConnection _connection;
        private IConnectionFactory _factory;
        private readonly static object _obj = new object();
        private readonly static object _connObj = new object();
        private bool _display;
        private static string _userName;
        private static string _passWord;
        private static string _host;
        private static RMqConnectionFactory _connFactory;
        private List<IModel> _channers = new List<IModel>();

        /// <summary>
        /// 锁+双IF获取单例
        /// </summary>
        public static RMqConnectionFactory ConnFactory
        {
            get
            {
                if (_connFactory == null)
                {
                    lock (_connObj)
                    {
                        if (_connFactory == null)
                        {
                            _connFactory = new RMqConnectionFactory();
                        }
                    }
                }
                return _connFactory;
            }
        }

        /// <summary>
        /// MQ链接信息的模型委托
        /// </summary>
        private Action<MQConnectionEntity> _connectionDelegate = null;

        /// <summary>
        /// 添加MQ的链接信息
        /// </summary>
        /// <param name="connectionDelegate"></param>
        /// <returns></returns>
        public RMqConnectionFactory AddMqConnectionInfo(Action<MQConnectionEntity> connectionDelegate)
        {
            _connectionDelegate = delegate (MQConnectionEntity x)
            {
                connectionDelegate(x);

                _userName = x.UserName;
                _passWord = x.PassWord;
                _host = x.Host;
            };
            return ConnFactory;
        }

        /// <summary>
        /// 绑定MQ的链接信息
        /// </summary>
        public void Build()
        {
            if (_connectionDelegate != null)
            {
                _connectionDelegate.Invoke(new MQConnectionEntity() { PassWord="123" });
            }
        }

        /// <summary>
        /// 检测MQ是否断开
        /// </summary>
        private bool _isConnection
        {
            get
            {
                return _connection != null && _connection.IsOpen && !_display;
            }
        }

        /// <summary>
        /// 建立rabbitMQ链接
        /// </summary>
        /// <returns></returns>
        public RMqConnectionFactory GetConnectionFactory()
        {

            _factory = new ConnectionFactory()
            {
                UserName = _userName,
                Password = _passWord,
                HostName = _host
            };

            GetConnection();

            return ConnFactory;
        }

        /// <summary>
        /// 利用Policy来实现重连
        /// </summary>
        /// <returns></returns>
        private bool GetConnection()
        {
            if (_connection == null || !_connection.IsOpen)
            {
                lock (_obj)
                {
                    if (_connection == null || !_connection.IsOpen)
                    {
                        var _policy = Policy.Handle<SocketException>()
                          .Or<BrokerUnreachableException>()
                          .WaitAndRetry(10, x => TimeSpan.FromSeconds(Math.Pow(2, x)));

                        _policy.Execute(() =>
                        {
                            _connection = _factory.CreateConnection();
                        });
                    }
                }
            }
            if (_isConnection)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 获取rabbitMQ模型
        /// </summary>
        /// <returns></returns>
        public IModel GetModel()
        {
            if (!_isConnection)
            {
                throw new InvalidOperationException("没有链接上远程服务或链接未打开！");
            }
            var channel = _connection.CreateModel();
            _channers.Add(channel);
            return channel;
        }

        public List<IModel> GetChanners()
        {
            return _channers;
        }

        public void Dispose()
        {
            if (_isConnection)
            {
                if (_display) return;

                _display = true;

                try
                {
                    _connection.Dispose();
                }
                catch
                {
                    throw new InvalidOperationException("尝试释放空的实例！");
                }
            }

        }
    }
}
