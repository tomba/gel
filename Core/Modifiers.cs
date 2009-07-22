using System;

namespace Gel.Core
{
	[Flags]
	public enum Modifiers
	{
		None		= 0,
		Public		= 1<<1,
		Protected	= 1<<2,
		Private		= 1<<3,
		Internal	= 1<<4,

		AccessMask	= Public | Protected | Private | Internal,

		Const		= 1<<5,
		Static		= 1<<6,
		Override	= 1<<7,
		Sealed		= 1<<8,
		Virtual		= 1<<9,
		Readonly	= 1<<10,
		New			= 1<<11,
		Abstract	= 1<<12,

	}

}
