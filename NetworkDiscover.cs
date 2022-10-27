using System.Net;
using Mirror;
using Mirror.Discovery;
using UnityEngine.Events;
using System;

// client sunucuya yolluyor
public class DiscoveryRequest : NetworkMessage
{
    
}


// server cliente yolluyor
public class DiscoveryResponse : NetworkMessage
{
    public IPEndPoint EndPoint { get; set; }
    public Uri uri;
    public long serverId;

    public string HostName;
    public int maxPlayer;
    public int currentPlayer;
}

[Serializable]
public class ServerFoundUnityEvent : UnityEvent<DiscoveryResponse> { };

public class NetworkDiscover : NetworkDiscoveryBase<DiscoveryRequest, DiscoveryResponse>
{
    public ServerFoundUnityEvent OnServerFound;
    public long ServerId { get; private set; }

    public Transport transport;

    #region Server

    public override void Start()
    {
        ServerId = RandomLong();
        if (transport == null)
            transport = Transport.activeTransport;

    }

    /// <summary>
    /// Reply to the client to inform it of this server
    /// </summary>
    /// <remarks>
    /// Override if you wish to ignore server requests based on
    /// custom criteria such as language, full server game mode or difficulty
    /// </remarks>
    /// <param name="request">Request coming from client</param>
    /// <param name="endpoint">Address of the client that sent the request</param>
    protected override void ProcessClientRequest(DiscoveryRequest request, IPEndPoint endpoint)
    {
        // sunucu, cliente haber veriyor
        base.ProcessClientRequest(request, endpoint);
    }

    /// <summary>
    /// Process the request from a client
    /// </summary>
    /// <remarks>
    /// Override if you wish to provide more information to the clients
    /// such as the name of the host player
    /// </remarks>
    /// <param name="request">Request coming from client</param>
    /// <param name="endpoint">Address of the client that sent the request</param>
    /// <returns>A message containing information about this server</returns>
    protected override DiscoveryResponse ProcessRequest(DiscoveryRequest request, IPEndPoint endpoint) 
    {
        // client'den gelen isteği işle
        DiscoveryResponse x = new DiscoveryResponse();
        x.HostName = PlayerData.singleton.playername;
        x.serverId = ServerId;
        x.uri = transport.ServerUri();
        x.maxPlayer = NetworkManager.singleton.maxConnections;
        x.currentPlayer = NetworkServer.connections.Count;

        return x;
    }

    #endregion

    #region Client

    /// <summary>
    /// Create a message that will be broadcasted on the network to discover servers
    /// </summary>
    /// <remarks>
    /// Override if you wish to include additional data in the discovery message
    /// such as desired game mode, language, difficulty, etc... </remarks>
    /// <returns>An instance of ServerRequest with data to be broadcasted</returns>
    protected override DiscoveryRequest GetRequest()
    {
        // sunucu keşfetmek için bir paket yolla
        return new DiscoveryRequest();
    }

    /// <summary>
    /// Process the answer from a server
    /// </summary>
    /// <remarks>
    /// A client receives a reply from a server, this method processes the
    /// reply and raises an event
    /// </remarks>
    /// <param name="response">Response that came from the server</param>
    /// <param name="endpoint">Address of the server that replied</param>
    protected override void ProcessResponse(DiscoveryResponse response, IPEndPoint endpoint) 
    {
        // sunucudan gelen yanıtı işle
        response.EndPoint = endpoint;

        UriBuilder realUri = new UriBuilder(response.uri)
        {
            Host = response.EndPoint.Address.ToString()
        };
        response.uri = realUri.Uri;

        // eventi oyandır
        OnServerFound.Invoke(response);
    }

    #endregion
}
