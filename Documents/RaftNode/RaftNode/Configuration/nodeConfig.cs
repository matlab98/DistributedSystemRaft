using System;
using System.Net;
using DotNext.Net.Cluster.Consensus.Raft;

namespace RaftNode.Configuration
{
    public class nodeConfig
    {
        public IPEndPoint EndPoint { get; }
        public TimeSpan RequestTimeout { get; }
        public int LowerElectionTimeout { get; }
        public int UpperElectionTimeout { get; }
        public int TransmissionBlockSize { get; }
        public bool ColdStart { get; }

        public nodeConfig(int port)
        {
            EndPoint = new IPEndPoint(IPAddress.Loopback, port);
            RequestTimeout = TimeSpan.FromSeconds(10);
            LowerElectionTimeout = 1500;
            UpperElectionTimeout = 3000;
            TransmissionBlockSize = 4096;
            ColdStart = false;
        }
    }
}
