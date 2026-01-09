/*
 * Copyright (c). 2026 Daniel Patterson, MCSD (danielanywhere).
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 * 
 */

using System.Text;

namespace SolutionModeler
{
	//*-------------------------------------------------------------------------*
	//*	ProgramUtil																															*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// ProgramUtil
	/// </summary>
	public class ProgramUtil
	{
		//*************************************************************************
		//*	Private																																*
		//*************************************************************************
		//*************************************************************************
		//*	Protected																															*
		//*************************************************************************
		//*************************************************************************
		//*	Public																																*
		//*************************************************************************
		//*-----------------------------------------------------------------------*
		//* Clear																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Clear the contents of the specified string builder.
		/// </summary>
		/// <param name="builder">
		/// Reference to the string builder to clear.
		/// </param>
		public static void Clear(StringBuilder builder)
		{
			if(builder?.Length > 0)
			{
				builder.Remove(0, builder.Length);
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	RightOf																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the portion of the caller's string to the right of the last
		/// instance of the specified pattern.
		/// </summary>
		/// <param name="value">
		/// The string value to inspect.
		/// </param>
		/// <param name="pattern">
		/// The pattern to find.
		/// </param>
		/// <returns>
		/// The portion of the string to the right of the last instance of the
		/// specified pattern, if found. Otherwise, the original string if
		/// the caller's value was non-null. Otherwise, an empty string.
		/// </returns>
		public static string RightOf(string value, string pattern)
		{
			string result = "";

			if(value?.Length > 0)
			{
				result = value;
				if(pattern?.Length > 0)
				{
					if(value.LastIndexOf(pattern) > -1)
					{
						result =
							value.Substring(value.LastIndexOf(pattern) + pattern.Length);
					}
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*


	}
	//*-------------------------------------------------------------------------*

}
