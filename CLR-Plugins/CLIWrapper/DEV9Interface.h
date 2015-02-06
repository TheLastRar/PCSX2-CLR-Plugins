/* 
*  Copyright (C) 2002-2010  PCSX2 Dev Team
*
*  PCSX2 is free software: you can redistribute it and/or modify it under the terms
*  of the GNU Lesser General Public License as published by the Free Software Found-
*  ation, either version 3 of the License, or (at your option) any later version.
*
*  PCSX2 is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
*  without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR
*  PURPOSE.  See the GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License along with PCSX2.
*  If not, see <http://www.gnu.org/licenses/>.
*/

#ifndef __DEV9_H__
#define __DEV9_H__

#include "PSEInterface.h"
#include "DEV9Wrapper.h"

class NativeDEV9Wrapper
{
public:
	msclr::auto_gcroot<DEV9Wrapper^> DEV9wrap;
};
extern NativeDEV9Wrapper* nat_dev9;

//extern u8 *ram;

#endif