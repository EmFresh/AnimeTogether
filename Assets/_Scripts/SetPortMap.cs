using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPortMap : MonoBehaviour
{

   
    public void portMap()
    {
        try
        {
            CreatePopups.SendPopup(UPnP.NAT.Discover());
            CreatePopups.SendPopup("You have an UPnP-enabled router and your IP is: " + UPnP.NAT.GetExternalIP());
        }
        catch
        {
            CreatePopups.SendPopup("You do not have an UPnP-enabled router.");
        }
    }
}