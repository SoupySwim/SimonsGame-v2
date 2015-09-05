using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	// An interface to define that an object could be interactable.
	public interface IInteractable
	{
		// character will be the object that will interact with the IInteractable Object.
		// The character will chose to interact with the current object which will call this function.
		void InteractWith(PhysicsObject character);
	}
}
