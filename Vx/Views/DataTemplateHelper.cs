using System;
namespace Vx.Views
{
    public static class DataTemplateHelper
    {
        /// <summary>
        /// Caller needs to handle calling render to native on component if hasn't been rendered natively yet
        /// </summary>
        /// <param name="data"></param>
        /// <param name="template"></param>
        /// <param name="recycledNativeView"></param>
        /// <returns></returns>
        public static bool ProcessAndIsNewComponent(object data, Func<object, View> template, INativeComponent recycledNativeView, out VxComponent newComponent)
        {
            if (recycledNativeView?.Component is VxDataTemplateComponent recycledComponent)
            {
                recycledComponent.Data = data;
                recycledComponent.Template = template;
                newComponent = null;
                return false;
            }

            newComponent = new VxDataTemplateComponent
            {
                Data = data,
                Template = template
            };
            return true;
        }

        public class VxDataTemplateComponent : VxComponent
        {
            public object Data { get => GetState<object>(); set => SetState(value); }

            public Func<object, View> Template { get => GetState<Func<object, View>>(); set => SetState(value); }

            protected override View Render()
            {
                if (Template == null || Data == null)
                {
                    return null;
                }

                return Template(Data);
            }
        }
    }
}
