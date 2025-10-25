namespace Crolow.FastDico.Common.Enums
{
    public enum MessageType
    {
        Login = 0,
        Logout = 1,

        UpdateRoom = 12,
        GameStarted = 14,

        Change = 21,
        Pass = 22,
        Pick = 23,
        BoardRackSelected = 24,
        BoardRackSelectNext = 25,
        BoardRackSelectPrevious = 26,
        RoundIsPlayed = 27,
        SetNextPlayer = 28,
        PickOnly = 29,
        RobotPlay = 30,
    }
}
