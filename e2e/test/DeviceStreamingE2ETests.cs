// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientDeviceStreamingRequest = Microsoft.Azure.Devices.Client.DeviceStreamRequest;
using ServiceDeviceStreamingRequest = Microsoft.Azure.Devices.DeviceStreamRequest;
using CE = Microsoft.Azure.Devices.Common.Exceptions;

namespace Microsoft.Azure.Devices.E2ETests
{
    [TestClass]
    [TestCategory("IoTHub-E2E")]
    public partial class DeviceStreamingTests : IDisposable
    {
        private readonly string DevicePrefix = $"E2E_{nameof(DeviceStreamingTests)}_";
        private readonly string ModulePrefix = $"E2E_{nameof(DeviceStreamingTests)}_";
        private static string ProxyServerAddress = Configuration.IoTHub.ProxyServerAddress;
        private static TestLogging _log = TestLogging.GetInstance();

        private readonly ConsoleEventListener _listener;

        public DeviceStreamingTests()
        {
            _listener = TestConfig.StartEventListener();
        }

#region Device Client Tests
        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_Sas_Amqp()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_Tcp_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.Sasl, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_Sas_Amqp_WithProxy()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_Tcp_Only)
            {
                Proxy = new WebProxy(ProxyServerAddress)
            };
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.Sasl, transportSettings, true).ConfigureAwait(false);
        }


        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_Sas_AmqpWs()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_WebSocket_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.Sasl, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_Sas_AmqpWs_WithProxy()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_WebSocket_Only)
            {
                Proxy = new WebProxy(ProxyServerAddress)
            };
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.Sasl, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_Sas_Mqtt()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.Sasl, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_Sas_Mqtt_WithProxy()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_Tcp_Only)
                {
                    Proxy = new WebProxy(ProxyServerAddress)
                };
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.Sasl, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_Sas_MqttWs()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_WebSocket_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.Sasl, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_Sas_MqttWs_WithProxy()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_WebSocket_Only)
                {
                    Proxy = new WebProxy(ProxyServerAddress)
                };
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.Sasl, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_x509_Amqp()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_Tcp_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.X509, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_x509_Amqp_WithProxy()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_Tcp_Only)
            {
                Proxy = new WebProxy(ProxyServerAddress)
            };
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.X509, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_x509_Mqtt()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.X509, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_x509_Mqtt_WithProxy()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_Tcp_Only)
                {
                    Proxy = new WebProxy(ProxyServerAddress)
                };
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.X509, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_x509_Amqp()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_Tcp_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.X509, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_x509_Amqp_WithProxy()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_Tcp_Only)
            {
                Proxy = new WebProxy(ProxyServerAddress)
            };
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.X509, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_x509_Mqtt()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.X509, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_x509_Mqtt_WithProxy()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_Tcp_Only)
                {
                    Proxy = new WebProxy(ProxyServerAddress)
                };
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.X509, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_Sas_Amqp()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_Tcp_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.Sasl, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_Sas_Amqp_WithProxy()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_Tcp_Only)
            {
                Proxy = new WebProxy(ProxyServerAddress)
            };

            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.Sasl, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_Sas_AmqpWs()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_WebSocket_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.Sasl, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_Sas_AmqpWs_WithProxy()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_WebSocket_Only)
            {
                Proxy = new WebProxy(ProxyServerAddress)
            };
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.Sasl, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_Sas_Mqtt()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.Sasl, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_Sas_Mqtt_WithProxy()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_Tcp_Only)
                {
                    Proxy = new WebProxy(ProxyServerAddress)
                };
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.Sasl, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_Sas_MqttWs()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_WebSocket_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.Sasl, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_Sas_MqttWs_WithProxy()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_WebSocket_Only)
                {
                    Proxy = new WebProxy(ProxyServerAddress)
                };
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestDeviceStreamingAsync(TestDeviceType.Sasl, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task DeviceStreaming_WaitForDeviceStreamRequestAsync_5secs_TimesOut_Sas_Amqp()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_Tcp_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            TestDevice testDevice = await TestDevice.GetTestDeviceAsync(DevicePrefix).ConfigureAwait(false);

            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            using (DeviceClient deviceClient = testDevice.CreateDeviceClient(transportSettings))
            {
                await deviceClient.OpenAsync(cts.Token).ConfigureAwait(false);

                try
                {
                    ClientDeviceStreamingRequest clientRequestTask = await deviceClient.WaitForDeviceStreamRequestAsync(cts.Token).ConfigureAwait(false);
                }
                catch (IotHubCommunicationException ce)
                {
                    throw ce.InnerException;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task DeviceStreaming_WaitForDeviceStreamRequestAsync_5secs_TimesOut_Sas_Amqp_WithProxy()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_Tcp_Only)
            {
                Proxy = new WebProxy(ProxyServerAddress)
            };
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            TestDevice testDevice = await TestDevice.GetTestDeviceAsync(DevicePrefix).ConfigureAwait(false);

            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            using (DeviceClient deviceClient = testDevice.CreateDeviceClient(transportSettings))
            {
                await deviceClient.OpenAsync(cts.Token).ConfigureAwait(false);

                try
                {
                    ClientDeviceStreamingRequest clientRequestTask = await deviceClient.WaitForDeviceStreamRequestAsync(cts.Token).ConfigureAwait(false);
                }
                catch (IotHubCommunicationException ce)
                {
                    throw ce.InnerException;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task DeviceStreaming_WaitForDeviceStreamRequestAsync_5secs_TimesOut_Sas_Mqtt()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            TestDevice testDevice = await TestDevice.GetTestDeviceAsync(DevicePrefix).ConfigureAwait(false);

            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            using (DeviceClient deviceClient = testDevice.CreateDeviceClient(transportSettings))
            {
                await deviceClient.OpenAsync(cts.Token).ConfigureAwait(false);

                try
                {
                    ClientDeviceStreamingRequest clientRequestTask = await deviceClient.WaitForDeviceStreamRequestAsync(cts.Token).ConfigureAwait(false);
                }
                catch (IotHubCommunicationException ce)
                {
                    throw ce.InnerException;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task DeviceStreaming_WaitForDeviceStreamRequestAsync_5secs_TimesOut_Sas_Mqtt_WithProxy()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_Tcp_Only)
                {
                    Proxy = new WebProxy(ProxyServerAddress)
                };
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            TestDevice testDevice = await TestDevice.GetTestDeviceAsync(DevicePrefix).ConfigureAwait(false);

            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            using (DeviceClient deviceClient = testDevice.CreateDeviceClient(transportSettings))
            {
                await deviceClient.OpenAsync(cts.Token).ConfigureAwait(false);

                try
                {
                    ClientDeviceStreamingRequest clientRequestTask = await deviceClient.WaitForDeviceStreamRequestAsync(cts.Token).ConfigureAwait(false);
                }
                catch (IotHubCommunicationException ce)
                {
                    throw ce.InnerException;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task DeviceStreaming_WaitForDeviceStreamRequestAsync_5secs_TimesOut_x509_Amqp()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_Tcp_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            TestDevice testDevice = await TestDevice.GetTestDeviceAsync(DevicePrefix, TestDeviceType.X509).ConfigureAwait(false);

            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            using (DeviceClient deviceClient = testDevice.CreateDeviceClient(transportSettings))
            {
                await deviceClient.OpenAsync(cts.Token).ConfigureAwait(false);

                try
                {
                    ClientDeviceStreamingRequest clientRequestTask = await deviceClient.WaitForDeviceStreamRequestAsync(cts.Token).ConfigureAwait(false);
                }
                catch (IotHubCommunicationException ce)
                {
                    throw ce.InnerException;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task DeviceStreaming_WaitForDeviceStreamRequestAsync_5secs_TimesOut_x509_Amqp_WithProxy()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_Tcp_Only)
            {
                Proxy = new WebProxy(ProxyServerAddress)
            };
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            TestDevice testDevice = await TestDevice.GetTestDeviceAsync(DevicePrefix, TestDeviceType.X509).ConfigureAwait(false);

            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            using (DeviceClient deviceClient = testDevice.CreateDeviceClient(transportSettings))
            {
                await deviceClient.OpenAsync(cts.Token).ConfigureAwait(false);

                try
                {
                    ClientDeviceStreamingRequest clientRequestTask = await deviceClient.WaitForDeviceStreamRequestAsync(cts.Token).ConfigureAwait(false);
                }
                catch (IotHubCommunicationException ce)
                {
                    throw ce.InnerException;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task DeviceStreaming_WaitForDeviceStreamRequestAsync_5secs_TimesOut_x509_Mqtt()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            TestDevice testDevice = await TestDevice.GetTestDeviceAsync(DevicePrefix, TestDeviceType.X509).ConfigureAwait(false);

            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            using (DeviceClient deviceClient = testDevice.CreateDeviceClient(transportSettings))
            {
                await deviceClient.OpenAsync(cts.Token).ConfigureAwait(false);

                try
                {
                    ClientDeviceStreamingRequest clientRequestTask = await deviceClient.WaitForDeviceStreamRequestAsync(cts.Token).ConfigureAwait(false);
                }
                catch (IotHubCommunicationException ce)
                {
                    throw ce.InnerException;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task DeviceStreaming_WaitForDeviceStreamRequestAsync_5secs_TimesOut_x509_Mqtt_WithProxy()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_Tcp_Only)
                {
                    Proxy = new WebProxy(ProxyServerAddress)
                };
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            TestDevice testDevice = await TestDevice.GetTestDeviceAsync(DevicePrefix, TestDeviceType.X509).ConfigureAwait(false);

            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            using (DeviceClient deviceClient = testDevice.CreateDeviceClient(transportSettings))
            {
                await deviceClient.OpenAsync(cts.Token).ConfigureAwait(false);

                try
                {
                    ClientDeviceStreamingRequest clientRequestTask = await deviceClient.WaitForDeviceStreamRequestAsync(cts.Token).ConfigureAwait(false);
                }
                catch (IotHubCommunicationException ce)
                {
                    throw ce.InnerException;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        #endregion Device Client Tests

        #region Module Client Tests
        [TestMethod]
        public async Task ModuleStreaming_RequestAccepted_Sas_Amqp()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_Tcp_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestModuleStreamingAsync(TestDeviceType.Sasl, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ModuleStreaming_RequestAccepted_Sas_Amqp_WithProxy()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_Tcp_Only)
            {
                Proxy = new WebProxy(ProxyServerAddress)
            };
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestModuleStreamingAsync(TestDeviceType.Sasl, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ModuleStreaming_RequestAccepted_Sas_AmqpWs()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_WebSocket_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestModuleStreamingAsync(TestDeviceType.Sasl, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ModuleStreaming_RequestAccepted_Sas_AmqpWs_WithProxy()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_WebSocket_Only)
            {
                Proxy = new WebProxy(ProxyServerAddress)
            };
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestModuleStreamingAsync(TestDeviceType.Sasl, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ModuleStreaming_RequestAccepted_Sas_Mqtt()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestModuleStreamingAsync(TestDeviceType.Sasl, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ModuleStreaming_RequestAccepted_Sas_Mqtt_WithProxy()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_Tcp_Only)
                {
                    Proxy = new WebProxy(ProxyServerAddress)
                };
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestModuleStreamingAsync(TestDeviceType.Sasl, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ModuleStreaming_RequestAccepted_Sas_MqttWs()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_WebSocket_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestModuleStreamingAsync(TestDeviceType.Sasl, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ModuleStreaming_RequestAccepted_Sas_MqttWs_WithProxy()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_WebSocket_Only)
                {
                    Proxy = new WebProxy(ProxyServerAddress)
                };
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestModuleStreamingAsync(TestDeviceType.Sasl, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ModuleStreaming_RequestRejected_Sas_MqttWs()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_WebSocket_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestModuleStreamingAsync(TestDeviceType.Sasl, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ModuleStreaming_RequestRejected_Sas_MqttWs_WithProxy()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_WebSocket_Only)
                {
                    Proxy = new WebProxy(ProxyServerAddress)
                };
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestModuleStreamingAsync(TestDeviceType.Sasl, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ModuleStreaming_RequestRejected_Sas_Amqp()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_Tcp_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestModuleStreamingAsync(TestDeviceType.Sasl, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ModuleStreaming_RequestRejected_Sas_Amqp_WithProxy()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_Tcp_Only)
            {
                Proxy = new WebProxy(ProxyServerAddress)
            };
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestModuleStreamingAsync(TestDeviceType.Sasl, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ModuleStreaming_RequestRejected_Sas_AmqpWs()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_WebSocket_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestModuleStreamingAsync(TestDeviceType.Sasl, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ModuleStreaming_RequestRejected_Sas_AmqpWs_WithProxy()
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_WebSocket_Only)
            {
                Proxy = new WebProxy(ProxyServerAddress)
            };
            ITransportSettings[] transportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestModuleStreamingAsync(TestDeviceType.Sasl, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ModuleStreaming_RequestRejected_Sas_Mqtt()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestModuleStreamingAsync(TestDeviceType.Sasl, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ModuleStreaming_RequestRejected_Sas_Mqtt_WithProxy()
        {
            Client.Transport.Mqtt.MqttTransportSettings mqttTransportSettings =
                new Client.Transport.Mqtt.MqttTransportSettings(Client.TransportType.Mqtt_Tcp_Only)
                {
                    Proxy = new WebProxy(ProxyServerAddress)
                };
            ITransportSettings[] transportSettings = new ITransportSettings[] { mqttTransportSettings };

            await TestModuleStreamingAsync(TestDeviceType.Sasl, transportSettings, false).ConfigureAwait(false);
        }

        #endregion Module Client Tests

        #region Service Client Tests

        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_Sas_Service_Amqp()
        {
            await TestDeviceStreamingAsync(TestDeviceType.Sasl, TransportType.Amqp, new ServiceClientTransportSettings(), true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_Sas_Service_Amqp_WithProxy()
        {
            ServiceClientTransportSettings transportSettings = new ServiceClientTransportSettings()
            {
                AmqpProxy = new WebProxy(ProxyServerAddress),
                HttpProxy = new WebProxy(ProxyServerAddress)
            };
            await TestDeviceStreamingAsync(TestDeviceType.Sasl, TransportType.Amqp, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_Sas_Service_AmqpWs()
        {
            await TestDeviceStreamingAsync(TestDeviceType.Sasl, TransportType.Amqp_WebSocket_Only, new ServiceClientTransportSettings(), true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_Sas_Service_Amqpws_WithProxy()
        {
            ServiceClientTransportSettings transportSettings = new ServiceClientTransportSettings()
            {
                AmqpProxy = new WebProxy(ProxyServerAddress),
                HttpProxy = new WebProxy(ProxyServerAddress)
            };
            await TestDeviceStreamingAsync(TestDeviceType.Sasl, TransportType.Amqp_WebSocket_Only, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_Sas_Service_Amqp()
        {
            await TestDeviceStreamingAsync(TestDeviceType.Sasl, TransportType.Amqp, new ServiceClientTransportSettings(), false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_Sas_Service_Amqp_WithProxy()
        {
            ServiceClientTransportSettings transportSettings = new ServiceClientTransportSettings()
            {
                AmqpProxy = new WebProxy(ProxyServerAddress),
                HttpProxy = new WebProxy(ProxyServerAddress)
            };
            await TestDeviceStreamingAsync(TestDeviceType.Sasl, TransportType.Amqp, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_Sas_Service_AmqpWs()
        {
            await TestDeviceStreamingAsync(TestDeviceType.Sasl, TransportType.Amqp_WebSocket_Only, new ServiceClientTransportSettings(), false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_Sas_Service_Amqpws_WithProxy()
        {
            ServiceClientTransportSettings transportSettings = new ServiceClientTransportSettings()
            {
                AmqpProxy = new WebProxy(ProxyServerAddress),
                HttpProxy = new WebProxy(ProxyServerAddress)
            };
            await TestDeviceStreamingAsync(TestDeviceType.Sasl, TransportType.Amqp_WebSocket_Only, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_x509_Service_Amqp()
        {
            await TestDeviceStreamingAsync(TestDeviceType.X509, TransportType.Amqp, new ServiceClientTransportSettings(), true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_x509_Service_Amqp_WithProxy()
        {
            ServiceClientTransportSettings transportSettings = new ServiceClientTransportSettings()
            {
                AmqpProxy = new WebProxy(ProxyServerAddress),
                HttpProxy = new WebProxy(ProxyServerAddress)
            };
            await TestDeviceStreamingAsync(TestDeviceType.X509, TransportType.Amqp, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_x509_Service_AmqpWs()
        {
            await TestDeviceStreamingAsync(TestDeviceType.X509, TransportType.Amqp_WebSocket_Only, new ServiceClientTransportSettings(), true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestAccepted_x509_Service_Amqpws_WithProxy()
        {
            ServiceClientTransportSettings transportSettings = new ServiceClientTransportSettings()
            {
                AmqpProxy = new WebProxy(ProxyServerAddress),
                HttpProxy = new WebProxy(ProxyServerAddress)
            };
            await TestDeviceStreamingAsync(TestDeviceType.X509, TransportType.Amqp_WebSocket_Only, transportSettings, true).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_x509_Service_Amqp()
        {
            await TestDeviceStreamingAsync(TestDeviceType.X509, TransportType.Amqp, new ServiceClientTransportSettings(), false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_x509_Service_Amqp_WithProxy()
        {
            ServiceClientTransportSettings transportSettings = new ServiceClientTransportSettings()
            {
                AmqpProxy = new WebProxy(ProxyServerAddress),
                HttpProxy = new WebProxy(ProxyServerAddress)
            };
            await TestDeviceStreamingAsync(TestDeviceType.X509, TransportType.Amqp, transportSettings, false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_x509_Service_AmqpWs()
        {
            await TestDeviceStreamingAsync(TestDeviceType.X509, TransportType.Amqp_WebSocket_Only, new ServiceClientTransportSettings(), false).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeviceStreaming_RequestRejected_x509_Service_Amqpws_WithProxy()
        {
            ServiceClientTransportSettings transportSettings = new ServiceClientTransportSettings()
            {
                AmqpProxy = new WebProxy(ProxyServerAddress),
                HttpProxy = new WebProxy(ProxyServerAddress)
            };
            await TestDeviceStreamingAsync(TestDeviceType.X509, TransportType.Amqp_WebSocket_Only, transportSettings, false).ConfigureAwait(false);
        }

        [Ignore]
        [TestMethod]
        public async Task DeviceStreaming_WaitForDeviceStreamResponseAsync_5secs_TimesOut_Amqp()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            using (ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(Configuration.IoTHub.ConnectionString, TransportType.Amqp))
            {
                await serviceClient.OpenAsync().ConfigureAwait(false);
                DeviceStreamRequest deviceStreamRequest = new DeviceStreamRequest(
                    streamName: "TestStream"
                );
                TestDevice testDevice = await TestDevice.GetTestDeviceAsync(DevicePrefix).ConfigureAwait(false);

                try
                {
                    DeviceStreamResponse result = await serviceClient.CreateStreamAsync(testDevice.Id, deviceStreamRequest).ConfigureAwait(false);
                }
                catch (CE.DeviceNotFoundException ex)
                {
                    Assert.AreEqual(CE.ErrorCode.DeviceTimeout, ex.Code, ex.Message);
                }
                catch
                {
                    throw;
                }
            }
        }
        
        [Ignore]
        [TestMethod]
        public async Task DeviceStreaming_WaitForDeviceStreamResponseAsync_5secs_TimesOut_Amqp_WithProxy()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            using (ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(
                Configuration.IoTHub.ConnectionString, 
                TransportType.Amqp, 
                new ServiceClientTransportSettings()
                {
                    AmqpProxy = new WebProxy(ProxyServerAddress),
                    HttpProxy = new WebProxy(ProxyServerAddress)
                }))
            {
                await serviceClient.OpenAsync().ConfigureAwait(false);
                DeviceStreamRequest deviceStreamRequest = new DeviceStreamRequest(
                    streamName: "TestStream"
                );
                TestDevice testDevice = await TestDevice.GetTestDeviceAsync(DevicePrefix).ConfigureAwait(false);

                try
                {
                    DeviceStreamResponse result = await serviceClient.CreateStreamAsync(testDevice.Id, deviceStreamRequest).ConfigureAwait(false);
                }
                catch (CE.DeviceNotFoundException ex)
                {
                    Assert.AreEqual(CE.ErrorCode.DeviceTimeout, ex.Code, ex.Message);
                }
                catch
                {
                    throw;
                }
            }
        }

        [Ignore]
        [TestMethod]
        public async Task DeviceStreaming_WaitForDeviceStreamResponseAsync_5secs_TimesOut_AmqpWs()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            using (ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(Configuration.IoTHub.ConnectionString, TransportType.Amqp_WebSocket_Only))
            {
                await serviceClient.OpenAsync().ConfigureAwait(false);
                DeviceStreamRequest deviceStreamRequest = new DeviceStreamRequest(
                    streamName: "TestStream"
                );
                TestDevice testDevice = await TestDevice.GetTestDeviceAsync(DevicePrefix).ConfigureAwait(false);

                try
                {
                    DeviceStreamResponse result = await serviceClient.CreateStreamAsync(testDevice.Id, deviceStreamRequest).ConfigureAwait(false);
                }
                catch (CE.DeviceNotFoundException ex)
                {
                    Assert.AreEqual(CE.ErrorCode.DeviceTimeout, ex.Code, ex.Message);
                }
                catch
                {
                    throw;
                }
            }
        }

        [Ignore]
        [TestMethod]
        public async Task DeviceStreaming_WaitForDeviceStreamResponseAsync_5secs_TimesOut_AmqpWs_WithProxy()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            using (ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(
                Configuration.IoTHub.ConnectionString,
                TransportType.Amqp_WebSocket_Only,
                new ServiceClientTransportSettings()
                {
                    AmqpProxy = new WebProxy(ProxyServerAddress),
                    HttpProxy = new WebProxy(ProxyServerAddress)
                }))
            {
                await serviceClient.OpenAsync().ConfigureAwait(false);
                DeviceStreamRequest deviceStreamRequest = new DeviceStreamRequest(
                    streamName: "TestStream"
                );
                TestDevice testDevice = await TestDevice.GetTestDeviceAsync(DevicePrefix).ConfigureAwait(false);

                try
                {
                    DeviceStreamResponse result = await serviceClient.CreateStreamAsync(testDevice.Id, deviceStreamRequest).ConfigureAwait(false);
                }
                catch (CE.DeviceNotFoundException ex)
                {
                    Assert.AreEqual(CE.ErrorCode.DeviceTimeout, ex.Code, ex.Message);
                }
                catch
                {
                    throw;
                }
            }
        }

        #endregion Service Client Tests

        #region Private Methods

        private async Task TestDeviceStreamingAsync(TestDeviceType type, ITransportSettings[] deviceTransportSettings, bool acceptRequest)
        {
            await TestDeviceStreamingAsync(type, deviceTransportSettings, TransportType.Amqp, new ServiceClientTransportSettings(), acceptRequest).ConfigureAwait(false);
        }

        private async Task TestDeviceStreamingAsync(TestDeviceType type, TransportType serviceTransportType, ServiceClientTransportSettings serviceTransportSettings, bool acceptRequest)
        {
            Client.AmqpTransportSettings amqpTransportSettings = new Client.AmqpTransportSettings(Client.TransportType.Amqp_Tcp_Only);
            ITransportSettings[] deviceTransportSettings = new ITransportSettings[] { amqpTransportSettings };

            await TestDeviceStreamingAsync(type, deviceTransportSettings, TransportType.Amqp, new ServiceClientTransportSettings(), acceptRequest).ConfigureAwait(false);
        }

        private async Task TestDeviceStreamingAsync(TestDeviceType type, ITransportSettings[] deviceTransportSettings, TransportType serviceTransportType, ServiceClientTransportSettings serviceTransportSettings, bool acceptRequest)
        {
            TestDevice testDevice = await TestDevice.GetTestDeviceAsync(DevicePrefix, type).ConfigureAwait(false);

            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
            using (ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(Configuration.IoTHub.ConnectionString, serviceTransportType, serviceTransportSettings))
            using (DeviceClient deviceClient = testDevice.CreateDeviceClient(deviceTransportSettings))
            {
                await serviceClient.OpenAsync().ConfigureAwait(false);
                await deviceClient.OpenAsync(cts.Token).ConfigureAwait(false);

                Task<ClientDeviceStreamingRequest> clientRequestTask = deviceClient.WaitForDeviceStreamRequestAsync(cts.Token);

                Task<DeviceStreamResponse> serviceRequestTask = serviceClient.CreateStreamAsync(testDevice.Id, new ServiceDeviceStreamingRequest("bla"));

                ClientDeviceStreamingRequest clientRequest = await clientRequestTask.ConfigureAwait(false);

                Assert.IsNotNull(clientRequest, "Received an unexpected null device streaming request");

                _log.WriteLine("Device streaming request received (name=" + clientRequest.Name + "; url=" + clientRequest.Url + "; authToken=" + clientRequest.AuthorizationToken + ")");

                if (acceptRequest)
                {
                    await deviceClient.AcceptDeviceStreamRequestAsync(clientRequest, cts.Token).ConfigureAwait(false);

                    DeviceStreamResponse serviceResponse = await serviceRequestTask.ConfigureAwait(false);

                    Assert.IsNotNull(serviceResponse, "Received an unexpected null device streaming response");

                    _log.WriteLine("Device streaming response received (name=" + serviceResponse.StreamName + "; accepted=" + serviceResponse.IsAccepted + "; url=" + serviceResponse.Url + "; authToken=" + serviceResponse.AuthorizationToken + ")");

                    Assert.IsTrue(serviceResponse.IsAccepted, "Service expected Device Streaming respose with IsAccepted true, but got false");

                    await TestEchoThroughStreamingGatewayAsync(clientRequest, serviceResponse, cts).ConfigureAwait(false);
                }
                else
                {
                    await deviceClient.RejectDeviceStreamRequestAsync(clientRequest, cts.Token).ConfigureAwait(false);

                    DeviceStreamResponse serviceResponse = await serviceRequestTask.ConfigureAwait(false);

                    Assert.IsNotNull(serviceResponse, "Received an unexpected null device streaming response");

                    _log.WriteLine("Device streaming response received (name=" + serviceResponse.StreamName + "; accepted=" + serviceResponse.IsAccepted + "; url=" + serviceResponse.Url + "; auth_token=" + serviceResponse.AuthorizationToken + ")");

                    Assert.IsFalse(serviceResponse.IsAccepted, "Service expected Device Streaming respose with IsAccepted false, but got true");
                }

                await serviceClient.CloseAsync().ConfigureAwait(false);
                await deviceClient.CloseAsync().ConfigureAwait(false);
            }
        }

        private async Task TestModuleStreamingAsync(TestDeviceType type, ITransportSettings[] transportSettings, bool acceptRequest)
        {
            TestModule testModule = await TestModule.GetTestModuleAsync(DevicePrefix, ModulePrefix).ConfigureAwait(false);

            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
            using (ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(Configuration.IoTHub.ConnectionString))
            using (ModuleClient moduleClient = ModuleClient.CreateFromConnectionString(testModule.ConnectionString, transportSettings))
            {
                await serviceClient.OpenAsync().ConfigureAwait(false);
                await moduleClient.OpenAsync(cts.Token).ConfigureAwait(false);

                Task<ClientDeviceStreamingRequest> clientRequestTask = moduleClient.WaitForDeviceStreamRequestAsync(cts.Token);

                Task<DeviceStreamResponse> serviceRequestTask = serviceClient.CreateStreamAsync(testModule.DeviceId, testModule.Id, new ServiceDeviceStreamingRequest("bla"));

                ClientDeviceStreamingRequest clientRequest = await clientRequestTask.ConfigureAwait(false);

                Assert.IsNotNull(clientRequest, "Received an unexpected null device streaming request");

                _log.WriteLine("Device streaming request received (name=" + clientRequest.Name + "; url=" + clientRequest.Url + "; authToken=" + clientRequest.AuthorizationToken + ")");

                if (acceptRequest)
                {
                    await moduleClient.AcceptDeviceStreamRequestAsync(clientRequest, cts.Token).ConfigureAwait(false);

                    DeviceStreamResponse serviceResponse = await serviceRequestTask.ConfigureAwait(false);

                    Assert.IsNotNull(serviceResponse, "Received an unexpected null device streaming response");

                    _log.WriteLine("Device streaming response received (name=" + serviceResponse.StreamName + "; accepted=" + serviceResponse.IsAccepted + "; url=" + serviceResponse.Url + "; authToken=" + serviceResponse.AuthorizationToken + ")");

                    Assert.IsTrue(serviceResponse.IsAccepted, "Service expected Device Streaming respose with IsAccepted true, but got false");

                    await TestEchoThroughStreamingGatewayAsync(clientRequest, serviceResponse, cts).ConfigureAwait(false);
                }
                else
                {
                    await moduleClient.RejectDeviceStreamRequestAsync(clientRequest, cts.Token).ConfigureAwait(false);

                    DeviceStreamResponse serviceResponse = await serviceRequestTask.ConfigureAwait(false);

                    Assert.IsNotNull(serviceResponse, "Received an unexpected null device streaming response");

                    _log.WriteLine("Device streaming response received (name=" + serviceResponse.StreamName + "; accepted=" + serviceResponse.IsAccepted + "; url=" + serviceResponse.Url + "; authToken=" + serviceResponse.AuthorizationToken + ")");

                    Assert.IsFalse(serviceResponse.IsAccepted, "Service expected Device Streaming respose with IsAccepted false, but got true");
                }

                await serviceClient.CloseAsync().ConfigureAwait(false);
                await moduleClient.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<ClientWebSocket> GetStreamingClientAsync(Uri url, string authorizationToken, CancellationToken cancellationToken)
        {
            ClientWebSocket wsClient = new ClientWebSocket();
            wsClient.Options.SetRequestHeader("Authorization", "Bearer " + authorizationToken);

            await wsClient.ConnectAsync(url, cancellationToken).ConfigureAwait(false);

            return wsClient;
        }

        private async Task TestEchoThroughStreamingGatewayAsync(ClientDeviceStreamingRequest clientRequest, DeviceStreamResponse serviceResponse, CancellationTokenSource cts)
        {
            Task<ClientWebSocket> deviceWSClientTask = GetStreamingClientAsync(clientRequest.Url, clientRequest.AuthorizationToken, cts.Token);
            Task<ClientWebSocket> serviceWSClientTask = GetStreamingClientAsync(serviceResponse.Url, serviceResponse.AuthorizationToken, cts.Token);

            await Task.WhenAll(deviceWSClientTask, serviceWSClientTask).ConfigureAwait(false);

            ClientWebSocket deviceWSClient = deviceWSClientTask.Result;
            ClientWebSocket serviceWSClient = serviceWSClientTask.Result;

            byte[] serviceBuffer = Encoding.ASCII.GetBytes("This is a test message !!!@#$@$423423\r\n");
            byte[] clientBuffer = new byte[serviceBuffer.Length];

            await Task.WhenAll(
                serviceWSClient.SendAsync(new ArraySegment<byte>(serviceBuffer), WebSocketMessageType.Binary, true, cts.Token),
                deviceWSClient.ReceiveAsync(new ArraySegment<byte>(clientBuffer), cts.Token).ContinueWith((wsrr) => {
                    Assert.AreEqual(wsrr.Result.Count, serviceBuffer.Length, "Number of bytes received by device WS client is different than sent by service WS client");
                    Assert.IsTrue(clientBuffer.SequenceEqual(serviceBuffer), "Content received by device WS client is different than sent by service WS client");
                }, TaskScheduler.Current)
            ).ConfigureAwait(false);

            await Task.WhenAll(
                deviceWSClient.SendAsync(new ArraySegment<byte>(clientBuffer), WebSocketMessageType.Binary, true, cts.Token),
                serviceWSClient.ReceiveAsync(new ArraySegment<byte>(serviceBuffer), cts.Token).ContinueWith((wsrr) => {
                    Assert.AreEqual(wsrr.Result.Count, serviceBuffer.Length, "Number of bytes received by service WS client is different than sent by device WS client");
                    Assert.IsTrue(clientBuffer.SequenceEqual(serviceBuffer), "Content received by service WS client is different than sent by device WS client");
                }, TaskScheduler.Current)
            ).ConfigureAwait(false);

            await Task.WhenAll(
                deviceWSClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "End of test", cts.Token),
                serviceWSClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "End of test", cts.Token)
            ).ConfigureAwait(false);

            deviceWSClient.Dispose();
            serviceWSClient.Dispose();
        }
#endregion Private Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}