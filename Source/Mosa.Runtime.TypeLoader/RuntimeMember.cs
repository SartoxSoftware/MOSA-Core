﻿/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Michael Ruck (grover) <sharpos@michaelruck.de>
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Mosa.Runtime.Metadata.Loader;

namespace Mosa.Runtime.TypeLoader
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class RuntimeMember : RuntimeObject, IRuntimeAttributable
	{
		#region Data members

		/// <summary>
		/// Holds the attributes of the member.
		/// </summary>
		private RuntimeAttribute[] attributes;

		/// <summary>
		/// Specifies the type, that declares the member.
		/// </summary>
		private RuntimeType declaringType;

		/// <summary>
		/// Holds the (cached) name of the type.
		/// </summary>
		private string name;

		#endregion // Data members

		#region Construction

		/// <summary>
		/// Initializes a new instance of <see cref="RuntimeMember"/>.
		/// </summary>
		/// <param name="token">Holds the token of this runtime metadata.</param>
		/// <param name="declaringType">The declaring type of the member.</param>
		/// <param name="attributes">Holds the attributes of the member.</param>
		protected RuntimeMember(int token, RuntimeType declaringType, RuntimeAttribute[] attributes) :
			base(token)
		{
			this.declaringType = declaringType;
			this.attributes = attributes;
		}

		#endregion // Construction

		#region Properties

		/// <summary>
		/// Retrieves the declaring type of the member.
		/// </summary>
		public RuntimeType DeclaringType
		{
			get { return this.declaringType; }
			internal set { this.declaringType = value; }
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return this.name; }
			protected set
			{
				if (value == null)
					throw new ArgumentNullException(@"value");
				if (this.name != null)
					throw new InvalidOperationException();

				this.name = value;
			}
		}

		#endregion // Properties

		#region Methods

		#endregion // Methods

		#region IRuntimeAttributable Members

		/// <summary>
		/// Gets the custom attributes.
		/// </summary>
		/// <value>The custom attributes.</value>
		public RuntimeAttribute[] CustomAttributes
		{
			get
			{
				return this.attributes;
			}
		}

		/// <summary>
		/// Determines if the given attribute type is applied.
		/// </summary>
		/// <param name="attributeType">The type of the attribute to check.</param>
		/// <returns>
		/// The return value is true, if the attribute is applied. Otherwise false.
		/// </returns>
		public bool IsDefined(RuntimeType attributeType)
		{
			bool result = false;
			if (this.attributes != null)
			{
				foreach (RuntimeAttribute attribute in this.attributes)
				{
					if (attribute.Type.Equals(attributeType) == true ||
						attribute.Type.IsSubclassOf(attributeType) == true)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Returns an array of custom attributes identified by RuntimeType.
		/// </summary>
		/// <param name="attributeType">Type of attribute to search for. Only attributes that are assignable to this type are returned.</param>
		/// <returns>An array of custom attributes applied to this member, or an array with zero (0) elements if no matching attributes have been applied.</returns>
		public object[] GetCustomAttributes(RuntimeType attributeType)
		{
			List<object> result = new List<object>();
			if (this.attributes != null)
			{
				foreach (RuntimeAttribute attribute in this.attributes)
				{
					if (attributeType.IsAssignableFrom(attribute.Type))
						result.Add(attribute.GetAttribute());
				}
			}

			return result.ToArray();
		}

		/// <summary>
		/// Sets the attributes of this member.
		/// </summary>
		/// <param name="attributes">The attributes.</param>
		internal void SetAttributes(RuntimeAttribute[] attributes)
		{
			if (this.attributes != null)
				throw new InvalidOperationException(@"Can't set attributes twice.");

			this.attributes = attributes;
		}

		#endregion // IRuntimeAttributable Members
	}
}
