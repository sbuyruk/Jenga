namespace Jenga.Models.MTS
{
    public class KatilimciEqualityComparer : IEqualityComparer<Katilimci>
    {
        public bool Equals(Katilimci x, Katilimci y)
        {
            return x.TCKimlikNo != 0 && x.TCKimlikNo == y.TCKimlikNo;
        }

        public int GetHashCode(Katilimci obj)
        {
            return obj.TCKimlikNo.GetHashCode();
        }
    }
}
