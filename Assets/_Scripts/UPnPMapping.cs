using System;
using System.Threading;
//using NATUPNPLib;
//
//namespace iSpyApplication
//{
//    public static class NATControl
//    {
//        public static UPnPNAT NAT = new UPnPNAT();
//        private static IStaticPortMappingCollection _mappings;
//
//        public static IStaticPortMappingCollection Mappings
//        {
//            get
//            {
//                if (_mappings == null)
//                {
//
//                    try
//                    {
//                        if (NAT.NATEventManager != null)
//                            _mappings = NAT.StaticPortMappingCollection;
//                    }
//                    catch {}
//                }
//
//                return _mappings;
//            }
//        }
//
//        public static bool SetPorts(int wanPort, int lanPort)
//        {
//            bool b = false;
//            int i = 3;
//            while (Mappings == null && i > 0)
//            {
//                Thread.Sleep(2000);
//                i--;
//            }
//
//            if (Mappings != null)
//            {
//                try
//                {
//                    Mappings.Remove(wanPort, "TCP");
//                }
//                catch (Exception ex)
//                {
//                    // do something
//                }
//                try
//                {
//                    Mappings.Add(wanPort, "TCP", lanPort, internalIP, true, "iSpy");
//                    b = true;
//                }
//                catch (Exception ex)
//                {
//                    // do something
//                }
//            }
//
//            return b;
//
//        } // method
//
//    } // class    
//
//} // namespace