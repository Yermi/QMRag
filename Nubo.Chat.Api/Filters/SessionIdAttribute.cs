using Microsoft.AspNetCore.Mvc.Filters;

namespace QMRagPipline.Api.Filters
{
    public class SessionIdAttribute : ActionFilterAttribute
    {
        private const string HeaderName = "X-Session-Id";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
            var response = context.HttpContext.Response;
            var headers = request.Headers;

            string sessionId;

            if (headers.TryGetValue(HeaderName, out var sessionHeader) &&
                !string.IsNullOrWhiteSpace(sessionHeader))
            {
                sessionId = sessionHeader!;
            }
            else
            {
                sessionId = Guid.NewGuid().ToString();
                response.Headers.Append(HeaderName, sessionId);
            }

            context.HttpContext.Items["SessionId"] = sessionId;

            base.OnActionExecuting(context);
        }
    }
}
