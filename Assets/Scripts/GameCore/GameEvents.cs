using System;
/*
 GameEvents.cs — Lightweight Event Hub (Observer / Event-Bus Pattern)
 --------------------------------------------------------------------
 Static delegate fields act as a central “message broker” so gameplay
 systems can publish and subscribe without hard references:

   • Player → (OnPlayerDied) → GameManager, UI, Audio
   • Key pickup → (OnKeyCollected) → HUD counter
   • Door logic → (OnDoorOpened string) → GameManager scene flow
   • UI button → (OnInstructionRequested) → SceneLoader

 This Observer/Event-Bus approach keeps components loosely coupled and
 lets you add or remove listeners (e.g., achievements, analytics) without
 modifying the emitters.
*/

namespace GameCore
{
    public static class GameEvents
    {
        /// <summary>Triggered when player dies</summary>
        public static Action OnPlayerDied;

        /// <summary>Triggered when key is collected</summary>
        public static Action OnKeyCollected;

        /// <summary>Triggered when door is opened</summary>
        // public static Action OnDoorOpened;

        public static Action<string> OnDoorOpened;   

        /* Triggered when the main menu requests Instructions, no parameters */
        public static Action OnInstructionRequested;     
    }
}
