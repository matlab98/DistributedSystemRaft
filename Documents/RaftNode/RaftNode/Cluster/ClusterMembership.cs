using Microsoft.AspNetCore.Connections;

namespace RaftNode.Infrastructure;

public static class ClusterMembership
{
    public static void AddClusterMembers(ICollection<UriEndPoint> members)
    {
        members.Add(new UriEndPoint(new("http://localhost:3262", UriKind.Absolute)));
        members.Add(new UriEndPoint(new("http://localhost:3263", UriKind.Absolute)));
        members.Add(new UriEndPoint(new("http://localhost:3264", UriKind.Absolute)));
    }
}
