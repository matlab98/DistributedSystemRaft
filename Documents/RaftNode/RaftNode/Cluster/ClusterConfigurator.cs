using System.Diagnostics;
using DotNext.Net.Cluster;
using DotNext.Net.Cluster.Consensus.Raft;

namespace RaftNode.Cluster
{
    internal sealed class ClusterConfigurator : IClusterMemberLifetime
    {
        /// <summary>
        /// Maneja el evento cuando cambia el líder del clúster.
        /// </summary>
        internal static void LeaderChanged(ICluster cluster, IClusterMember? leader)
        {
            Debug.Assert(cluster is IRaftCluster);
            var term = ((IRaftCluster)cluster).Term;
            var timeout = ((IRaftCluster)cluster).ElectionTimeout;

            Console.WriteLine(leader is null
                ? "❌ No consensus can be reachedo"
                : $"✅ New cluster leader elected. Address: {leader.EndPoint}");
            Console.WriteLine($"📌 Current: {term} | time: {timeout}");
        }

        /// <summary>
        /// Se ejecuta cuando el nodo inicia, agregando la escucha de cambios de líder.
        /// </summary>
        public void OnStart(IRaftCluster cluster, IDictionary<string, string> metadata)
        {
            cluster.LeaderChanged += LeaderChanged;
            Console.WriteLine("🚀 Raft node started and listening for leader changes.");
        }

        /// <summary>
        /// Se ejecuta cuando el nodo se detiene, eliminando la escucha de cambios de líder.
        /// </summary>
        public void OnStop(IRaftCluster cluster)
        {
            cluster.LeaderChanged -= LeaderChanged;
            Console.WriteLine("⏹️ Raft node stopped.");
        }
    }
}
