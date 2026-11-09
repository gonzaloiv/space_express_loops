using System;
using UnityEngine;

namespace DigitalLove.Game.Placement
{
    // Where the field goes relative to a seated player, in metres. The head gives direction and
    // distance; resting hands give the height (they find the desk).
    [Serializable]
    public struct SeatSettings
    {
        [Tooltip("Field distance in front of the head, along its yaw.")]
        public float distance;
        [Tooltip("Field height below the eyes when no hand is resting in front.")]
        public float defaultDrop;
        [Tooltip("Field height above resting hands.")]
        public float aboveHands;
        [Tooltip("The field never sits higher than this below the eyes.")]
        public float minDrop;
        [Tooltip("The field never sits lower than this below the eyes.")]
        public float maxDrop;

        [Header("A resting hand")]
        [Tooltip("How far in front of the head a resting hand can be (min, max).")]
        public Vector2 restReach;
        [Tooltip("How far to the side of the head a resting hand can be.")]
        public float restSide;
        [Tooltip("How far below the eyes a resting hand can be (min, max).")]
        public Vector2 restDrop;

        public static SeatSettings Default => new()
        {
            distance = 0.45f,
            defaultDrop = 0.15f,
            aboveHands = 0.25f,
            minDrop = 0.05f,
            maxDrop = 0.4f,
            restReach = new Vector2(0.1f, 0.7f),
            restSide = 0.5f,
            restDrop = new Vector2(0.25f, 0.8f),
        };
    }
}
