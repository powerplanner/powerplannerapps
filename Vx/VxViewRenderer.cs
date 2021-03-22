using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace Vx
{
    public static class VxViewRenderer
    {
        public static Dictionary<Type, Type> Mappings { get; private set; } = new Dictionary<Type, Type>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="N"></typeparam>
        /// <param name="oldView"></param>
        /// <param name="newView"></param>
        /// <returns></returns>
        //public static N Render<N>(VxView oldView, VxView newView)
        //{
        //    if (oldView.GetType() == newView.GetType())
        //    {
                
        //    }
        //    else
        //    {
        //        Activator.CreateInstance(Mappings[newView.GetType()], new object[] { newView });
        //    }
        //}
    }
}
