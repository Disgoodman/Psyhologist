using Microsoft.AspNetCore.Mvc;

namespace Psychologist.Server.Utils;

public static class ControllerExtensions
{
    public static ObjectResult Problem400(this ControllerBase controller, string title) =>
        controller.Problem(title: title, statusCode: 400);
}