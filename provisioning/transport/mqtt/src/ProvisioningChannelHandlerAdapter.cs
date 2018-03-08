﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Buffers;
using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Transport.Channels;
using Microsoft.Azure.Devices.Provisioning.Client.Transport.Models;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Provisioning.Client.Transport
{
    internal class ProvisioningChannelHandlerAdapter : ChannelHandlerAdapter
    {
        private const string ExceptionPrefix = "MQTT Protocol Exception:";
        private const QualityOfService Qos = QualityOfService.AtLeastOnce;
        private const string UsernameFormat = "{0}/registrations/{1}/api-version={2}&ClientVersion={3}";
        private const string SubscribeFilter = "$dps/registrations/res/#";
        private const string RegisterTopic = "$dps/registrations/PUT/iotdps-register/?$rid={0}";
        private const string GetOperationsTopic = "$dps/registrations/GET/iotdps-get-operationstatus/?$rid={0}&operationId={1}";

        private static readonly TimeSpan s_defaultOperationPoolingIntervalMilliseconds = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan s_regexTimeoutMilliseconds = TimeSpan.FromMilliseconds(500);

        private static readonly Regex s_registrationStatusTopicRegex =
            new Regex("^\\$dps/registrations/res/(.*?)/\\?\\$rid=(.*?)$", RegexOptions.Compiled, s_regexTimeoutMilliseconds);

        private ProvisioningTransportRegisterMessage _message;
        private TaskCompletionSource<RegistrationOperationStatus> _taskCompletionSource;
        private CancellationToken _cancellationToken;
        private int _state;
        private int _packetId = 0;

        internal enum State
        {
            Start,
            Failed,
            WaitForConnack,
            WaitForSuback,
            WaitForPubAck,
            WaitForStatus,
            Done,
        }

        public ProvisioningChannelHandlerAdapter(
            ProvisioningTransportRegisterMessage message,
            TaskCompletionSource<RegistrationOperationStatus> taskCompletionSource,
            CancellationToken cancellationToken)
        {
            _message = message;
            _taskCompletionSource = taskCompletionSource;
            _cancellationToken = cancellationToken;

            ForceState(State.Start);
        }

        #region DotNetty.ChannelHandlerAdapter overrides

        public override async void ChannelActive(IChannelHandlerContext context)
        {
            if (Logging.IsEnabled) Logging.Enter(this, context.Name, nameof(ChannelActive));
            await VerifyCancellationAsync(context).ConfigureAwait(false);

            try
            {
                ChangeState(State.Start, State.WaitForConnack);
                await ConnectAsync(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await FailWithExceptionAsync(context, ex).ConfigureAwait(false);
            }

            base.ChannelActive(context);

            if (Logging.IsEnabled) Logging.Exit(this, context.Name, nameof(ChannelActive));
        }

        public override async void ChannelInactive(IChannelHandlerContext context)
        {
            if (Logging.IsEnabled) Logging.Enter(this, context.Name, nameof(ChannelInactive));
            base.ChannelInactive(context);

            await FailWithExceptionAsync(
                context,
                new ProvisioningTransportException($"{ExceptionPrefix} Channel closed.")).ConfigureAwait(false);

            if (Logging.IsEnabled) Logging.Exit(this, context.Name, nameof(ChannelInactive));
        }

        public async override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (Logging.IsEnabled) Logging.Enter(this, context.Name, nameof(ChannelRead));
            Debug.Assert(message is Packet);
            await VerifyCancellationAsync(context).ConfigureAwait(false);

            await ProcessMessageAsync(context, (Packet)message).ConfigureAwait(false);

            base.ChannelRead(context, message);
            if (Logging.IsEnabled) Logging.Exit(this, context.Name, nameof(ChannelRead));
        }

        public async override void ChannelReadComplete(IChannelHandlerContext context)
        {
            if (Logging.IsEnabled) Logging.Enter(this, context.Name, nameof(ChannelReadComplete));
            await VerifyCancellationAsync(context).ConfigureAwait(false);

            base.ChannelReadComplete(context);

            if (Logging.IsEnabled) Logging.Exit(this, context.Name, nameof(ChannelReadComplete));
        }

        public override async void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            if (Logging.IsEnabled) Logging.Enter(this, $"{nameof(ExceptionCaught)}({exception.ToString()}");
            base.ExceptionCaught(context, exception);

            await FailWithExceptionAsync(context, exception).ConfigureAwait(false);
            if (Logging.IsEnabled) Logging.Exit(this, "", nameof(ExceptionCaught));
        }

        #endregion

        private Task ConnectAsync(IChannelHandlerContext context)
        {
            string registrationId = _message.Security.GetRegistrationID();
            string userAgent = _message.ProductInfo;

            var message = new ConnectPacket()
            {
                CleanSession = true,
                ClientId = registrationId,
                HasWill = false,
                HasUsername = true,
                Username = string.Format(
                    CultureInfo.InvariantCulture,
                    UsernameFormat,
                    _message.IdScope,
                    registrationId,
                    ClientApiVersionHelper.ApiVersion,
                    Uri.EscapeDataString(userAgent)),
                HasPassword = false,
            };

            return context.WriteAndFlushAsync(message);
        }

        private async Task ProcessMessageAsync(IChannelHandlerContext context, Packet message)
        {
            State currentState = (State)Volatile.Read(ref _state);

            switch (currentState)
            {
                case State.Start:
                    Debug.Fail($"{nameof(ProvisioningChannelHandlerAdapter)}: Invalid state: {nameof(State.Start)}");
                    break;
                case State.Done:
                    Debug.Fail($"{nameof(ProvisioningChannelHandlerAdapter)}: Invalid state: {nameof(State.Done)}");
                    break;
                case State.Failed:
                    Debug.Fail($"{nameof(ProvisioningChannelHandlerAdapter)}: Invalid state: {nameof(State.Failed)}");
                    break;
                case State.WaitForConnack:
                    await VerifyExpectedPacketType(context, PacketType.CONNACK, message).ConfigureAwait(false);
                    await ProcessConnAckAsync(context, (ConnAckPacket)message).ConfigureAwait(false);
                    break;
                case State.WaitForSuback:
                    await VerifyExpectedPacketType(context, PacketType.SUBACK, message).ConfigureAwait(false);
                    await ProcessSubAckAsync(context, (SubAckPacket)message).ConfigureAwait(false);
                    break;
                case State.WaitForPubAck:
                    ChangeState(State.WaitForPubAck, State.WaitForStatus);
                    await VerifyExpectedPacketType(context, PacketType.PUBACK, message).ConfigureAwait(false);
                    break;
                case State.WaitForStatus:
                    await VerifyExpectedPacketType(context, PacketType.PUBLISH, message).ConfigureAwait(false);
                    await ProcessRegistrationStatusAsync(context, (PublishPacket)message).ConfigureAwait(false);
                    break;
                default:
                    await FailWithExceptionAsync(
                        context,
                        new ProvisioningTransportException(
                            $"{ExceptionPrefix} Invalid state: {(State)_state}")).ConfigureAwait(false);
                    break;
            }
        }

        private async Task ProcessConnAckAsync(IChannelHandlerContext context, ConnAckPacket packet)
        {
            if (packet.SessionPresent)
            {
                await FailWithExceptionAsync(
                    context,
                    new ProvisioningTransportException(
                        $"{ExceptionPrefix} Unexpected CONNACK with SessionPresent.")).ConfigureAwait(false);
            }

            switch (packet.ReturnCode)
            {
                case ConnectReturnCode.Accepted:
                    try
                    {
                        ChangeState(State.WaitForConnack, State.WaitForSuback);
                        await SubscribeAsync(context).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        await FailWithExceptionAsync(context, ex).ConfigureAwait(false);
                    }

                    break;

                case ConnectReturnCode.RefusedUnacceptableProtocolVersion:
                case ConnectReturnCode.RefusedIdentifierRejected:
                case ConnectReturnCode.RefusedBadUsernameOrPassword:
                case ConnectReturnCode.RefusedNotAuthorized:
                    await FailWithExceptionAsync(
                        context,
                        new ProvisioningTransportException(
                            $"{ExceptionPrefix} CONNACK failed with {packet.ReturnCode}")).ConfigureAwait(false);
                    break;

                case ConnectReturnCode.RefusedServerUnavailable:
                    await FailWithExceptionAsync(
                        context,
                        new ProvisioningTransportException(
                            $"{ExceptionPrefix} CONNACK failed with {packet.ReturnCode}. Try again later.",
                            null,
                            true)).ConfigureAwait(false);
                    break;

                default:
                    await FailWithExceptionAsync(
                        context,
                        new ProvisioningTransportException(
                            $"{ExceptionPrefix} CONNACK failed unknown return code: {packet.ReturnCode}")).ConfigureAwait(false);
                    break;
            }
        }

        private Task SubscribeAsync(IChannelHandlerContext context)
        {
            var message = new SubscribePacket(GetNextPacketId(), new SubscriptionRequest(SubscribeFilter, Qos));
            return context.WriteAndFlushAsync(message);
        }

        private async Task ProcessSubAckAsync(IChannelHandlerContext context, SubAckPacket packet)
        {
            if (packet.PacketId == GetCurrentPacketId())
            {
                try
                {
                    ChangeState(State.WaitForSuback, State.WaitForPubAck);
                    await PublishRegisterAsync(context).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await FailWithExceptionAsync(context, ex).ConfigureAwait(false);
                }
            }
        }

        private Task PublishRegisterAsync(IChannelHandlerContext context)
        {
            int packetId = GetNextPacketId();

            var message = new PublishPacket(Qos, false, false)
            {
                TopicName = string.Format(CultureInfo.InvariantCulture, RegisterTopic, packetId),
                PacketId = packetId,
                Payload = Unpooled.Empty,
            };

            return context.WriteAndFlushAsync(message);
        }

        private async Task VerifyPublishPacketTopicAsync(IChannelHandlerContext context, string topicName, string jsonData)
        {
            try
            {
                Match match = s_registrationStatusTopicRegex.Match(topicName);

                if(match.Groups.Count >= 2)
                {
                    if(Enum.TryParse(match.Groups[1].Value, out HttpStatusCode statusCode))
                    {
                        if (statusCode >= HttpStatusCode.BadRequest)
                        {
                            try
                            {
                                var errorDetails = JsonConvert.DeserializeObject<ProvisioningErrorDetails>(jsonData);

                                bool isTransient = statusCode >= HttpStatusCode.InternalServerError || (int)statusCode == 429;
                                await FailWithExceptionAsync(
                                     context,
                                     new ProvisioningTransportException(
                                         errorDetails.CreateMessage($"{ExceptionPrefix} Server Error: {match.Groups[1].Value}"),
                                         null,
                                         isTransient,
                                         errorDetails.TrackingId)).ConfigureAwait(false);
                            }
                            catch (JsonException ex)
                            {
                                if (Logging.IsEnabled) Logging.Error(
                                    this,
                                    $"{nameof(ProvisioningTransportHandlerMqtt)} server returned malformed error response." +
                                    $"Parsing error: {ex}. Server response: {jsonData}",
                                    nameof(VerifyPublishPacketTopicAsync));

                                await FailWithExceptionAsync(
                                    context,
                                    new ProvisioningTransportException(
                                        $"{ExceptionPrefix} Malformed server error message: '{jsonData}'",
                                        ex,
                                        false)).ConfigureAwait(false);
                            }
                        }
                    }
                }
                else
                {
                    await FailWithExceptionAsync(
                                 context,
                                 new ProvisioningTransportException(
                                     $"{ExceptionPrefix} Unexpected server response. TopicName invalid: '{topicName}'",
                                     null,
                                     false)).ConfigureAwait(false);
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                await FailWithExceptionAsync(context, e).ConfigureAwait(false);
            }
        }

        private async Task ProcessRegistrationStatusAsync(IChannelHandlerContext context, PublishPacket packet)
        {
            string operationId = null;

            try // TODO : extract generic method for exception handling.
            {
                await PubAckAsync(context, packet.PacketId).ConfigureAwait(false);

                string jsonData = Encoding.UTF8.GetString(
                    packet.Payload.GetIoBuffer().Array,
                    packet.Payload.GetIoBuffer().Offset,
                    packet.Payload.GetIoBuffer().Count);

                await VerifyPublishPacketTopicAsync(context, packet.TopicName, jsonData).ConfigureAwait(false);

                //"{\"operationId\":\"0.indcertdevice1.e50c0fa7-8b9b-4b3d-8374-02d71377886f\",\"status\":\"assigning\"}"
                var operation = JsonConvert.DeserializeObject<RegistrationOperationStatus>(jsonData);
                operationId = operation.OperationId;

                if (string.CompareOrdinal(operation.Status, RegistrationOperationStatus.OperationStatusAssigning) == 0 ||
                    string.CompareOrdinal(operation.Status, RegistrationOperationStatus.OperationStatusUnassigned) == 0)
                {
                    await Task.Delay(s_defaultOperationPoolingIntervalMilliseconds).ConfigureAwait(false);
                    ChangeState(State.WaitForStatus, State.WaitForPubAck);
                    await PublishGetOperationAsync(context, operationId).ConfigureAwait(false);
                }
                else
                {
                    ChangeState(State.WaitForStatus, State.Done);
                    _taskCompletionSource.TrySetResult(operation);

                    await this.DoneAsync(context).ConfigureAwait(false);
                }
            }
            catch (ProvisioningTransportException te)
            {
                await FailWithExceptionAsync(context, te).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var wrapperEx = new ProvisioningTransportException(
                    $"{ExceptionPrefix} Error while processing RegistrationStatus.",
                    ex,
                    false);

                await FailWithExceptionAsync(context, wrapperEx).ConfigureAwait(false);
            }
        }

        private Task PubAckAsync(IChannelHandlerContext context, int packetId)
        {
            var message = new PubAckPacket()
            {
                PacketId = packetId,
            };

            return context.WriteAndFlushAsync(message);
        }


        private Task PublishGetOperationAsync(IChannelHandlerContext context, string operationId)
        {
            int packetId = GetNextPacketId();

            var message = new PublishPacket(Qos, false, false)
            {
                TopicName = string.Format(CultureInfo.InvariantCulture, GetOperationsTopic, packetId, operationId),
                PacketId = packetId,
                Payload = Unpooled.Empty,
            };

            return context.WriteAndFlushAsync(message);
        }

        private async Task VerifyExpectedPacketType(IChannelHandlerContext context, PacketType expectedPacketType, Packet message)
        {
            if (message.PacketType != expectedPacketType)
            {
                await FailWithExceptionAsync(
                    context,
                    new ProvisioningTransportException(
                        $"{ExceptionPrefix} Received unexpected packet type {message.PacketType} in state {(State)_state}")).ConfigureAwait(false);
            }
        }

        private async Task FailWithExceptionAsync(IChannelHandlerContext context, Exception ex)
        {
            if (Volatile.Read(ref _state) != (int)State.Failed)
            {
                ForceState(State.Failed);
                _taskCompletionSource.TrySetException(ex);

                await context.CloseAsync().ConfigureAwait(false);
            }
        }

        private async Task VerifyCancellationAsync(IChannelHandlerContext context)
        {
            if (_cancellationToken.IsCancellationRequested &&
                (Volatile.Read(ref _state) != (int)State.Failed))
            {
                ForceState(State.Failed);
                _taskCompletionSource.TrySetCanceled(_cancellationToken);

                await context.CloseAsync().ConfigureAwait(false);
            }
        }

        private void ChangeState(State expectedCurrentState, State newState)
        {
            if (Logging.IsEnabled) Logging.Info(this, $"{nameof(ChangeState)}: {expectedCurrentState} -> {newState}");

            int currentState = Interlocked.CompareExchange(ref _state, (int)newState, (int)expectedCurrentState);

            if (currentState != (int)expectedCurrentState)
            {
                string newStateString = Enum.GetName(typeof(State), newState);
                string currentStateString = Enum.GetName(typeof(State), currentState);
                string expectedStateString = Enum.GetName(typeof(State), expectedCurrentState);

                var exception = new ProvisioningTransportException(
                    $"{ExceptionPrefix} Unexpected state transition to {newStateString} from {currentStateString}. " +
                    $"Expecting {expectedStateString}");

                ForceState(State.Failed);
                _taskCompletionSource.TrySetException(exception);
            }
        }

        private void ForceState(State newState)
        {
            if (Logging.IsEnabled) Logging.Info(this, $"{nameof(ForceState)}: {(State)_state} -> {newState}");
            Volatile.Write(ref _state, (int)newState);
        }

        private async Task DoneAsync(IChannelHandlerContext context)
        {
            if (Logging.IsEnabled) Logging.Enter(this, context.Name, $"{nameof(DoneAsync)}");

            try
            {
                await context.Channel.WriteAndFlushAsync(DisconnectPacket.Instance).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (Logging.IsEnabled) Logging.Info(this, $"Exception trying to send disconnect packet: {e.ToString()}");
                await FailWithExceptionAsync(context, e).ConfigureAwait(false);
            }

            // This delay is required to work-around a .NET Framework CloseAsync bug.
            if (Logging.IsEnabled) Logging.Info(this, $"{nameof(DoneAsync)}: Applying close channel delay.");
            await Task.Delay(TimeSpan.FromMilliseconds(400)).ConfigureAwait(false);

            if (Logging.IsEnabled) Logging.Info(this, $"{nameof(DoneAsync)}: Closing channel.");

            try
            {
                await context.Channel.CloseAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (Logging.IsEnabled) Logging.Info(this, $"Exception trying to close channel: {e.ToString()}");
                await FailWithExceptionAsync(context, e).ConfigureAwait(false);
            }

            if (Logging.IsEnabled) Logging.Exit(this, context.Name, $"{nameof(DoneAsync)}");
        }

        private ushort GetNextPacketId()
        {
            unchecked
            {
                ushort newIdShort;
                int newId = Interlocked.Increment(ref _packetId);

                newIdShort = (ushort)newId;
                if (newIdShort == 0) return GetNextPacketId();

                return newIdShort;
            }
        }

        private ushort GetCurrentPacketId()
        {
            unchecked
            {
                return (ushort)Volatile.Read(ref _packetId);
            }
        }
    }
}
