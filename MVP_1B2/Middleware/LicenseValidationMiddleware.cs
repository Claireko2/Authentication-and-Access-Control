using MVP_1B2.Services;

namespace MVP_1B2.Middleware
{
    public sealed class LicenseValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public LicenseValidationMiddleware(
            RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ILicenseService licenseService)
        {
            if (context.Request.Path
                    .StartsWithSegments("/Identity")
                || context.Request.Path
                    .StartsWithSegments("/License"))
            {
                await _next(context);
                return;
            }

            var result = licenseService.Validate();

            if (!result.IsValid)
            {
                context.Response.Redirect(
                    "/License/Status");

                return;
            }

            await _next(context);
        }
    }
}
