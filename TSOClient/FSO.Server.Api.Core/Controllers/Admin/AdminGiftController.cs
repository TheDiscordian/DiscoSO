using FSO.Server.Api.Core.Utils;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FSO.Server.Api.Core.Controllers.Admin
{
    [EnableCors("AdminAppPolicy")]
    [Route("admin/gift-money")]
    [ApiController]
    public class AdminGiftController : ControllerBase
    {
        [HttpPost]
        public IActionResult gift(GiftMoneyModel g)
        {
            var api = Api.INSTANCE;
            api.DemandModerator(Request);
            api.GiftMoney(g.avatar_id, g.amount);
            return ApiResponse.Json(HttpStatusCode.OK, true);
        }
    }

    public class GiftMoneyModel
    {
        public uint avatar_id;
        public int amount;
    }
}
