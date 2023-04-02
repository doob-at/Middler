using doob.Middler.Common.SharedModels.Interfaces;
using doob.Middler.Common.SharedModels.Models;

namespace doob.Middler.Core.ExtensionMethods {
    public static class MiddlerActionExtensions {

        //public static MiddlerAction ToBasicMiddlerAction<T>(this MiddlerAction<T> middlerAction) where T : class, new()
        //{
        //    var act = new MiddlerAction
        //    {
        //        Terminating = middlerAction.Terminating,
        //        WriteStreamDirect = middlerAction.WriteStreamDirect,
        //        ActionType = middlerAction.ActionType,
        //        Parameters = Converter.Json.ToJObject(middlerAction.Parameters)
        //    };

        //    return act;
        //}

        public static MiddlerAction? ToBasicMiddlerAction(this IMiddlerAction middlerAction)
        {
            return Converter.Json.ToObject<MiddlerAction>(middlerAction);
        }

    }
}
