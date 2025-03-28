using Microsoft.AspNetCore.Mvc;
using DotNext.Net.Cluster.Consensus.Raft;
using System.Globalization;
using DotNext;

[Route("api/[controller]")]
[ApiController]
public class RaftController : ControllerBase
{
    private readonly IRaftCluster _cluster;
    private readonly ISupplier<long> _provider;

    public RaftController(IRaftCluster cluster, ISupplier<long> provider)
    {
        _cluster = cluster;
        _provider = provider;
    }

    [HttpGet("leader")]
    public IActionResult GetLeader()
    {
        return Ok($"Leader address is {_cluster.Leader?.EndPoint}. Current address is {HttpContext.Connection.LocalIpAddress}:{HttpContext.Connection.LocalPort}");
    }

    [HttpGet("value")]
    public async Task<IActionResult> GetValue(CancellationToken cancellationToken)
    {
        if (_cluster.Leader is null)
        {
            return StatusCode(503, "Leader node is not yet elected. Please try again later.");
        }

        await _cluster.ApplyReadBarrierAsync(cancellationToken);
        return Ok(_provider.Invoke().ToString(CultureInfo.InvariantCulture));

    }
}
