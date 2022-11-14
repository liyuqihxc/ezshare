using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Stateless;

namespace EZShare.Common.Net
{
    public sealed class FileReceiver
    {
        private enum _States
        {
            Listening,
            ExchangingKey,
            InitializingTransport,
            BeginReceivingFile, ReceivingFile, EndReceivingFile,
            TerminatingTransport,
        }

        private enum _Commands
        {
            InitializeConnection,
            InitializeTransport,

            StartReceivingFile, ReceiveFile, FileReceived,

            CloseConnection,
        }

        private readonly StateMachine<_States, _Commands> _stateMachine;
        private readonly TcpClient _tcpClient;

        internal FileReceiver(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;

            _stateMachine = new StateMachine<_States, _Commands>(_States.Listening);

            _stateMachine.Configure(_States.Listening)
                .OnEntryAsync(InitializeOrResetConnectionAsync)
                .Permit(_Commands.InitializeConnection, _States.ExchangingKey);

            _stateMachine.Configure(_States.ExchangingKey)
                .OnEntryAsync(EstablishConnectionAsync)
                .Permit(_Commands.InitializeTransport, _States.InitializingTransport)
                .Permit(_Commands.CloseConnection, _States.Listening);

            _stateMachine.Configure(_States.InitializingTransport)
                .OnEntryAsync(WaitForTransportationBasicInformationAsync)
                .Permit(_Commands.StartReceivingFile, _States.BeginReceivingFile)
                .Permit(_Commands.CloseConnection, _States.Listening);

            _stateMachine.Configure(_States.BeginReceivingFile)
                .OnEntryAsync(WaitForFileBasicInformationAsync)
                .Permit(_Commands.ReceiveFile, _States.ReceivingFile)
                .Permit(_Commands.CloseConnection, _States.Listening);

            _stateMachine.Configure(_States.ReceivingFile)
                .OnEntryAsync(ReceiveFileAsync)
                .Permit(_Commands.FileReceived, _States.EndReceivingFile)
                .Permit(_Commands.CloseConnection, _States.Listening);

            _stateMachine.Configure(_States.EndReceivingFile)
                .OnEntryAsync(CheckFileIntegrityAsync)
                .Permit(_Commands.StartReceivingFile, _States.BeginReceivingFile)
                .Permit(_Commands.CloseConnection, _States.Listening);

            InitializeOrResetConnectionAsync().Wait();
        }

        private Task InitializeOrResetConnectionAsync()
        {
            throw new NotImplementedException();
        }

        private Task EstablishConnectionAsync()
        {
            throw new NotImplementedException();
        }

        private Task WaitForTransportationBasicInformationAsync()
        {
            throw new NotImplementedException();
        }

        private Task WaitForFileBasicInformationAsync()
        {
            throw new NotImplementedException();
        }

        private Task ReceiveFileAsync()
        {
            throw new NotImplementedException();
        }

        private Task CheckFileIntegrityAsync()
        {
            throw new NotImplementedException();
        }
    }
}
