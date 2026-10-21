namespace DigitalLove.Game.Spaceships
{
    public static class SpaceshipsSpawnerExtensions
    {
        public static void SetBubbleView(this SpaceshipsSpawner spawner, bool isBubble)
        {
            for (int i = 0; i < spawner.All.Count; i++)
            {
                SpaceshipBehaviour spaceship = spawner.All[i];
                if (spaceship != null && spaceship.IsActive)
                    spaceship.SetBubbleView(isBubble);
            }
        }

        public static bool IsAnyRouteEditing(this SpaceshipsSpawner spawner)
        {
            for (int i = 0; i < spawner.All.Count; i++)
            {
                SpaceshipBehaviour spaceship = spawner.All[i];
                if (spaceship != null && spaceship.IsActive && spaceship.IsRouteEditing)
                    return true;
            }

            return false;
        }
    }
}
