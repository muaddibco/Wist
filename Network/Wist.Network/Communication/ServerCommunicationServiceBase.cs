using Wist.Network.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Wist.Core.Logging;
using Wist.Core.Architecture;
using Wist.BlockLattice.Core.Handlers;
using Wist.Network.Topology;

namespace Wist.Network.Communication
{
    public abstract class ServerCommunicationServiceBase : CommunicationServiceBase, IServerCommunicationService
    {
        protected SocketListenerSettings _settingsListener;
        protected Socket _listenSocket;
        protected ICommunicationProvisioning _communicationProvisioning = null;

        /// <summary>
        /// Create an uninitialized server instance.  
        /// To start the server listening for connection requests
        /// call the Init method followed by Start method 
        /// </summary>
        /// <param name="settings">instance of <see cref="SocketListenerSettings"/> with defined settings of listener</param>
        /// <param name="receiveBufferSize">buffer size to use for each socket I/O operation</param>
        public ServerCommunicationServiceBase(IApplicationContext applicationContext, ILoggerService loggerService, IBufferManagerFactory bufferManagerFactory, IPacketsHandler packetsHandler, INodesResolutionService nodesResolutionService)
            : base(applicationContext, loggerService, bufferManagerFactory, packetsHandler, nodesResolutionService)
        {
        }

        #region ICommunicationService implementation

        public override void Init(SocketSettings settings)
        {
            _settingsListener = settings as SocketListenerSettings;

            if (_settingsListener == null)
            {
                throw new ArgumentException($"Argument {nameof(settings)} must be provided with instance of {nameof(SocketListenerSettings)} type");
            }

            base.Init(settings);
        }

        public virtual void InitCommunicationProvisioning(ICommunicationProvisioning communicationProvisioning = null)
        {
            if (_communicationProvisioning != null)
            {
                _communicationProvisioning.AllowedEndpointsChanged -= CommunicationProvisioning_AllowedEndpointsChanged;
            }

            _communicationProvisioning = communicationProvisioning;

            if (_communicationProvisioning != null)
            {
                _communicationProvisioning.AllowedEndpointsChanged += CommunicationProvisioning_AllowedEndpointsChanged;
            }
        }

        public override void Start()
        {
            base.Start();

            _listenSocket = CreateSocket();

            _listenSocket.Bind(_settingsListener.ListeningEndpoint);

            StartAccept();
        }

        //TODO: need to ascertain that this functionality is really needed because it looks very weird
        public void RegisterOnReceivedExtendedValidation(Func<ICommunicationChannel, IPEndPoint, int, bool> onReceiveExtendedValidation)
        {
            foreach (ICommunicationChannel communicationChannel in _communicationChannelsPool)
            {
                communicationChannel.RegisterExtendedValidation(onReceiveExtendedValidation);
            }
        }

        #endregion ICommunicationService implementation

        #region Private functions

        protected abstract void StartAccept();

        private void CommunicationProvisioning_AllowedEndpointsChanged(object sender, EventArgs e)
        {
            List<ICommunicationChannel> clientsToBeDisconnected = _clientConnectedList.Where(c => _communicationProvisioning.AllowedEndpoints.All(ep => !ep.Address.Equals(c.RemoteIPAddress))).ToList();

            foreach (ICommunicationChannel clientHandler in clientsToBeDisconnected)
            {
                clientHandler.Close();
            }

            while (_clientConnectedList.Any(c => clientsToBeDisconnected.Contains(c)))
            {
                Thread.Sleep(100);
            };

            foreach (IPEndPoint endPoint in _communicationProvisioning.AllowedEndpoints.Where(ep => _clientConnectedList.All(cc => !cc.RemoteIPAddress.Equals(ep.Address))))
            {
                ICommunicationChannel communicationChannel = CreateChannel(endPoint);
                //TODO: what's next?
            }
        }

        #endregion Private functions
    }
}
