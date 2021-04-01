#region File and License Information
/*
<File>
	<License Type="BSD">
		Copyright © 2009 - 2016, Outcoder. All rights reserved.
	
		This file is part of Calcium (http://calciumsdk.net).

		Redistribution and use in source and binary forms, with or without
		modification, are permitted provided that the following conditions are met:
			* Redistributions of source code must retain the above copyright
			  notice, this list of conditions and the following disclaimer.
			* Redistributions in binary form must reproduce the above copyright
			  notice, this list of conditions and the following disclaimer in the
			  documentation and/or other materials provided with the distribution.
			* Neither the name of the <organization> nor the
			  names of its contributors may be used to endorse or promote products
			  derived from this software without specific prior written permission.

		THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
		ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
		WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
		DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
		DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
		(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
		LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
		ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
		(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
		SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
	</License>
	<Owner Name="Daniel Vaughan" Email="danielvaughan@outcoder.com" />
	<CreationDate>$CreationDate$</CreationDate>
</File>
*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace BareMvvm.Core.Bindings
{
	internal class TypeUtility
	{
		/// <summary>
		/// Retrieves a list of types deriving from the specified type, 
		/// and including the specified type.
		/// Based on code by Thomas Lebrun http://bit.ly/1OQsD8L
		/// </summary>
		/// <typeparam name="TDerivingFrom">The type to match.</typeparam>
		/// <returns>A list of types deriving from the specified type.</returns>
		internal static IEnumerable<Type> GetTypes<TDerivingFrom>()
		{
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(i => !IsSystemAssembly(i.GetName().Name));

            List<Type> result = new List<Type>();

            foreach (var assembly in assemblies)
            {
                foreach (var type in GetTypes<TDerivingFrom>(assembly))
                {
                    result.Add(type);
                }
            }

            return result;
		}

        internal static IEnumerable<Type> GetTypes<TDerivingFrom>(Assembly assembly)
        {
            var assemblyTypes = assembly.GetTypes();

            var types = assemblyTypes.Where(t => typeof(TDerivingFrom).IsAssignableFrom(t)).ToList();
            foreach (var type in types)
            {
                if (!type.IsInterface && !type.IsAbstract)
                {
                    yield return type;
                }
            }
        }

		static bool IsSystemAssembly(string assemblyName)
		{
			return string.Equals(assemblyName, "Mono.Android", StringComparison.InvariantCultureIgnoreCase)
			|| string.Equals(assemblyName, "mscorlib", StringComparison.InvariantCultureIgnoreCase)
			|| string.Equals(assemblyName, "System", StringComparison.InvariantCultureIgnoreCase)
			|| string.Equals(assemblyName, "System.Core", StringComparison.InvariantCultureIgnoreCase)
			|| string.Equals(assemblyName, "System.Xml", StringComparison.InvariantCultureIgnoreCase)
			|| string.Equals(assemblyName, "System.Xml.Linq", StringComparison.InvariantCultureIgnoreCase)
			|| assemblyName.StartsWith("Xamarin.Android.Support", StringComparison.OrdinalIgnoreCase);
		}
	}
}