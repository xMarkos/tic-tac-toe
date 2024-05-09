using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;

namespace Markos.TicTacToe.MVC;

internal class InternalControllerProvider : ControllerFeatureProvider
{
    protected override bool IsController(TypeInfo typeInfo)
    {
        if (typeInfo.IsPublic || !typeInfo.IsClass || typeInfo.IsAbstract || typeInfo.ContainsGenericParameters || typeInfo.IsDefined(typeof(NonControllerAttribute)))
            return false;

        if (!typeInfo.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase) &&
            !typeInfo.IsDefined(typeof(ControllerAttribute)))
        {
            return false;
        }

        return true;
    }
}
