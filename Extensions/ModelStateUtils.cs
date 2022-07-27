using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace VLO_BOARDS.Extensions;

public static class ModelStateUtils
{
    private static ValidationProblemDetails GenProblem(int code, ModelStateDictionary modelState)
    {
        return new ValidationProblemDetails(modelState) {Status = code};
    }

    public static BadRequestObjectResult GenBadRequestProblem(this ControllerBase controller)
    {
        return controller.BadRequest(GenProblem(StatusCodes.Status400BadRequest, controller.ModelState));
    }

    public static UnprocessableEntityObjectResult GenUnprocessableProblem(this ControllerBase controller)
    {
        return controller.UnprocessableEntity(GenProblem(StatusCodes.Status422UnprocessableEntity, controller.ModelState));
    }

    public static ObjectResult GenInternalError(this ControllerBase controller)
    {
        return controller.StatusCode(StatusCodes.Status500InternalServerError, GenProblem(StatusCodes.Status500InternalServerError, controller.ModelState));
    }

    public static ObjectResult GenLockedProblem(this ControllerBase controller)
    {
        return controller.StatusCode(StatusCodes.Status423Locked, GenProblem(StatusCodes.Status423Locked, controller.ModelState));
    }
}