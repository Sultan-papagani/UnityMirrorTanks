using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkDiscoverHud : MonoBehaviour
{
    readonly Dictionary<long, DiscoveryResponse> discoveredServers = new Dictionary<long, DiscoveryResponse>();
    public NetworkDiscover networkDiscovery;
    public Transform ServerViewport;
    public GameObject buttonPrefab;
    public TanksCanvas tanksCanvas;


#if UNITY_EDITOR
    void OnValidate()
    {
        if (networkDiscovery == null)
        {
            networkDiscovery = GetComponent<NetworkDiscover>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(networkDiscovery.OnServerFound, OnDiscoveredServer);
            UnityEditor.Undo.RecordObjects(new UnityEngine.Object[] { this, networkDiscovery }, "Set NetworkDiscovery");
        }
    }
#endif


    public void HostaBaglan(DiscoveryResponse info)
    {
        networkDiscovery.StopDiscovery();
        NetworkManager.singleton.StartClient(info.uri);
    }

    // var olan oyunları ara
    public void HostAra()
    {
        discoveredServers.Clear();
        networkDiscovery.StartDiscovery();
    }

    // bir oyun oluştur
    public void HostBaslat()
    {
        discoveredServers.Clear();
        NetworkManager.singleton.maxConnections = tanksCanvas.PlayerCountMax;
        NetworkManager.singleton.StartHost();
        networkDiscovery.AdvertiseServer();
    }


    public void OnDiscoveredServer(DiscoveryResponse info)
    {
        discoveredServers[info.serverId] = info;

        Transform x;
        for (int i = 0; i < ServerViewport.childCount; i++)
        {
            x = ServerViewport.GetChild(i);
            x.GetComponent<FoundServerButton>().button.onClick.RemoveAllListeners();
            Destroy(x.gameObject);
        }

        foreach (DiscoveryResponse d in discoveredServers.Values)
        {
            DiscoveryResponse cache = d;

            GameObject button = Instantiate(buttonPrefab);
            FoundServerButton c = button.GetComponent<FoundServerButton>();
            c.Setup(d.HostName, d.maxPlayer, d.currentPlayer, ServerViewport);
            c.button.onClick.AddListener(() => HostaBaglan(cache));
        }

    }

   
}
