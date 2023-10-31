using Framework.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Framework.Application
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        public BaseController()
        {
        }

        [NonAction]
        protected static async Task<GeneralResponse<T>> WithLoggingGeneralResponseAsync<T>(Func<Task<T>> tryPart)
        {
            var logId = Guid.Empty;

            try
            {
                var res = await tryPart();
                return new GeneralResponse<T>
                {
                    LogId = logId,
                    Data = res,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<T> { ErrorMessage = ex.Message, Success = false };
            }
        }

    }
}