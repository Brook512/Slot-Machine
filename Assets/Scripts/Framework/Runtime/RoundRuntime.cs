namespace Framework.Runtime
{
    public class RoundRuntime
    {
        public int ResultSectorIndex;
        public string ResultSymbolId;
        public int RewardPoints;
        public bool IsWin;

        public void Reset()
        {
            ResultSectorIndex = -1;
            ResultSymbolId = string.Empty;
            RewardPoints = 0;
            IsWin = false;
        }
    }
}