namespace Tests
{
    public enum MoveType
    {
        Run,
        Jump,
        WallJump
    }
    public struct EdgeData { }
    public struct NumEdgeData
    {
        public int testNum;
    }
    public struct MoveTypeEdgeData
    {
        public MoveType moveType;
    }
}