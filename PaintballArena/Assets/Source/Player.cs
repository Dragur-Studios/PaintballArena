public class Player : iGameStateListener
{
    public bool isPaused { get; private set; } = false;
    public bool isAlive { get; private set; } = true;


    public override void HandleGameOver()
    {
        isAlive = false;
    }

    public override void HandleGamePaused()
    {
        isPaused = true;
    }

    public override void HandleGameUnpaused()
    {
        isPaused = false;
    }
}
