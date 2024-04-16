namespace busfy_api.src.Web.Middlewares
{
    public class CustomStatusCodeMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomStatusCodeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == 404)
            {
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync("<img src=\"Resources/criminal.gif\">");
            }
            else if (context.Response.StatusCode == 400)
            {
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync("<img src=\"Resources/bad.gif\">");
            }
        }
    }
}